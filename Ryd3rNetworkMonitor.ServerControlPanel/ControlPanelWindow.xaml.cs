using Ryd3rNetworkMonitor.Library;
using Ryd3rNetworkMonitor.ServerControlPanel.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    public partial class ControlPanelWindow : Window
    {
        private bool? ServerStatus { get; set; }
        private int offlineTimeCounter;

        private BackgroundWorker bgw;
        private BackgroundWorker rBgw;
        private DispatcherTimer timer;
        private MessageServer mesServer;
        private RegistrationServer regServer;
        private DbServer dbServer;
        private List<Host> dbHosts;
        private Settings settings;

        public ControlPanelWindow()
        {
            InitializeComponent();

            ServerStatus = null;

            settings = new Settings();
            settings.GetSettings();

            mesServer = new MessageServer(settings.Ip, settings.MesPort);
            regServer = new RegistrationServer(settings.Ip, settings.RegPort);
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

            SettingsToWindowLog(settings);
        }        

        private void ClientsChecking()
        {
            offlineTimeCounter = 0;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, settings.MessageCheckTime);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mesServer.Messages != null && mesServer.Messages.Count > 0)
            {
                for (int i = 0; i < mesServer.Messages.Count; i++)
                {
                    HostMessage mes = mesServer.Messages[i];

                    if (!mes.DelFlag)
                    {
                        if (!mes.IsExit)
                        {
                            if (mes.IsRegistration)
                            {
                                logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Ip}' with HostID '{mes.HostId}' was registered in system and online now")}");

                                mesServer.SetDbHosts(DbServer.DbGetHosts());
                                mesServer.SetHostState(mes.HostId, true);
                            }

                            else
                            {
                                mesServer.SetHostState(mes.HostId, true);
                                if (settings.MessageDisplay)
                                {
                                    logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Ip}' with HostID '{mes.HostId}' online")}");
                                }
                            }

                            mes.SetMessageForDelete();
                        }
                        else
                        {
                            mesServer.SetHostState(mes.HostId, false);
                            logTxt.AppendText($"\n{ConstructLogString($"Host '{mes.Ip}' with HostID '{mes.HostId}' offline")}");

                            Host hostToUpdate = mesServer.Hosts.Find(x => x.HostId == mes.HostId);
                            hostToUpdate.LastOnline = DateTime.Now;
                            DbServer.DbUpdateLastOnline(hostToUpdate);
                        }
                    }
                }

                ShowHostControls(mesServer.Hosts);

                mesServer.Messages.Clear();
            }

            //если хост долго не шлет сообщения, отображать его как оффлайн
            if (offlineTimeCounter < settings.HostOfflineCheckTime)
                offlineTimeCounter += Properties.Settings.Default.MessagesCheckTime;
            else
            {
                
                for (int i=0; i<hostsPanel.Children.Count; i++)
                {
                    if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                    {
                        HostControl hst = (HostControl)hostsPanel.Children[i];
                        var tt = hst.Host.LastOnline;
                        tt.AddSeconds(Properties.Settings.Default.HostOfflineCheckTime);
                        
                        if (DateTime.Now > tt)
                        {
                            hst.HostState = false;
                            logTxt.AppendText($"\n{ConstructLogString($"Host '{hst.Host.Ip}' with HostID '{hst.Host.HostId}' set as offline because of timeout")}");
                            continue;
                        }
                    }
                }

                offlineTimeCounter = 0;
            }
        }

        private void RestartServers()
        {
            StopServers();
            startServerBtn_Click(null, null);
        }

        private void StopServers()
        {
            if ((mesServer != null && mesServer.ServerState) && (regServer != null && regServer.ServerState))
            {
                if (mesServer.Stop() && regServer.Stop())
                {
                    timer.Stop();
                    timer = null;

                    if (bgw != null && rBgw != null)
                    {
                        bgw.CancelAsync();
                        bgw.Dispose();

                        rBgw.CancelAsync();
                        rBgw.Dispose();
                    }

                    ServerStatus = false;
                }
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
                        hostsPanel.Children.Add(host);
                    }
                }
            }

            //host state animation
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

        private void InfoBtn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < hostsPanel.Children.Count; i++)
            {
                if (hostsPanel.Children[i].GetType() == typeof(HostControl))
                {
                    HostControl hostCtrl = (HostControl)hostsPanel.Children[i];
                    if ((Host)hostCtrl.Tag != null)
                    {
                        Host host = (Host)hostCtrl.Tag;

                        HostInfoWindow hostInfo = new HostInfoWindow(host);
                        hostInfo.Owner = this;
                        hostInfo.applyBtn.Click += ApplyInfoBtn_Click;
                        hostInfo.ShowDialog();

                        hostCtrl.Tag = null;
                        break;
                    }
                }
            }
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
            Host hostToDelete = null;

            for (int i=0; i<hostsPanel.Children.Count; i++)
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

            logTxt.Text += ConstructLogString($"Host with HostID '{hostToDelete.HostId}' was deleted");
        }        

        private string ConstructLogString(string str)
        {
            return $"\n{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} " + str;
        }

        private void SettingsToWindowLog(Settings stgs)
        {
            logTxt.AppendText($"{ConstructLogString($"Server IP: {stgs.Ip}")}");
            logTxt.AppendText($"{ConstructLogString($"Registration server port: {stgs.RegPort}")}");
            logTxt.AppendText($"{ConstructLogString($"Message server port: {stgs.MesPort}")}\n");
        }

        private void SettingsToWindowLog(Settings newSettings, Settings oldSettings)
        {
            if (!newSettings.Equals(oldSettings))
            {
                if (newSettings.Ip != oldSettings.Ip)
                    logTxt.AppendText($"{ConstructLogString($"Server IP changed: {newSettings.Ip}")}");

                if (newSettings.RegPort != oldSettings.RegPort)
                    logTxt.AppendText($"{ConstructLogString($"Registration server port changed: {newSettings.RegPort}")}");

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
            if ((mesServer != null && !mesServer.ServerState) && (regServer != null && !regServer.ServerState))
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
            if (regServer.Start())
            {
                rBgw = new BackgroundWorker();
                rBgw.WorkerSupportsCancellation = true;
                rBgw.DoWork += RBgw_DoWork;
                rBgw.RunWorkerAsync();

                logTxt.AppendText($"{ConstructLogString("Registration server started")}");
            }

            if (mesServer.Start())
            {
                mesServer.SetDbHosts(dbHosts);

                bgw = new BackgroundWorker();
                bgw.WorkerSupportsCancellation = true;
                bgw.DoWork += Bgw_DoWork;
                bgw.RunWorkerAsync();

                logTxt.AppendText($"{ConstructLogString("Message server started")}\n");
            }            

            if(dbHosts != null && dbHosts.Count > 0)
            {
                logTxt.AppendText($"{ConstructLogString($"Registered hosts number in DB: {dbHosts.Count}")}");

                for (int i = 0; i < dbHosts.Count; i++)                
                    logTxt.AppendText($"{ConstructLogString($"HostID '{dbHosts[i].HostId}' is offline")}");                
            }
            else
                logTxt.AppendText($"{ConstructLogString($"No registered hosts in DB. Count: 0")}");

            ServerStatus = true;

            ClientsChecking();

            ShowHostControls(dbHosts);
        }

        private void RBgw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (regServer != null)
            {
                regServer.StartListening();
            }

            else
            {
                rBgw.CancelAsync();
                rBgw.Dispose();
            }
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
            StopServers();

            if (ServerStatus != null)  
            {
                if (!(bool)ServerStatus)
                {
                    logTxt.AppendText($"{ConstructLogString("Registration server stopped")}");
                    logTxt.AppendText($"{ConstructLogString("Message server stopped")}\n");
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
                RestartServers();
            }

            else            
                startServerBtn_Click(null, null);            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopServers();
        }
    }
}
