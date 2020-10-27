using Ryd3rNetworkMonitor.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ryd3rNetworkMonitor.ServerControlPanel.Controls
{
    public partial class HostControl : UserControl
    {
        private bool hostState;
        private int dbIntervalCount;
        
        public DateTime? DbIntervalStart { get; set; }
        public DateTime? DbIntervalEnd { get; set; }
        public int DbIntervalCount 
        { 
            get { return dbIntervalCount; }
            set 
            { 
                dbIntervalCount = value;
                if (dbIntervalCount > 3 || (dbIntervalCount > 0 && DbIntervalEnd != null))
                {
                    InsertCounts();
                    dbIntervalCount = 0;

                    DbIntervalStart = DbIntervalEnd;
                    DbIntervalEnd = null;
                }
            }
        }       
        public bool HostState { 
            
            get { return hostState; } 
            
            set
            {
                hostState = value;
                Storyboard rsb = ((Storyboard)this.FindResource("RedStoryBoard"));
                Storyboard gsb = ((Storyboard)this.FindResource("GreenStoryBoard"));

                if (hostState)
                {
                    rsb.Stop();
                    gsb.Begin();
                }
                else
                {
                    gsb.Stop();
                    rsb.Begin();
                }
            }
        }
        public int IntervalMouseClicks { get; set; }
        public int IntervalKeyPresses { get; set; }
        public Host Host { get; set; }
        public List<HostMessage> Messages { get; set; }
        public List<byte[]> Screenshots { get; set; }
        public byte[] CurrentImage { get; set; }

        public HostControl(Host host)
        {
            InitializeComponent();

            this.Host = host;
            Messages = new List<HostMessage>();

            if (host != null)
            {
                ipLbl.Content = host.Ip;
                loginLbl.Content = host.Login;
                HostState = host.IsOnline;
            }            
        }        

        public void SetDBInterval(bool isStart, DateTime dt)
        {
            if (isStart)
                DbIntervalStart = dt;
            else
                DbIntervalEnd = dt;
        }

        public void UpdateHostInfo(Host newHost)
        {
            if (newHost != null)
            {
                Host = newHost;

                ipLbl.Content = newHost.Ip;
                loginLbl.Content = newHost.Login;
                HostState = newHost.IsOnline;
            }
        }

        public void UpdateInterface(bool? ipDisp, bool? loginDisp)
        {
            if (ipDisp != null)
            {
                if ((bool)ipDisp)
                    ipLbl.Visibility = Visibility.Visible;
                else
                    ipLbl.Visibility = Visibility.Hidden; 
            }

            if (loginDisp != null)
            {
                if ((bool)loginDisp)
                    loginLbl.Visibility = Visibility.Visible;
                else
                    loginLbl.Visibility = Visibility.Hidden; 
            }
        }        

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Host != null)
            {
                this.Tag = Host;
            }
        }

        private void infoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Host != null)
            {
                this.Tag = Host;
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Host != null)
            {
                this.Tag = Host;
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF686868"));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA4A4A4"));
        }

        private void InsertCounts()
        {
            if (Messages != null && Messages.Count > 0)
            {
                List<HostMessage> tmpList = Messages.FindAll(x => x.MessageTime >= DbIntervalStart && x.MessageTime <= DbIntervalEnd);
                if (tmpList != null && tmpList.Count > 0)
                {
                    int clicks = 0;
                    int presses = 0;

                    for (int i=0; i<tmpList.Count; i++)
                    {
                        clicks += tmpList[i].InnerMessage.Clicks != null ? (int)tmpList[i].InnerMessage.Clicks : 0;
                        presses += tmpList[i].InnerMessage.KeyPresses != null ? (int)tmpList[i].InnerMessage.KeyPresses : 0;
                    }

                    IntervalMouseClicks += clicks;
                    IntervalKeyPresses += presses;

                    DbServer.DbAddMKCount(Host.HostId, DbIntervalStart, DbIntervalEnd, clicks, presses);
                }
            }
        }
    }
}
