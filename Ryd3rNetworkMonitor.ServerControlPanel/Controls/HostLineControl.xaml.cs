﻿using Ryd3rNetworkMonitor.Library;
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
    /// <summary>
    /// Interaction logic for HostLineControl.xaml
    /// </summary>
    public partial class HostLineControl : UserControl
    {
        private bool hostState;
        public bool HostState
        {

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

        public HostLineControl(Host host)
        {
            InitializeComponent();

            this.Host = host;
            Messages = new List<HostMessage>();

            if (host != null)
            {
                ipLbl.Content = host.Ip;
                loginLbl.Content = host.Login;
                printerLbl.Content = host.PrinterMFP != null ? host.PrinterMFP : "no printer of MFP";

                if (host.UPS)
                    upsBox.IsChecked = true;

                if (host.Scanner)
                    scannerBox.IsChecked = true;

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
    }
}
