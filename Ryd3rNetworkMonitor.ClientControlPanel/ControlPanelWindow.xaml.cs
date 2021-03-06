﻿using Ryd3rNetworkMonitor.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
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
using System.Windows.Forms;
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
        private int mouseClicks;
        private int keyboardPresses;

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
        private int MouseClicks
        {
            get { return mouseClicks; }
            set
            {
                if (value <= int.MaxValue)
                    mouseClicks = value;
                else
                    mouseClicks = int.MaxValue;
            }
        }
        private int KeyboardPresses
        {
            get { return keyboardPresses; }
            set
            {
                if (value <= int.MaxValue)
                    keyboardPresses = value;
                else
                    keyboardPresses = int.MaxValue;
            }
        }

        private BackgroundWorker scWorker;
        private DispatcherTimer timer;
        private DispatcherTimer scTimer;
        private Host host;
        private Settings settings;
        private List<HostMessage> messageQueue;
        private KeyboardPressesCapture kp;

        public ControlPanelWindow()
        {
            InitializeComponent();

            settings = new Settings();
            settings.GetSettings();

            ClientStatus = false;
            messageQueue = new List<HostMessage>();

            LoadHostInfo();
            UpdateInfo();
        }

        private string ConstructLogString(string str)
        {
            return $"\n{DateTime.Now.ToShortDateString()} | {DateTime.Now.ToLongTimeString()} | --- " + str;
        }

        private byte[] GetScreenshot()
        {
            try
            {
                byte[] imageBytes = null;

                var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop, 0, 0, bmp.Size);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Bmp);
                    imageBytes = ms.ToArray();
                }

                return imageBytes;
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Get screenshot error"));
                return null;
            }
        }

        private void LoadHostInfo()
        {
            try
            {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml"))
                    System.Windows.MessageBox.Show("You need to add info about this host\nbefore start using program", "Warning");
                else
                    host = new Host(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml");
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Load host info error"));
            }
        }

        private void ScreenSending()
        {
            if (scTimer != null)
            {
                scTimer.Stop();
                scTimer = null;
            }

            Random rnd = new Random();
            int sec = rnd.Next(62, 3581);

            scTimer = new DispatcherTimer();
            scTimer.Interval = new TimeSpan(0, 0, sec);
            scTimer.Tick += ScTimer_Tick;
            scTimer.Start();
        }

        private void ScTimer_Tick(object sender, EventArgs e)
        {
            scWorker = new BackgroundWorker();
            scWorker.WorkerSupportsCancellation = true;
            scWorker.DoWork += ScWorker_DoWork;
            scWorker.RunWorkerCompleted += ScWorker_RunWorkerCompleted;
            scWorker.RunWorkerAsync();
        }

        private void ScWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                byte[] scBytes = GetScreenshot();

                InnerMessage mes = new InnerMessage(InnerMessageTypes.Screenshot, null, scBytes, null, null);
                SendMessage(mes);
                scTimer.Stop();
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Send screenshot error"));
            }
        }

        private void ScWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logTxt.AppendText(ConstructLogString("Screenshot sended"));
            ScreenSending();
        }

        public bool SendMessage(HostMessage mes)
        {
            try
            {
                if (mes != null && mes.InnerMessage != null)
                {
                    using (TcpClient mesClient = new TcpClient(settings.Ip, settings.MesPort))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        NetworkStream strm = mesClient.GetStream();
                        formatter.Serialize(strm, mes);

                        if (mes.InnerMessage.Type == InnerMessageTypes.RegistrationCheck)
                        {
                            if (strm.CanRead)
                            {
                                byte[] buffer = new byte[1024];
                                StringBuilder respString = new StringBuilder();
                                int bytes = 0;

                                do
                                {
                                    bytes = strm.Read(buffer, 0, buffer.Length);
                                    respString.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, bytes));

                                }
                                while (strm.DataAvailable);

                                if (String.IsNullOrWhiteSpace(respString.ToString()) != true && respString.ToString().Length > 0)
                                {
                                    if (respString.ToString().Equals("OK"))
                                    {
                                        //проверка (и возможно  обновление данных хоста в бд) выполнена
                                        logTxt.Text += ConstructLogString("Host successfully checked");
                                    }
                                }
                                else
                                {
                                    //ошибка при получении ответа
                                    logTxt.Text += ConstructLogString("Error when receiving server response");
                                }
                            }
                        }

                        strm.Close();
                    }

                    return true;
                }
                else
                {
                    logTxt.AppendText(ConstructLogString("Send message error"));
                    return false;
                }                    
            }

            catch (SocketException)
            {
                return false;
            }
        }

        public bool SendMessage(InnerMessage inMes)
        {
            try
            {
                if (inMes != null)
                {
                    HostMessage mes = new HostMessage(host, inMes);

                    try
                    {
                        using (TcpClient mesClient = new TcpClient(settings.Ip, settings.MesPort))
                        {
                            IFormatter formatter = new BinaryFormatter();
                            NetworkStream strm = mesClient.GetStream();
                            formatter.Serialize(strm, mes);

                            if (inMes.Type == InnerMessageTypes.Registration || inMes.Type == InnerMessageTypes.RegistrationCheck)
                            {
                                if (strm.CanRead)
                                {
                                    byte[] buffer = new byte[1024];
                                    StringBuilder respString = new StringBuilder();
                                    int bytes = 0;

                                    do
                                    {
                                        bytes = strm.Read(buffer, 0, buffer.Length);
                                        respString.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, bytes));

                                    }
                                    while (strm.DataAvailable);

                                    if (String.IsNullOrWhiteSpace(respString.ToString()) != true && respString.ToString().Length > 0)
                                    {
                                        if (respString.ToString().Equals("OK"))
                                        {
                                            //проверка (и возможно  обновление данных хоста в бд) выполнена
                                            logTxt.Text += ConstructLogString("Host successfully checked");
                                        }
                                        else
                                        {
                                            //регистрация выполнена
                                            settings.Host.HostId = respString.ToString();
                                            logTxt.Text += ConstructLogString("Settings was updated. Host successfully registered");
                                        }
                                    }
                                    else
                                    {
                                        //ошибка при получении ответа
                                        messageQueue.Add(mes);
                                        logTxt.Text += ConstructLogString("Error when receiving server response. Message saved");
                                    }
                                }
                            }

                            strm.Close();
                            MouseClicks = 0;
                            KeyboardPresses = 0;
                        }

                        return true;
                    }
                    catch (SocketException)
                    {
                        messageQueue.Add(mes);
                        logTxt.Text += ConstructLogString("Server not found. Message saved");
                        return false;
                    }
                }
                else
                    return false;
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Send message error"));
                return false;
            }
        }

        private void SendMessageQueue()
        {
            //отправка списка сохраненных сообщений
            if (messageQueue != null && messageQueue.Count > 0)
            {
                for (int i=0; i<messageQueue.Count; i++)
                {
                    if (SendMessage(messageQueue[i]))
                        messageQueue[i] = null;
                    else                    
                        break;                    
                }

                messageQueue = messageQueue.FindAll(x => x != null);
                if (messageQueue == null)
                    messageQueue = new List<HostMessage>();
            }
        }

        private void StartMessaging()
        {
            if (!ClientStatus)
            {
                ClientStatus = true;

                if (host != null)
                {
                    InnerMessage inMes;

                    if (host.HostId == null || host.HostId == string.Empty)
                        inMes = new InnerMessage(InnerMessageTypes.Registration, string.Empty, null, null, null);

                    else
                        inMes = new InnerMessage(InnerMessageTypes.RegistrationCheck, string.Empty, null, 0, null);

                    if (SendMessage(inMes))
                    {
                        logTxt.AppendText(ConstructLogString("Registration check successful"));
                        logTxt.AppendText(ConstructLogString("Message sending started"));
                    }           
                        
                    else
                    {
                        logTxt.AppendText(ConstructLogString("Registration check not passed, message saved"));
                        logTxt.AppendText(ConstructLogString("All messages will be saved"));
                    }

                    MouseClicksCapture.StartCapture();
                    MouseClicksCapture.MouseAction += MouseClicksCapture_MouseAction;

                    kp = new KeyboardPressesCapture(true);
                    kp.KeyDown += Kp_KeyDown;

                    timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, settings.SendInterval);
                    timer.Tick += Timer_Tick;
                    timer.Start();                   

                    ScreenSending();                   
                }
            }
        }

        private void Kp_KeyDown(Keys key, bool Shift, bool Ctrl, bool Alt)
        {
            KeyboardPresses += 1;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (messageQueue != null && messageQueue.Count > 0)
                SendMessageQueue();

            InnerMessage inMes = new InnerMessage(InnerMessageTypes.Online, string.Empty, GetScreenshot(), MouseClicks, KeyboardPresses);
            SendMessage(inMes);
        }

        private void StopMessaging()
        {
            try
            {
                if (ClientStatus)
                {
                    ClientStatus = false;

                    if (timer != null)
                    {
                        timer.Stop();
                        timer = null;
                    }

                    if (scTimer != null)
                    {
                        scTimer.Stop();
                        scTimer = null;
                    }

                    if (host != null)
                    {
                        host.LastOnline = DateTime.Now;

                        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml"))
                        {
                            host.UpdateLastOnlineTime();

                            InnerMessage inMes = new InnerMessage(InnerMessageTypes.Exit, string.Empty, null, MouseClicks, null);
                            SendMessage(inMes);
                        }
                    }

                    MouseClicksCapture.StopCapture();
                    MouseClicksCapture.MouseAction -= MouseClicksCapture_MouseAction;
                    kp.Dispose();

                    logTxt.Text += ConstructLogString("Message sending stopped");
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Stop messaging error"));
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
            //сравнение старых и новых настроек и данных клиента
            //InnerMessageTypes.SettingsChanged

            if (ClientStatus)
                StopMessaging();

            if (host != null)
            {
                var newSettings = new Settings();
                newSettings.GetSettings();

                if (!settings.Equals(newSettings))
                {
                    logTxt.Text += ConstructLogString("Settings was updated");
                    InnerMessage inMes = new InnerMessage(InnerMessageTypes.SettingsChanged, string.Empty, null, null, null);
                    SendMessage(inMes);
                }
            }

            settings = null;
            settings = new Settings();
            settings.GetSettings();

            host = settings.Host;
            UpdateInfo();            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ClientStatus)
                StopMessaging();

            //сделать сохранение очереди сообщений в файл
            //как призакрытии, так и периодическое
        }
        
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            StartMessaging();
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopMessaging();
        }

        private void MouseClicksCapture_MouseAction(object sender, EventArgs e)
        {
            MouseClicks += 1;
        }
    }
}
