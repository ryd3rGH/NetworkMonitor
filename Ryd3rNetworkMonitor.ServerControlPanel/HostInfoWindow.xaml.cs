using Ryd3rNetworkMonitor.Library;
using Ryd3rNetworkMonitor.ServerControlPanel.Controls;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    public partial class HostInfoWindow : Window
    {
        public List<HostMessage> Messages { get; private set; }

        public Host host;

        private List<string> stringsFromLog;

        public HostInfoWindow(Host host, List<HostMessage> messages)
        {            
            InitializeComponent();

            this.host = host;
            this.Messages = messages;

            todayBtn.IsChecked = true;

            if (host != null)
            {
                idValueLbl.Content = !String.IsNullOrWhiteSpace(host.HostId) ? host.HostId : "no value";
                ipValueLbl.Content = !String.IsNullOrWhiteSpace(host.Ip) ? host.Ip : "no value";
                nameTxt.Text = !String.IsNullOrWhiteSpace(host.Name) ? host.Name : "no value";
                loginValueLbl.Content = !String.IsNullOrWhiteSpace(host.Login) ? host.Login : "no value";
                passValueLbl.Content = !String.IsNullOrWhiteSpace(host.Password) ? host.Password : "no value";
                printerTxt.Text = !String.IsNullOrWhiteSpace(host.PrinterMFP) ? host.PrinterMFP : "no value";
                lastOnlineValuelbl.Content = host.LastOnline != null ? host.LastOnline.ToString() : "no value";

                if (host.UPS)
                    aUpsBtn.IsChecked = true;
                else
                    naUpsBtn.IsChecked = true;

                if (host.Scanner)
                    aScanBtn.IsChecked = true;
                else
                    naScanBtn.IsChecked = true;
            }

            ShowScreenshots();

            FillLog();
        }

        private void ShowScreenshots()
        {
            try
            {
                screenPanel.Children.Clear();

                var folderPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{host.HostId}\\Screenshots");
                if (folderPath.GetDirectories().Length > 0)
                {
                    var folders = folderPath.GetDirectories();
                    var lastFolder = $"Logs\\{host.HostId}\\Screenshots\\" + folders[folders.Length - 1].ToString();

                    if (Directory.GetFiles(lastFolder).Length > 0)
                    {
                        byte[] imBytes = null;
                        string[] files = Directory.GetFiles(lastFolder);

                        string strDt = string.Empty;

                        if (files.Length <= 10)
                        {
                            for (int i = 0; i < Directory.GetFiles(lastFolder).Length; i++)
                            {
                                imBytes = File.ReadAllBytes(files[i]);
                                strDt = $"{lastFolder.Substring(lastFolder.LastIndexOf("\\") + 1).Replace('-', '.')} " +
                                    $"{files[i].Substring(files[i].LastIndexOf("\\") + 1).Replace('-', ':').Replace(".jpg", "")}";
                                DateTime imDt = Convert.ToDateTime(strDt);
                                ScreenshotControl scr = new ScreenshotControl(imBytes, imDt);
                                screenPanel.Children.Add(scr);
                            }
                        }
                        else if (files.Length > 10)
                        {
                            for (int i = Directory.GetFiles(lastFolder).Length - 10; i < Directory.GetFiles(lastFolder).Length; i++)
                            {
                                imBytes = File.ReadAllBytes(files[i]);
                                strDt = $"{lastFolder.Substring(lastFolder.LastIndexOf("\\") + 1).Replace('-', '.')} " +
                                    $"{files[i].Substring(files[i].LastIndexOf("\\") + 1).Replace('-', ':').Replace(".jpg", "")}";
                                DateTime imDt = Convert.ToDateTime(strDt);
                                ScreenshotControl scr = new ScreenshotControl(imBytes, imDt);
                                screenPanel.Children.Add(scr);
                            }
                        }
                    }
                }

                if (Messages != null && Messages.Count > 0)
                {
                    for (int i = 0; i < Messages.Count; i++)
                    {
                        if (Messages[i].InnerMessage.Type == InnerMessageTypes.Screenshot && Messages[i].InnerMessage.ImageBytes != null)
                        {
                            ScreenshotControl scr = new ScreenshotControl(Messages[i]);
                            screenPanel.Children.Add(scr);
                        }
                    }
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Show screenshot error"));
            }
        }

        private string ConstructLogString(string str)
        {
            return $"\n{DateTime.Now.ToShortDateString()} | {DateTime.Now.ToLongTimeString()} | --- " + str;
        }

        private string ConstructLogString(string str, DateTime dt)
        {
            return $"\n{dt.ToShortDateString()} | {dt.ToLongTimeString()} | --- " + str;
        }

        private string MessageSelector(HostMessage mes)
        {
            string str = string.Empty;

            switch (mes.InnerMessage.Type)
            {            
                case InnerMessageTypes.Exit:
                    str = ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' offline", mes.MessageTime);
                    break;
                case InnerMessageTypes.Online:
                    break;
                case InnerMessageTypes.Registration:
                    str = ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' was registered in system and online now", mes.MessageTime);
                    break;
                case InnerMessageTypes.RegistrationCheck:
                    str = ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' has passed registration check", mes.MessageTime);
                    break;
                case InnerMessageTypes.Screenshot:
                    str = ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' sended screenshot", mes.MessageTime);
                    break;
                case InnerMessageTypes.SettingsChanged:
                    str = ConstructLogString($"Host '{mes.Host.Ip}' with HostID '{mes.Host.HostId}' has changed settings", mes.MessageTime);
                    break;
            }

            return str;
        }

        private void GetStringsFromLogFiles()
        {
            try
            {
                stringsFromLog = new List<string>();
                string logDir = AppDomain.CurrentDomain.BaseDirectory + $"Logs\\{host.HostId}\\Log";

                if (Directory.Exists(logDir))
                {
                    if (Directory.GetFiles(logDir).Length > 0)
                    {
                        var logFiles = Directory.GetFiles(logDir);

                        for (int i = 0; i < logFiles.Length; i++)
                        {
                            using (StreamReader rd = new StreamReader(logFiles[i]))
                            {
                                string logStrings = rd.ReadToEnd();
                                string[] temp = logStrings.Split('\n');

                                for (int j = 0; j < temp.Length; j++)
                                    if (temp[j] != string.Empty && temp[j] != "\r" && temp[j] != "")
                                        stringsFromLog.Add(temp[j]);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Get strings from log file error"));
            }
        }

        private void FillLog()
        {
            try
            {
                logTxt.Clear();

                if (stringsFromLog == null || stringsFromLog.Count == 0)
                    GetStringsFromLogFiles();

                for (int i = 0; i < stringsFromLog.Count; i++)
                {
                    if (stringsFromLog[i] != string.Empty)
                    {
                        DateTime dt = Convert.ToDateTime($"{stringsFromLog[i].Substring(0, 21).Replace(" | ", " ")}");

                        if ((bool)todayBtn.IsChecked)
                        {
                            if (dt.Date == DateTime.Now.Date)
                                logTxt.AppendText(stringsFromLog[i]);
                        }

                        else if ((bool)weekBtn.IsChecked)
                        {
                            if (dt.Date >= DateTime.Now.Date.AddDays(-7) && dt.Date <= DateTime.Now.Date)
                                logTxt.AppendText(stringsFromLog[i]);
                        }

                        else if ((bool)wholeBtn.IsChecked)
                            logTxt.AppendText(stringsFromLog[i]);
                    }
                }

                if (Messages != null && Messages.Count > 0)
                {
                    for (int i = 0; i < Messages.Count; i++)
                    {
                        if ((bool)todayBtn.IsChecked)
                        {
                            if (Messages[i].MessageTime.Date == DateTime.Now.Date)
                                logTxt.AppendText(MessageSelector(Messages[i]));
                        }

                        else if ((bool)weekBtn.IsChecked)
                        {
                            if (Messages[i].MessageTime.Date >= DateTime.Now.Date.AddDays(-7) && Messages[i].MessageTime.Date <= DateTime.Now.Date)
                                logTxt.AppendText(MessageSelector(Messages[i]));
                        }

                        else if ((bool)wholeBtn.IsChecked)
                            logTxt.AppendText(MessageSelector(Messages[i]));
                    }
                }
            }
            catch (Exception)
            {
                logTxt.AppendText(ConstructLogString("Fill log error"));
            }
        }

        private void applyBtn_Click(object sender, RoutedEventArgs e)
        {
            bool ups = false;
            bool scanner = false;

            if ((bool)aUpsBtn.IsChecked)
                ups = true;
            else
                ups = false;

            if ((bool)aScanBtn.IsChecked)
                scanner = true;
            else
                scanner = false;

            if (nameTxt.Text != host.Name || printerTxt.Text != host.PrinterMFP 
                || ups != host.UPS || scanner != host.Scanner)
            {
                host.Name = nameTxt.Text != string.Empty ? nameTxt.Text : "no value";
                host.PrinterMFP = printerTxt.Text != string.Empty ? printerTxt.Text : "no value";
                host.UPS = ups;
                host.Scanner = scanner;

                DbServer.DbUpdateHost(host);
            }

            applyBtn.Tag = host;
            this.Close();
        }

        private void logTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            logTxt.ScrollToEnd();
        }

        private void todayBtn_Checked(object sender, RoutedEventArgs e)
        {
            weekBtn.IsChecked = false;
            wholeBtn.IsChecked = false;

            FillLog();
        }

        private void weekBtn_Checked(object sender, RoutedEventArgs e)
        {
            todayBtn.IsChecked = false;
            wholeBtn.IsChecked = false;

            FillLog();
        }

        private void wholeBtn_Checked(object sender, RoutedEventArgs e)
        {
            todayBtn.IsChecked = false;
            weekBtn.IsChecked = false;

            FillLog();
        }
    }
}
