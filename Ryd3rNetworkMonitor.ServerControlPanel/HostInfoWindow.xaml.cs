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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    public partial class HostInfoWindow : Window
    {
        public Host host;

        public HostInfoWindow(Host host)
        {            
            InitializeComponent();

            this.host = host;

            if (host != null)
            {
                idValueLbl.Content = !String.IsNullOrWhiteSpace(host.HostId) ? host.HostId : "no value";
                ipValueLbl.Content = !String.IsNullOrWhiteSpace(host.Ip) ? host.Ip : "no value";
                nameTxt.Text = !String.IsNullOrWhiteSpace(host.Name) ? host.Name : "no value";
                loginValueLbl.Content = !String.IsNullOrWhiteSpace(host.Login) ? host.Login : "no value";
                passValueLbl.Content = !String.IsNullOrWhiteSpace(host.Password) ? host.Password : "no value";
                printerTxt.Text = !String.IsNullOrWhiteSpace(host.PrinterMFP) ? host.PrinterMFP : "no value";

                if (host.UPS)
                    aUpsBtn.IsChecked = true;
                else
                    naUpsBtn.IsChecked = true;

                if (host.Scanner)
                    aScanBtn.IsChecked = true;
                else
                    naScanBtn.IsChecked = true;
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
    }
}
