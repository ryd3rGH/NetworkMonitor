using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ryd3rNetworkMonitor.Library;

namespace Ryd3rNetworkMonitor.ClientControlPanel
{
    public class Settings
    {
        public string Ip { get; private set; }
        public int MesPort { get; private set; }
        public int SendInterval { get; set; }
        public bool SendOnClick { get; set; }
        public Host Host { get; set; }

        public void GetSettings()
        {
            Ip = Properties.Settings.Default.ServerIp;
            MesPort = Properties.Settings.Default.MesPort;
            SendInterval = Properties.Settings.Default.SendInterval;
            SendOnClick = Properties.Settings.Default.SendOnClick;

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml"))
            {
                Host = new Host(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml");
            }
        }

        public void SaveSettings(string newIp, int newMesPort, int newInterval, bool newSendOnClick, Host newHostInfo)
        {
            if (newIp != Ip)
            {
                Ip = newIp;
                Properties.Settings.Default.ServerIp = newIp;
            }

            if (newMesPort != MesPort)
            {
                MesPort = newMesPort;
                Properties.Settings.Default.MesPort = newMesPort;
            }

            if (newInterval != SendInterval)
            {
                SendInterval = newInterval;
                Properties.Settings.Default.SendInterval = newInterval;
            }

            if (newSendOnClick != SendOnClick)
            {
                SendOnClick = newSendOnClick;
                Properties.Settings.Default.SendOnClick = newSendOnClick;
            }

            Properties.Settings.Default.Save();

            if (Host != null)
            {
                if (!Host.Equals(newHostInfo) || Host.Name != newHostInfo.Name || Host.Login != newHostInfo.Login ||
                        Host.Password != newHostInfo.Password || Host.PrinterMFP != newHostInfo.PrinterMFP ||
                        Host.UPS != newHostInfo.UPS || Host.Scanner != newHostInfo.Scanner)
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\host.xaml");
                    Host = newHostInfo;
                    newHostInfo.SaveHostFile(AppDomain.CurrentDomain.BaseDirectory);
                } 
            }
            else
            {
                Host = new Host(
                        string.Empty,
                        GetLocalIPAddress(),
                        newHostInfo.Name, newHostInfo.Login, newHostInfo.Password,newHostInfo.PrinterMFP,
                        true,
                        false,
                        DateTime.Now);

                Host.SaveHostFile(AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        public override bool Equals(object obj)
        {
            Settings settingsY = (Settings)obj;

            if (this.Ip == settingsY.Ip && this.MesPort == settingsY.MesPort 
                && this.SendInterval == settingsY.SendInterval && settingsY.Host.Equals(Host))
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static string GetLocalIPAddress()
        {
            string ipAdr = string.Empty;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAdr = ip.ToString();
                }
            }

            if (ipAdr != string.Empty)
                return ipAdr;
            else
                return "0.0.0.0";
        }
    }
}
