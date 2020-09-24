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
                DbServer.DbDeleteHost(Host);
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
    }
}
