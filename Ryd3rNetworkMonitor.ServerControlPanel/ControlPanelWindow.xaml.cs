using Ryd3rNetworkMonitor.Library;
using Ryd3rNetworkMonitor.ServerControlPanel.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    public partial class ControlPanelWindow : Window
    {
        private bool? ServerStatus { get; set; }

        private int logMaxLines;        
        private int offlineTimeCounter;
        private StringBuilder logAdditionStr;
        private BackgroundWorker bgw;
        private DispatcherTimer timer;
        private MessageServer mesServer;
        private DbServer dbServer;
        private List<Host> dbHosts;
        private Settings settings;

        public ControlPanelWindow()
        {
            InitializeComponent();

            ServerStatus = false;

            settings = new Settings();
            settings.GetSettings();

            logMaxLines = settings.LogMaxLines;
            mesServer = new MessageServer(settings.Ip, settings.MesPort);

            DbServer.ConnStr = ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString;
            dbServer = new DbServer();
            dbServer.Start();

            dbHosts = new List<Host>();

            if (DbServer.DbGetHosts() != null)
            {
                dbHosts = DbServer.DbGetHosts();
                mesServer.SetDbHosts(dbHosts);
            }
            else
            {
                dbServer.Start();
            }
            
            if (Properties.Settings.Default.TableView)
            {
                tableBtn.IsChecked = true;
                listBtn.IsChecked = false;
                SetTableView();
            }
            else
            {
                if (Properties.Settings.Default.ListView)
                {
                    tableBtn.IsChecked = false;
                    listBtn.IsChecked = true;
                    SetListView();
                }
            }

            FoldersCheck(dbHosts);

            SettingsToWindowLog(settings);
        }

        private void FoldersCheck(List<Host> hosts)
        {
            try
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Logs"))
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Logs");

                if (hosts != null && hosts.Count > 0)
                {
                    for (int i = 0; i < hosts.Count; i++)
                    {
                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{hosts[i].HostId}"))
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{hosts[i].HostId}");

                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{hosts[i].HostId}\\Log"))
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{hosts[i].HostId}\\Log");

                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{hosts[i].HostId}\\Screenshots"))
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{hosts[i].HostId}\\Screenshots");
                    }
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Folders check error"));
            }            
        }

        private void CreateFolders(Host host)
        {
            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Logs"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}");
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}\\Log");
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}\\Screenshots");
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Folders creation error"));
            }
        }

        private void DeleteFolders(Host host)
        {
            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Logs"))
                {
                    DirectoryInfo[] dirs = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{ host.HostId }").GetDirectories();
                    
                    foreach (var dir in dirs)
                    {
                        if (dir.GetFiles() != null)
                        {
                            foreach(var file in dir.GetFiles())
                            {
                                file.Delete();
                            }
                        }

                        dir.Delete();
                    }

                    DirectoryInfo mainHostDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}");
                    mainHostDir.Delete();
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Folders delete error"));
            }
        }

        private void SaveLogAndPics(string hostId)
        {
            var hostsPanel = (Panel)hostsScroll.Content;

            try
            {
                if (hostId != null && hostId != string.Empty)
                {
                    if (hostsPanel.Children.Count > 0)
                    {
                        for (int i = 0; i < hostsPanel.Children.Count; i++)
                        {
                            if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                            {
                                if (((HostControl)hostsPanel.Children[i]).Host.HostId == hostId)
                                {
                                    HostControl hst = (HostControl)hostsPanel.Children[i];

                                    foreach (HostMessage mes in hst.Messages)
                                    {
                                        string newLine = string.Empty;

                                        switch (mes.InnerMessage.Type)
                                        {
                                            case InnerMessageTypes.Exit:
                                                newLine = ConstructLogString(mes.MessageTime, $"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' offline");
                                                break;
                                            case InnerMessageTypes.Online:
                                                break;
                                            case InnerMessageTypes.Registration:
                                                newLine = ConstructLogString(mes.MessageTime, $"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' was registered in system");
                                                break;
                                            case InnerMessageTypes.RegistrationCheck:
                                                newLine = ConstructLogString(mes.MessageTime, $"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' was checked and online now");
                                                break;
                                            case InnerMessageTypes.Screenshot:
                                                SaveScreenshot(mes.InnerMessage.ImageBytes, mes.Host.HostId, mes.MessageTime);
                                                newLine = ConstructLogString(mes.MessageTime, $"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' sended screenshot");
                                                break;
                                            case InnerMessageTypes.SettingsChanged:
                                                newLine = ConstructLogString(mes.MessageTime, $"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' has changed settings");
                                                break;
                                            case InnerMessageTypes.Timeout:
                                                newLine = ConstructLogString(mes.MessageTime, $"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' offline because of timeout");
                                                break;
                                        }

                                        string logFile = AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{mes.Host.HostId}\\Log\\{mes.MessageTime.Date.Day}-{mes.MessageTime.Date.Month}-{mes.MessageTime.Date.Year}.txt";

                                        if (!File.Exists(logFile))
                                        {
                                            using (StreamWriter sw = File.CreateText(logFile))
                                            {
                                                if (newLine != string.Empty)
                                                    sw.WriteLine(newLine);
                                            }
                                        }
                                        else
                                        {
                                            using (StreamWriter sw = File.AppendText(logFile))
                                            {
                                                if (newLine != string.Empty)
                                                    sw.WriteLine(newLine);
                                            }
                                        }
                                    }

                                    hst.Messages.Clear();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Save logs error"));
            }
        }

        private void SaveServerLog()
        {
            if (logTxt.Text != string.Empty)
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\Server"))
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\Server");

                string logFile = AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\Server\\{DateTime.Now.Date.Day}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Year}.txt";

                if (!File.Exists(logFile))
                {
                    using (StreamWriter sw = File.CreateText(logFile))
                    {
                        if (logAdditionStr != null && logAdditionStr.Length > 0)
                            sw.WriteLine(logAdditionStr);

                        for (int i = 0; i < logTxt.LineCount; i++)
                        {
                            if (logTxt.GetLineText(i) != string.Empty && !String.IsNullOrWhiteSpace(logTxt.GetLineText(i)))
                                sw.WriteLine(logTxt.GetLineText(i));
                        }
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(logFile))
                    {
                        if (logAdditionStr != null && logAdditionStr.Length > 0)
                            sw.WriteLine(logAdditionStr);

                        for (int i = 0; i < logTxt.LineCount; i++)
                        {
                            if (logTxt.GetLineText(i) != string.Empty && !String.IsNullOrWhiteSpace(logTxt.GetLineText(i)))
                                sw.WriteLine(logTxt.GetLineText(i));
                        }
                    }
                }

                logAdditionStr = new StringBuilder();
                logTxt.AppendText(ConstructLogString("Server log saved"));
            }
        }

        private void ClientsChecking()
        {
            offlineTimeCounter = 0;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, settings.MessageCheckTime);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void SaveScreenshot(byte[] imBytes, string hostId, DateTime dt)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(imBytes))
                {
                    BitmapImage bim = new BitmapImage();
                    bim.BeginInit();
                    bim.StreamSource = ms;
                    bim.CacheOption = BitmapCacheOption.OnLoad;
                    bim.EndInit();

                    if (bim != null)
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bim));
                        encoder.QualityLevel = 80;

                        var folderPath = AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{hostId}\\Screenshots\\{dt.ToShortDateString().Replace('.', '-')}";
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        var filePath = folderPath + $"\\{dt.ToLongTimeString().Replace(':', '-')}.jpg";
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }
                    }
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Screenshot save error"));
            }
        }

        private DateTime? GetHostStartTime(HostControl hst = null, HostLineControl hstl = null)
        {
            DateTime? dt = null;

            if (hst != null)
            {
                if (hst.Messages != null && hst.Messages.Count > 0)
                    dt = hst.Messages.Find(x => x.InnerMessage.Type == InnerMessageTypes.RegistrationCheck).MessageTime;
                
            }
            else if (hstl != null)
            {
                if (hst.Messages != null && hst.Messages.Count > 0)
                    dt = hst.Messages.Find(x => x.InnerMessage.Type == InnerMessageTypes.RegistrationCheck).MessageTime;

            }

            return dt;
        } 

        private void DbUpdateHostWorkTime(HostMessage mes, DateTime stTime)
        {
            int secondsToAdd = (int)(mes.MessageTime - stTime).TotalSeconds;
            DbServer.DBAddWholeTime(mes.Host.HostId, mes.MessageTime, secondsToAdd);
            DbServer.DbAddInteraval(mes.Host.HostId, stTime, mes.MessageTime);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var hostsPanel = (Panel)hostsScroll.Content;

            try
            {
                if (mesServer.Messages != null && mesServer.Messages.Count > 0)
                {
                    for (int i = 0; i < mesServer.Messages.Count; i++)
                    {
                        HostMessage mes = mesServer.Messages[i];

                        if (mes != null)
                        {
                            for (int j = 0; j < hostsPanel.Children.Count; j++)
                            {
                                if (hostsPanel.Children[j].GetType() == typeof(HostControl))
                                {
                                    HostControl hst = (HostControl)hostsPanel.Children[j];
                                    if (hst.Host.HostId == mesServer.Messages[i].Host.HostId)
                                    {
                                        hst.Host.LastOnline = mesServer.Messages[i].MessageTime;
                                        hst.Messages.Add(mesServer.Messages[i]);

                                        if (mes.InnerMessage.Type == InnerMessageTypes.RegistrationCheck)
                                        {
                                            hst.DbIntervalStart = mes.MessageTime;
                                            hst.DbIntervalCount += 1;
                                        }
                                        else if (mes.InnerMessage.Type == InnerMessageTypes.Online)
                                        {
                                            hst.DbIntervalCount += 1;
                                            if (hst.DbIntervalCount >= 3)
                                                hst.DbIntervalEnd = mes.MessageTime;
                                        }

                                        else if (mes.InnerMessage.Type == InnerMessageTypes.Exit)
                                        {
                                            hst.DbIntervalEnd = mes.MessageTime;
                                            hst.DbIntervalCount += 1;                                            

                                            if (GetHostStartTime(hst, null) != null)
                                                DbUpdateHostWorkTime(mes, (DateTime)GetHostStartTime(hst, null));
                                            else
                                                logTxt.AppendText($"\n{ConstructLogString($"Error getting start time for HostID '{mes.Host.HostId}'")}");
                                        }                                   
                                    }
                                }
                                else if (hostsPanel.Children[j].GetType() == typeof(HostLineControl))
                                {
                                    HostLineControl hst = (HostLineControl)hostsPanel.Children[j];
                                    if (hst.Host.HostId == mesServer.Messages[i].Host.HostId)
                                    {
                                        hst.Host.LastOnline = mesServer.Messages[i].MessageTime;
                                        hst.Messages.Add(mesServer.Messages[i]);

                                        if (mes.InnerMessage.Type == InnerMessageTypes.RegistrationCheck)
                                        {
                                            hst.DbIntervalStart = mes.MessageTime;
                                            hst.DbIntervalCount += 1;
                                        }
                                        else if (mes.InnerMessage.Type == InnerMessageTypes.Online)
                                        {
                                            hst.DbIntervalCount += 1;
                                            if (hst.DbIntervalCount >= 10)
                                                hst.DbIntervalEnd = mes.MessageTime;
                                        }

                                        else if (mes.InnerMessage.Type == InnerMessageTypes.Exit)
                                        {
                                            hst.DbIntervalEnd = mes.MessageTime;
                                            hst.DbIntervalCount += 1;

                                            if (GetHostStartTime(null, hst) != null)
                                                DbUpdateHostWorkTime(mes, (DateTime)GetHostStartTime(null, hst));
                                            else
                                                logTxt.AppendText($"\n{ConstructLogString($"Error getting start time for HostID '{mes.Host.HostId}'")}");
                                        }
                                    }
                                }
                            }

                            if (mes.InnerMessage != null)
                            {
                                Host hostToUpdate = mesServer.Hosts.Find(x => x.HostId == mes.Host.HostId);
                                hostToUpdate.LastOnline = DateTime.Now;

                                switch (mes.InnerMessage.Type)
                                {
                                    case InnerMessageTypes.Exit:
                                        mesServer.SetHostState(mes.Host.HostId, false);
                                        DbServer.DbUpdateLastOnline(hostToUpdate);
                                        SaveLogAndPics(mes.Host.HostId);
                                        logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' offline")}");
                                        break;

                                    case InnerMessageTypes.Online:
                                        mesServer.SetHostState(mes.Host.HostId, true);
                                        if (settings.MessageDisplay)
                                            logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' online")}");
                                        break;

                                    case InnerMessageTypes.Registration:
                                        mesServer.SetDbHosts(DbServer.DbGetHosts());
                                        mesServer.SetHostState(mes.Host.HostId, false);
                                        CreateFolders(mes.Host);

                                        if (Properties.Settings.Default.TableView)
                                            SetTableView(mesServer.Hosts);
                                        else if (Properties.Settings.Default.ListView)
                                            SetListView(mesServer.Hosts);

                                        logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' was registered in system")}");
                                        break;

                                    case InnerMessageTypes.RegistrationCheck:
                                        mesServer.SetHostState(mes.Host.HostId, true);
                                        logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' was checked and online now")}");
                                        break;

                                    case InnerMessageTypes.Screenshot:
                                        mesServer.SetHostState(mes.Host.HostId, true);
                                        logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' sended screenshot")}");
                                        break;

                                    case InnerMessageTypes.SettingsChanged:
                                        mesServer.SetHostState(mes.Host.HostId, true);
                                        logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' has changed settings")}");
                                        break;
                                    case InnerMessageTypes.Timeout:
                                        mesServer.SetHostState(mes.Host.HostId, false);
                                        DbServer.DbUpdateLastOnline(hostToUpdate);
                                        SaveLogAndPics(mes.Host.HostId);
                                        logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' offline because of timeout")}");
                                        break;
                                }
                            }
                        }
                    }

                    mesServer.Messages.Clear();
                }

                if (offlineTimeCounter < settings.HostOfflineCheckTime)
                    offlineTimeCounter += Properties.Settings.Default.MessagesCheckTime;
                else
                {
                    for (int i = 0; i < hostsPanel.Children.Count; i++)
                    {
                        if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                        {
                            HostControl hst = (HostControl)hostsPanel.Children[i];
                            if (hst.HostState)
                            {
                                var tt = hst.Host.LastOnline;
                                tt = tt.AddSeconds(Properties.Settings.Default.HostOfflineCheckTime);

                                if (DateTime.Now > tt)
                                {
                                    var newMes = new HostMessage(hst.Host, new InnerMessage(InnerMessageTypes.Timeout, null, null, null, null));
                                    hst.Messages.Add(newMes);

                                    if (GetHostStartTime(hst, null) != null)
                                        DbUpdateHostWorkTime(newMes, (DateTime)GetHostStartTime(hst, null));
                                    else
                                        logTxt.AppendText($"\n{ConstructLogString($"Error getting start time for HostID '{newMes.Host.HostId}'")}");

                                    SaveLogAndPics(hst.Host.HostId);
                                    hst.HostState = false;
                                    logTxt.AppendText($"\n{ConstructLogString($"Host '{hst.Host.Ip}' with HostID '{hst.Host.HostId}' offline because of timeout")}");
                                    continue;
                                }
                            }
                        }
                        else if (hostsPanel.Children[i].GetType() == typeof(HostLineControl))
                        {
                            HostLineControl hst = (HostLineControl)hostsPanel.Children[i];
                            if (hst.HostState)
                            {
                                var tt = hst.Host.LastOnline;
                                tt = tt.AddSeconds(Properties.Settings.Default.HostOfflineCheckTime);

                                if (DateTime.Now > tt)
                                {
                                    var newMes = new HostMessage(hst.Host, new InnerMessage(InnerMessageTypes.Timeout, null, null, null, null));
                                    hst.Messages.Add(newMes);

                                    if (GetHostStartTime(null, hst) != null)
                                        DbUpdateHostWorkTime(newMes, (DateTime)GetHostStartTime(null, hst));
                                    else
                                        logTxt.AppendText($"\n{ConstructLogString($"Error getting start time for HostID '{newMes.Host.HostId}'")}");

                                    SaveLogAndPics(hst.Host.HostId);
                                    hst.HostState = false;
                                    logTxt.AppendText($"\n{ConstructLogString($"Host '{hst.Host.Ip}' with HostID '{hst.Host.HostId}' offline because of timeout")}");
                                    continue;
                                }
                            }
                        }
                    }

                    offlineTimeCounter = 0;
                }

                UpdateHostControlInterface();
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Messaging error"));
            }
        }

        private void RestartServer()
        {
            StopServer();
            startServerBtn_Click(null, null);
        }

        private void StopServer()
        {
            try
            {
                if (mesServer != null && mesServer.ServerState)
                {
                    if (mesServer.Stop())
                    {
                        timer.Stop();
                        timer = null;

                        if (bgw != null)
                        {
                            bgw.CancelAsync();
                            bgw.Dispose();
                        }

                        if ((bool)ServerStatus)
                        {
                            ServerStatus = false;
                            logTxt.AppendText($"{ConstructLogString("Server stopped")}\n");
                        }
                    }
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Stop server error"));
            }
        }

        private void UpdateHostControlInterface()
        {
            var hostsPanel = (Panel)hostsScroll.Content;
            for (int i = 0; i < hostsPanel.Children.Count; i++)
            {
                if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                {
                    HostControl host = (HostControl)hostsPanel.Children[i];
                    host.UpdateInterface(settings.IpDisplay, settings.LoginDisplay);

                    if (host.Messages != null && host.Messages.Count > 0)
                    {
                        int mCount = host.Messages.Count;
                        if (host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Online ||
                            host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.RegistrationCheck ||
                            host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Screenshot ||
                            host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.SettingsChanged)
                            host.HostState = true;
                        else if (host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Exit ||
                                 host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Timeout)
                            host.HostState = false;
                    }
                    else
                        host.HostState = false;
                }
                else if (hostsPanel.Children[i].GetType() == typeof(HostLineControl))
                {
                    HostLineControl host = (HostLineControl)hostsPanel.Children[i];

                    if (host.Messages != null && host.Messages.Count > 0)
                    {
                        int mCount = host.Messages.Count;
                        if (host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Online ||
                            host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.RegistrationCheck ||
                            host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Screenshot ||
                            host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.SettingsChanged)
                            host.HostState = true;
                        else if (host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Exit ||
                                 host.Messages[mCount - 1].InnerMessage.Type == InnerMessageTypes.Timeout)
                            host.HostState = false;
                    }
                    else
                        host.HostState = false;
                }
            }
        }

        private void ShowHostInfoWindow()
        {
            var hostsPanel = (Panel)hostsScroll.Content;

            for (int i = 0; i < hostsPanel.Children.Count; i++)
            {
                if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                {
                    HostControl hostCtrl = (HostControl)hostsPanel.Children[i];
                    if ((Host)hostCtrl.Tag != null)
                    {
                        Host host = (Host)hostCtrl.Tag;
                        HostInfoWindow hostInfo = new HostInfoWindow(host, hostCtrl.Messages);
                        hostInfo.Owner = this;
                        hostInfo.applyBtn.Click += ApplyInfoBtn_Click;
                        hostInfo.ShowDialog();

                        hostCtrl.Tag = null;
                        break;
                    }
                }
                else if (hostsPanel.Children[i].GetType() == typeof(HostLineControl))
                {
                    HostLineControl hostCtrl = (HostLineControl)hostsPanel.Children[i];
                    if ((Host)hostCtrl.Tag != null)
                    {
                        Host host = (Host)hostCtrl.Tag;
                        HostInfoWindow hostInfo = new HostInfoWindow(host, hostCtrl.Messages);
                        hostInfo.Owner = this;
                        hostInfo.applyBtn.Click += ApplyInfoBtn_Click;
                        hostInfo.ShowDialog();

                        hostCtrl.Tag = null;
                        break;
                    }
                }
            }
        }

        private void Host_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowHostInfoWindow();
        }

        private void InfoBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowHostInfoWindow();
        }

        private void ApplyInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            var hostsPanel = (WrapPanel)hostsScroll.Content;
            Button apply = (Button)sender;

            if (apply.Tag != null)
            {
                Host host = (Host)apply.Tag;

                for (int i = 0; i < hostsPanel.Children.Count; i++)
                {
                    if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                    {
                        HostControl hostCtrl = (HostControl)hostsPanel.Children[i];
                        if (hostCtrl.Host.HostId == host.HostId)
                        {
                            hostCtrl.UpdateHostInfo(host);
                            break;
                        }
                    }
                }

                logTxt.Text += ConstructLogString($"Host with HostID '" + host.HostId + "' was updated");
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var hostsPanel = (Panel)hostsScroll.Content;
                var res = MessageBox.Show("Do really want to delete this host?\nAll data will be deleted", "Warning", MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.OK)
                {
                    Host hostToDelete = null;

                    for (int i = 0; i < hostsPanel.Children.Count; i++)
                    {
                        if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                        {
                            HostControl host = (HostControl)hostsPanel.Children[i];
                            if ((Host)host.Tag != null)
                            {
                                hostToDelete = (Host)host.Tag;
                                hostsPanel.Children.Remove(host);
                                break;
                            }
                        }

                        else if (hostsPanel.Children[i].GetType() == typeof(HostLineControl))
                        {
                            HostLineControl host = (HostLineControl)hostsPanel.Children[i];
                            if ((Host)host.Tag != null)
                            {
                                hostToDelete = (Host)host.Tag;
                                hostsPanel.Children.Remove(host);
                                break;
                            }
                        }
                    }

                    DbServer.DbDeleteHost(hostToDelete);
                    dbHosts = DbServer.DbGetHosts();

                    DeleteFolders(hostToDelete);

                    logTxt.Text += ConstructLogString($"Host with HostID '{hostToDelete.HostId}' was deleted");
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Host delete error"));
            }
        }        

        private string ConstructLogString(string str)
        {
            return $"\n{DateTime.Now.ToShortDateString()} | {DateTime.Now.ToLongTimeString()} | --- " + str;
        }

        private string ConstructLogString(DateTime dt, string str)
        {
            return $"{dt.ToShortDateString()} | {dt.ToLongTimeString()} | --- " + str;
        }

        private void SettingsToWindowLog(Settings stgs)
        {
            logTxt.AppendText($"{ConstructLogString($"Server IP: {stgs.Ip}")}");
            logTxt.AppendText($"{ConstructLogString($"Message port: {stgs.MesPort}")}\n");
        }

        private void SettingsToWindowLog(Settings newSettings, Settings oldSettings)
        {
            if (!newSettings.Equals(oldSettings))
            {
                if (newSettings.Ip != oldSettings.Ip)
                    logTxt.AppendText($"{ConstructLogString($"Server IP changed: {newSettings.Ip}")}");

                if (newSettings.MesPort != oldSettings.MesPort)
                    logTxt.AppendText($"{ConstructLogString($"Message server port changed: {newSettings.MesPort}")}");

                logTxt.Text += "\n";
            }
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = this;
            settings.applyBtn.Click += ApplyBtn_Click;
            settings.ShowDialog();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings newSettings = new Settings();
            newSettings.GetSettings();           

            SettingsToWindowLog(newSettings, settings);

            if (!newSettings.Equals(settings))
                settings = newSettings;

            stopServerBtn_Click(null, null);

            UpdateHostControlInterface();
        }

        private void startServerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mesServer != null && !mesServer.ServerState)
            {
                if (dbServer.Start())
                {
                    var dBgw = new BackgroundWorker();
                    dBgw.DoWork += DBgw_DoWork;
                    dBgw.RunWorkerCompleted += DBgw_RunWorkerCompleted;
                    dBgw.RunWorkerAsync();
                }
            }
        }

        private void DBgw_DoWork(object sender, DoWorkEventArgs e)
        {
            dbHosts = DbServer.DbGetHosts();
        }

        private void DBgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (mesServer.Start())
            {
                mesServer.SetDbHosts(dbHosts);

                bgw = new BackgroundWorker();
                bgw.WorkerSupportsCancellation = true;
                bgw.DoWork += Bgw_DoWork;
                bgw.RunWorkerAsync();

                logTxt.AppendText($"{ConstructLogString("Server started")}\n");
            }            

            if(dbHosts != null && dbHosts.Count > 0)
            {
                logTxt.AppendText($"{ConstructLogString($"Registered hosts number in DB: {dbHosts.Count}")}");

                for (int i = 0; i < dbHosts.Count; i++)                
                    logTxt.AppendText($"{ConstructLogString($"HostID '{dbHosts[i].HostId}' is offline")}");                
            }
            else
                logTxt.AppendText($"{ConstructLogString($"No registered hosts in DB. Count: 0")}");

            if (!(bool)ServerStatus)
                ServerStatus = true;

            ClientsChecking();
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (mesServer != null)
            {
                mesServer.StartListening();
            }

            else
            {
                bgw.CancelAsync();
                bgw.Dispose();
            }
        }

        private void stopServerBtn_Click(object sender, RoutedEventArgs e)
        {
            StopServer();

            var hostsPanel = (Panel)hostsScroll.Content;
            if (ServerStatus != null)
            {
                if (!(bool)ServerStatus)
                {
                    for (int i = 0; i < hostsPanel.Children.Count; i++)
                    {
                        if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                        {
                            HostControl hst = (HostControl)hostsPanel.Children[i];
                            hst.HostState = false;
                        }
                    }

                    ServerStatus = false;
                }
            }
        }

        private void logTxt_TextChanged(object sender, TextChangedEventArgs e)
        {          
            if (logTxt.LineCount > logMaxLines)
            {
                if (logAdditionStr == null)
                    logAdditionStr = new StringBuilder();

                for (int i=0; i<logTxt.LineCount - logMaxLines; i++)
                {
                    if (logTxt.GetLineText(i) != "")
                        logAdditionStr.AppendLine(logTxt.GetLineText(i));

                    logTxt.Text = logTxt.Text.Remove(0, logTxt.GetLineLength(i));
                }
            }

            logTxt.ScrollToEnd();
        }

        private void restartServerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ServerStatus != null && (bool)ServerStatus)
            {
                logTxt.AppendText($"{ConstructLogString("Servers restarting process started")}\n");
                RestartServer();
            }

            else            
                startServerBtn_Click(null, null);            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopServer();

            if (settings.SaveOnClose)
                SaveServerLog();
        }

        private List<HostControlTemplate> GetExistingHostFromPanel(List<Host> hosts = null)
        {
            //нужно собрать все объекты Host с панели и вернуть List<HostControlTemplate>
            List<HostControlTemplate> existingHosts = new List<HostControlTemplate>();

            if (hostsScroll.Content != null)
            {
                var hostsPanel = (Panel)hostsScroll.Content;
                if (hostsPanel.Children.Count > 0)
                {
                    for (int i = 0; i < hostsPanel.Children.Count; i++)
                    {
                        if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                        {
                            HostControl ctrl = (HostControl)hostsPanel.Children[i];
                            existingHosts.Add(new HostControlTemplate() { Host = ctrl.Host, Messages = ctrl.Messages, Screenshots = ctrl.Screenshots, CurrentImage = ctrl.CurrentImage });
                        }
                        else if (hostsPanel.Children[i].GetType() == typeof(HostLineControl))
                        {
                            HostLineControl ctrl = (HostLineControl)hostsPanel.Children[i];
                            existingHosts.Add(new HostControlTemplate() { Host = ctrl.Host, Messages = ctrl.Messages, Screenshots = ctrl.Screenshots, CurrentImage = ctrl.CurrentImage });
                        }
                    }

                }
                

                if (hosts != null && hosts.Count > 0)
                {
                    for (int i = 0; i < hosts.Count; i++)
                    {
                        if (existingHosts.Count > 0)
                        {
                            bool existingHost = false;

                            for (int j=0; j<existingHosts.Count; j++)
                            {

                                if (hosts[i].HostId == existingHosts[j].Host.HostId)
                                {
                                    existingHost = true;
                                    break;
                                }

                                else                                
                                    existingHost = false;
                                                              
                            }

                            if (!existingHost)
                                existingHosts.Add(new HostControlTemplate() { Host = hosts[i] });
                        }
                        else 
                            existingHosts.Add(new HostControlTemplate() { Host = hosts[i] });
                    }
                }
            }

            return existingHosts;
        }

        private void ChangePanel(bool isTable)
        {
            hostsScroll.Content = null;

            if (isTable)
            {
                titleGrid.ColumnDefinitions.Clear();
                titleGrid.Children.Clear();
                titleGrid.Children.Add(new Label() { Content = "Hosts Table", FontSize = 14, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });

                WrapPanel hostsPanel = new WrapPanel();
                hostsPanel.Orientation = Orientation.Horizontal;
                hostsPanel.VerticalAlignment = VerticalAlignment.Stretch;
                hostsPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                hostsScroll.Content = hostsPanel;
            }
            else
            {
                titleGrid.ColumnDefinitions.Clear();
                titleGrid.Children.Clear();

                for (int i=0; i<6; i++)
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    if (i == 0 || i == 4 || i == 5)
                        cd.Width = new GridLength(50, GridUnitType.Pixel);
                    else
                        cd.Width = new GridLength(1, GridUnitType.Star);

                    titleGrid.ColumnDefinitions.Add(cd);
                }

                for (int j=0; j<titleGrid.ColumnDefinitions.Count; j++)
                {
                    Label label = new Label();
                    label.VerticalAlignment = VerticalAlignment.Center;
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.FontSize = 14;

                    switch (j)
                    {
                        case 0:
                            label.Content = "Status";                            
                            break;
                        case 1:
                            label.Content = "IP";
                            break;
                        case 2:
                            label.Content = "Login";
                            break;
                        case 3:
                            label.Content = "Printer or MFP";
                            break;
                        case 4:
                            label.Content = "UPS";
                            break;
                        case 5:
                            label.Content = "Scan";
                            break;
                    }

                    label.SetValue(Grid.ColumnProperty, j);
                    titleGrid.Children.Add(label);
                }

                StackPanel hostsPanel = new StackPanel();
                hostsPanel.Orientation = Orientation.Vertical;
                hostsPanel.VerticalAlignment = VerticalAlignment.Stretch;
                hostsPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                hostsScroll.Content = hostsPanel;
            }
        }

        private void SetTableView(List<Host> hosts = null)
        {
            List<HostControlTemplate> hostsTemplates = null;

            if (hosts == null)
                hostsTemplates = GetExistingHostFromPanel();
            else
                hostsTemplates = GetExistingHostFromPanel(hosts);

            ChangePanel(true);
            var hostsPanel = (WrapPanel)hostsScroll.Content;            

            if (hostsTemplates != null && hostsTemplates.Count > 0)
            {
                for (int i = 0; i < hostsTemplates.Count; i++)
                {
                    HostControl host = new HostControl(hostsTemplates[i].Host);
                    host.Messages = hostsTemplates[i].Messages != null ? hostsTemplates[i].Messages : new List<HostMessage>();
                    host.Screenshots = hostsTemplates[i].Screenshots;
                    host.CurrentImage = hostsTemplates[i].CurrentImage;
                    host.deleteBtn.Click += DeleteBtn_Click;
                    host.infoBtn.Click += InfoBtn_Click;
                    host.MouseDoubleClick += Host_MouseDoubleClick;
                    hostsPanel.Children.Add(host);
                }

                hostsTemplates = null;
            }

            else
            {
                dbHosts = DbServer.DbGetHosts();
                mesServer.SetDbHosts(dbHosts);

                if (dbHosts != null && dbHosts.Count > 0)
                {
                    for (int i = 0; i < dbHosts.Count; i++)
                    {
                        HostControl host = new HostControl(dbHosts[i]);
                        host.deleteBtn.Click += DeleteBtn_Click;
                        host.infoBtn.Click += InfoBtn_Click;
                        host.MouseDoubleClick += Host_MouseDoubleClick;
                        hostsPanel.Children.Add(host);
                    }
                }
            }

            Properties.Settings.Default.TableView = true;
            Properties.Settings.Default.ListView = false;
            Properties.Settings.Default.Save();

            UpdateHostControlInterface();
        }   
        
        private void SetListView(List<Host> hosts = null)
        {
            List<HostControlTemplate> hostsTemplates = null;

            if (hosts == null)
                hostsTemplates = GetExistingHostFromPanel();
            else
                hostsTemplates = GetExistingHostFromPanel(hosts);

            ChangePanel(false);
            var hostsPanel = (StackPanel)hostsScroll.Content;

            if (hostsTemplates != null && hostsTemplates.Count > 0)
            {
                for (int i = 0; i < hostsTemplates.Count; i++)
                {
                    HostLineControl host = new HostLineControl(hostsTemplates[i].Host);
                    host.Messages = hostsTemplates[i].Messages != null ? hostsTemplates[i].Messages : new List<HostMessage>();
                    host.Screenshots = hostsTemplates[i].Screenshots;
                    host.CurrentImage = hostsTemplates[i].CurrentImage;
                    host.deleteBtn.Click += DeleteBtn_Click;
                    host.infoBtn.Click += InfoBtn_Click;
                    host.MouseDoubleClick += Host_MouseDoubleClick;
                    hostsPanel.Children.Add(host);
                }

                hostsTemplates = null;
            }

            else
            {
                dbHosts = DbServer.DbGetHosts();
                mesServer.SetDbHosts(dbHosts);

                if (dbHosts != null && dbHosts.Count > 0)
                {
                    for (int i = 0; i < dbHosts.Count; i++)
                    {
                        HostLineControl host = new HostLineControl(dbHosts[i]);
                        host.deleteBtn.Click += DeleteBtn_Click;
                        host.infoBtn.Click += InfoBtn_Click;
                        host.MouseDoubleClick += Host_MouseDoubleClick;
                        hostsPanel.Children.Add(host);
                    }
                }
            }

            Properties.Settings.Default.TableView = false;
            Properties.Settings.Default.ListView = true;
            Properties.Settings.Default.Save();
        }

        private void tableBtn_Click(object sender, RoutedEventArgs e)
        {
            tableBtn.IsChecked = true;
            listBtn.IsChecked = false;

            if (!Properties.Settings.Default.TableView)
                SetTableView();
        }

        private void listBtn_Click(object sender, RoutedEventArgs e)
        {
            tableBtn.IsChecked = false;
            listBtn.IsChecked = true;

            if (!Properties.Settings.Default.ListView)
                SetListView();
        }

        private void clearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!settings.SaveOnClear)
                logTxt.Clear();
            else
            {
                SaveServerLog();
                logTxt.Clear();
            }
        }

        private void saveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveServerLog();           
        }

        private void intervalsBtn_Click(object sender, RoutedEventArgs e)
        {
            IntervalsReportWindow intervals = new IntervalsReportWindow();
            intervals.Owner = this;
            intervals.ShowDialog();
        }
    }
}
