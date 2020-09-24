using Ryd3rNetworkMonitor.Library;
using Ryd3rNetworkMonitor.ServerControlPanel.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    public partial class ControlPanelWindow : Window
    {
        private bool? ServerStatus { get; set; }
        private int offlineTimeCounter;

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

            mesServer = new MessageServer(settings.Ip, settings.MesPort);
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
            
            ShowHostControls(dbHosts);
            FoldersCheck(dbHosts);

            SettingsToWindowLog(settings);

            reportsBox.Visibility = Visibility.Hidden;
            viewBtnsPanel.Visibility = Visibility.Hidden;
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
                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}\\Log");
                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}\\Screenshots");
                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}");
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Folders delete error"));
            }
        }

        private void SaveLogAndPics(string hostId)
        {
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
                                            case InnerMessageTypes.Count:
                                                break;
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

        private void ClientsChecking()
        {
            offlineTimeCounter = 0;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, settings.MessageCheckTime);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void SaveScreenshot(byte[] imBytes, string hostId, DateTime dt)
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

        private void Timer_Tick(object sender, EventArgs e)
        {
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
                                    }
                                }
                            }

                            if (mes.InnerMessage != null)
                            {
                                Host hostToUpdate = mesServer.Hosts.Find(x => x.HostId == mes.Host.HostId);
                                hostToUpdate.LastOnline = DateTime.Now;

                                switch (mes.InnerMessage.Type)
                                {
                                    case InnerMessageTypes.Count:
                                        break;

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

                    ShowHostControls(mesServer.Hosts);

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
                                    hst.Messages.Add(new HostMessage(hst.Host, new InnerMessage(InnerMessageTypes.Timeout, null, null, null, null)));
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
            for (int i = 0; i < hostsPanel.Children.Count; i++)
            {
                if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                {
                    HostControl host = (HostControl)hostsPanel.Children[i];
                    host.UpdateInterface(settings.IpDisplay, settings.LoginDisplay);
                }
            }
        }

        private void ShowHostControls(List<Host> hosts)
        {
            List<Host> hostsOnPanel = new List<Host>();
            if (hostsPanel.Children.Count > 0)
            {
                for (int i = 0; i < hostsPanel.Children.Count; i++)
                {
                    if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                        hostsOnPanel.Add(((HostControl)hostsPanel.Children[i]).Host);
                }
            }

            if (hosts != null)
            {
                if (hostsOnPanel.Count > 0)
                {
                    if (hosts.Count > 0)
                    {
                        if (hosts.Count == 1)
                        {
                            if (!hosts[0].Equals(hostsOnPanel[0]))
                            {
                                HostControl host = new HostControl(hosts[0]);
                                host.deleteBtn.Click += DeleteBtn_Click;
                                host.infoBtn.Click += InfoBtn_Click;
                                host.MouseDoubleClick += Host_MouseDoubleClick;
                                hostsPanel.Children.Add(host);
                            }
                        }

                        else if (hosts.Count > 1)
                        {
                            for (int i=0; i<hosts.Count; i++)
                            {
                                int hCount = 0;

                                for (int j=0; j<hostsOnPanel.Count; j++)
                                {
                                    if (hosts[i].Equals(hostsOnPanel[j]))
                                        break;
                                    else
                                        hCount += 1;
                                }

                                if (hCount == hostsOnPanel.Count)
                                {
                                    HostControl host = new HostControl(hosts[i]);
                                    host.deleteBtn.Click += DeleteBtn_Click;
                                    host.infoBtn.Click += InfoBtn_Click;
                                    host.MouseDoubleClick += Host_MouseDoubleClick;
                                    hostsPanel.Children.Add(host);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i=0; i<hosts.Count; i++)
                    {
                        HostControl host = new HostControl(hosts[i]);
                        host.deleteBtn.Click += DeleteBtn_Click;
                        host.infoBtn.Click += InfoBtn_Click;
                        host.MouseDoubleClick += Host_MouseDoubleClick;
                        hostsPanel.Children.Add(host);
                    }
                }
            }

            if (mesServer.Hosts != null)
            {
                for (int i = 0; i < mesServer.Hosts.Count; i++)
                {
                    for (int j = 0; j < hostsPanel.Children.Count; j++)
                    {
                        if (hostsPanel.Children[j].GetType() == typeof(HostControl))
                        {
                            HostControl hst = (HostControl)hostsPanel.Children[j];
                            if (mesServer.Hosts[i].HostId == hst.Host.HostId)
                            {
                                hst.HostState = mesServer.Hosts[i].IsOnline;
                                hst.Host.LastOnline = mesServer.Hosts[i].LastOnline;
                                break;
                            }
                        }
                    }
                }
            }
            else
                dbServer.Start();

            UpdateHostControlInterface();
        }

        private void ShowHostInfoWindow()
        {
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
                    }

                    dbHosts = DbServer.DbGetHosts();

                    ShowHostControls(dbHosts);
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

            ShowHostControls(dbHosts);
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

            if (ServerStatus != null)  
            {
                if (!(bool)ServerStatus)
                {
                    for (int i=0; i<hostsPanel.Children.Count; i++)
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
        }
    }
}
