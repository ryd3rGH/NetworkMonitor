using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ryd3rNetworkMonitor.Library;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker bgw;
        private BackgroundWorker rBgw;
        private DispatcherTimer timer;
        private MessageServer mesServer;
        private RegistrationServer regServer;
        private DbServer dbServer;
        private List<Host> dbHosts;

        public MainWindow()
        {
            InitializeComponent();

            mesServer = new MessageServer("127.0.0.1", 17178);
            regServer = new RegistrationServer("127.0.0.1", 17179);
            dbServer = new DbServer();            
        }

        private void ClientsChecking()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mesServer.Messages != null && mesServer.Messages.Count > 0)
            {
                //TODO: доделать очистку списка сообщений
                //можно сделать какой-то доп. список, кот. будет содержать только те, сообщения кот. нужно вывести на экран.
                for (int i=0; i<mesServer.Messages.Count; i++)
                {
                    HostMessage mes = mesServer.Messages[i];

                    if (!mes.DelFlag)
                    {
                        if (!mes.IsExit)
                        {
                            if (mes.IsRegistration)
                            {
                                testTextBox.Text += $"\n{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} Хост {mes.HostId} с именем {mes.HostName} зарегистрирован.";
                                mesServer.SetDbHosts(DbServer.DbGetHosts());
                            }

                            else
                                testTextBox.Text += $"\n{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} Хост {mes.HostId} онлайн.";

                            mes.SetMessageForDelete();
                        }
                        else
                        {
                            testTextBox.Text += $"\n{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} Хост {mes.HostId} офлайн.";

                            Host hostToUpdate = mesServer.Hosts.Find(x => x.HostId == mes.HostId);
                            hostToUpdate.LastOnline = DateTime.Now;
                            DbServer.DbUpdateLastOnline(hostToUpdate);
                        } 
                    }
                }

                //for (int i=0; i<mesServer.Messages.Count; i++)
                //{
                //    if (mesServer.Messages[i].DelFlag)
                //        mesServer.Messages.RemoveAt(i);
                //}
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
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
            dbHosts = new List<Host>();
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

                testTextBox.Text += "\nСервер сообщений запущен";

                for (int i=0; i<dbHosts.Count; i++)
                {
                    testTextBox.Text += $"\n{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} Хост {dbHosts[i].HostId} оффлайн.";
                }

                ClientsChecking();
            }

            if (regServer.Start())
            {
                rBgw = new BackgroundWorker();
                rBgw.WorkerSupportsCancellation = true;
                rBgw.DoWork += RBgw_DoWork;
                rBgw.RunWorkerAsync();

                testTextBox.Text += "\nСервер регистрации запущен";
            }
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

        private void stopBtn_Click(object sender, RoutedEventArgs e)
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

                    testTextBox.Text += "\nСервер регистрации остановлен";
                    testTextBox.Text += "\nСервер сообщений остановлен";
                }         
            }        
        }

        private void controlBtn_Click(object sender, RoutedEventArgs e)
        {
            //Controls.HostControl host = new Controls.HostControl();
            //host.Margin = new Thickness(10);
            //field.Children.Add(host);
        }
    }
}
