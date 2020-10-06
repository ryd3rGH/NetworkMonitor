using Ryd3rNetworkMonitor.Library;
using System;
using System.Collections.Generic;
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

namespace Ryd3rNetworkMonitor.ClientControlPanel
{
    public partial class SettingsWindow : Window
    {
        public bool IsRegAction { get; set; }
        public Settings settings;        

        public SettingsWindow(Settings stgs)
        {
            IsRegAction = false;

            InitializeComponent();
            settings = stgs;

            if (settings != null)
            {
                ipTxt.Text = settings.Ip;
                mesPortTxt.Text = settings.MesPort.ToString();
                intervalTxt.Text = settings.SendInterval.ToString();

                if (settings.SendOnClick)
                    messageChBox.IsChecked = true;
                else
                    messageChBox.IsChecked = false;

                if (settings.Host != null)
                {
                    nameTxt.Text = settings.Host.Name;
                    loginTxt.Text = settings.Host.Login;
                    passTxt.Text = settings.Host.Password;
                    printerTxt.Text = settings.Host.PrinterMFP;

                    if (settings.Host.UPS)
                        aUpsBtn.IsChecked = true;
                    else
                        naUpsBtn.IsChecked = true;

                    if (settings.Host.Scanner)
                        aScanBtn.IsChecked = true;
                    else
                        naScanBtn.IsChecked = true;
                }                
            }
        }

        private void applyBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((nameTxt.Text != string.Empty && !String.IsNullOrWhiteSpace(nameTxt.Text)) &&
                loginTxt.Text != string.Empty && !String.IsNullOrWhiteSpace(loginTxt.Text) &&
                passTxt.Text != string.Empty && !String.IsNullOrWhiteSpace(passTxt.Text) &&
                printerTxt.Text != string.Empty && !String.IsNullOrWhiteSpace(printerTxt.Text))
            {
                if (settings.Host != null)
                {
                    //если данные хоста загружены из файла, то проверить данные и при необходимости обновить их в файле и в БД на сервере
                    settings.SaveSettings(
                            !String.IsNullOrWhiteSpace(ipTxt.Text) ? ipTxt.Text : "127.0.0.1",
                            !String.IsNullOrWhiteSpace(mesPortTxt.Text) ? Convert.ToInt32(mesPortTxt.Text) : 17178,
                            !String.IsNullOrWhiteSpace(intervalTxt.Text) ? Convert.ToInt32(intervalTxt.Text) : 30,
                            (bool)messageChBox.IsChecked ? true : false,
                            new Library.Host(settings.Host.HostId, Settings.GetLocalIPAddress(),
                                !String.IsNullOrWhiteSpace(nameTxt.Text) ? nameTxt.Text : string.Empty,
                                !String.IsNullOrWhiteSpace(loginTxt.Text) ? loginTxt.Text : string.Empty,
                                !String.IsNullOrWhiteSpace(passTxt.Text) ? passTxt.Text : string.Empty,
                                !String.IsNullOrWhiteSpace(printerTxt.Text) ? printerTxt.Text : string.Empty,
                                (bool)aUpsBtn.IsChecked ? true : false,
                                (bool)aScanBtn.IsChecked ? true : false,
                                DateTime.Now
                            ));

                    //доделать проверку и обновление данных в БД на сервере
                }
                else
                {
                    //если данных хоста нет (скорее всего нет файла host.xml), то нужно отправить данные на регистрацию серверу.
                    //собрать объект Host

                    settings.Host = new Library.Host(string.Empty, Settings.GetLocalIPAddress(),
                                                    !String.IsNullOrWhiteSpace(nameTxt.Text) ? nameTxt.Text : string.Empty,
                                                    !String.IsNullOrWhiteSpace(loginTxt.Text) ? loginTxt.Text : string.Empty,
                                                    !String.IsNullOrWhiteSpace(passTxt.Text) ? passTxt.Text : string.Empty,
                                                    !String.IsNullOrWhiteSpace(printerTxt.Text) ? printerTxt.Text : string.Empty,
                                                    (bool)aUpsBtn.IsChecked ? true : false,
                                                    (bool)aScanBtn.IsChecked ? true : false,
                                                    DateTime.Now);

                    settings.Host.SaveHostFile(AppDomain.CurrentDomain.BaseDirectory);

                    IFormatter formatter = new BinaryFormatter();
                    InnerMessage inMes = new InnerMessage(InnerMessageTypes.Registration, string.Empty, null, null, null);
                    TcpClient regClient = new TcpClient(settings.Ip, settings.MesPort);
                    NetworkStream stream = regClient.GetStream();
                    formatter.Serialize(stream, new HostMessage(settings.Host, inMes));

                    if (stream.CanRead)
                    {
                        byte[] buffer = new byte[1024];
                        StringBuilder newHostId = new StringBuilder();
                        int bytes = 0;

                        do
                        {
                            bytes = stream.Read(buffer, 0, buffer.Length);
                            newHostId.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, bytes));

                        }
                        while (stream.DataAvailable);

                        settings.Host.HostId = newHostId.ToString();
                        if (settings.Host.HostId != string.Empty && !String.IsNullOrWhiteSpace(settings.Host.HostId))
                        {
                            settings.Host.InsertHostIdToFile();
                            MessageBox.Show("Host successfully registered", "Success");
                        }
                        else
                            MessageBox.Show("Registration failed", "Warning");
                    }
                    else
                        MessageBox.Show("No response from server");
                }
            }
            else
                MessageBox.Show("Changes will not be saved due to the fact that\nnot all fields have been filled", "Warning");

            this.Close();
        }

        private void mesPortTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void regPortTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void intervalTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void respTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }
    }
}
