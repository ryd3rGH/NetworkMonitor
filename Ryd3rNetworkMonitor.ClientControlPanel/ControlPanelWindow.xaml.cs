using Ryd3rNetworkMonitor.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace Ryd3rNetworkMonitor.ClientControlPanel
{
    public partial class ControlPanelWindow : Window
    {
        private bool clientStatus;
        private bool ClientStatus {
            get { return clientStatus; }
            set 
            {
                clientStatus = value;
                if (clientStatus)
                    mesValueLbl.Content = "Started";
                else
                    mesValueLbl.Content = "Stopped";
            } 
        }

        private DispatcherTimer timer;
        private Host host;
        private Settings settings;
        
        public ControlPanelWindow()
        {
            InitializeComponent();

            settings = new Settings();
            settings.GetSettings();

            ClientStatus = false;

            LoadHostInfo();
            UpdateInfo();            
        }

        private string ConstructLogString(string str)
        {
            return $"\n{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} " + str;
        }

        private void LoadHostInfo()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml"))            
                MessageBox.Show("You need to add info about this host\nbefore start using program", "Warning");            
            else            
                host = new Host(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml");
            
        }    

        public bool SendMessage(bool isExit, bool isRegistration)
        {
            if (host != null)
            {
                if (host.HostId != null)
                {
                    HostMessage mes = new HostMessage(host.HostId, host.Ip, host.Name, isExit, isRegistration);
                    try
                    {
                        using (TcpClient mesClient = new TcpClient(settings.Ip, settings.MesPort))
                        {
                            IFormatter formatter = new BinaryFormatter();
                            NetworkStream strm = mesClient.GetStream();
                            formatter.Serialize(strm, mes);
                            strm.Close();
                        }

                        return true;
                    }
                    catch (SocketException ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return false;
                    }
                }
                else
                    return false;
            }
            else
                return false;
        }

        private void StartMessaging()
        {
            if (!ClientStatus)
            {
                ClientStatus = true;

                if (SendMessage(false, false))
                {
                    logTxt.Text += ConstructLogString("Message sending started");

                    timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, settings.SendInterval);
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
                else
                {
                    ClientStatus = false;
                    logTxt.Text += ConstructLogString("Server not found");
                }
            }               
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            SendMessage(false, false);
        }

        private void StopMessaging()
        {
            if (ClientStatus)
            {
                ClientStatus = false;

                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }

                if (host != null)
                {
                    host.LastOnline = DateTime.Now;

                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml"))
                    {
                        host.UpdateLastOnlineTime();
                        SendMessage(true, false);
                    }
                }

                logTxt.Text += ConstructLogString("Message sending stopped");
            }
        }

        private void UpdateInfo()
        {
            if (settings != null)
            {
                ipValueLbl.Content = settings.Ip;

                if (host != null)
                {                    
                    nameValueLbl.Content = host.Name;
                    loginValueLbl.Content = host.Login;
                    printerValueLbl.Content = host.PrinterMFP;

                    upsValueLbl.Content = host.UPS ? "Available" : "Not available";
                    scanValueLbl.Content = host.Scanner ? "Available" : "Not available";

                    if (settings.SendOnClick)
                    {
                        startBtn.IsEnabled = true;
                        stopBtn.IsEnabled = true;
                    }
                    else
                    {
                        startBtn.IsEnabled = false;
                        stopBtn.IsEnabled = false;
                    }

                    if (host.HostId != string.Empty)
                        regValueLbl.Content = "OK";
                    else
                        regValueLbl.Content = "---";
                }
                else
                {
                    startBtn.IsEnabled = false;
                    stopBtn.IsEnabled = false;

                    regValueLbl.Content = "---";
                }
            }
            else
                regValueLbl.Content = "---";
        }

        private void logTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            logTxt.ScrollToEnd();
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWin = new SettingsWindow(settings);
            settingsWin.Owner = this;
            settingsWin.applyBtn.Click += ApplyBtn_Click;
            settingsWin.ShowDialog();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            StopMessaging();

            if (host == null)
            {
                host = settings.Host;
                host.InsertHostIdToFile();

                SendMessage(false, true);

                StartMessaging();
            }                       

            settings = null;
            settings = new Settings();
            settings.GetSettings();

            host = settings.Host;
            UpdateInfo();

            logTxt.Text += ConstructLogString("Settings was updated");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ClientStatus)
                StopMessaging();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            StartMessaging();
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopMessaging();
        }
    }
}
