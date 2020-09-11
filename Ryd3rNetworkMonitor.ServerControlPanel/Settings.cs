using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    public class Settings
    {
        public string Ip { get; private set; }
        public int RegPort { get; private set; }
        public int MesPort { get; private set; }
        public bool IpDisplay { get; private set; }
        public bool LoginDisplay { get; private set; }
        public bool MessageDisplay { get; private set; }
        public int MessageCheckTime { get; private set; }
        public int HostOfflineCheckTime { get; private set; }

        public Settings() { }

        public Settings(string ip, int regPort, int mesPort, bool ipDisplay, bool loginDisplay, bool messageDisplay, int mesCheckTime, int hostOfflineCheckTime)
        {
            Ip = ip;
            RegPort = regPort;
            MesPort = mesPort;
            IpDisplay = ipDisplay;
            LoginDisplay = loginDisplay;
            MessageDisplay = messageDisplay;
            MessageCheckTime = mesCheckTime;
            HostOfflineCheckTime = hostOfflineCheckTime;
        }

        public void GetSettings()
        {
            Ip = Properties.Settings.Default.ServerIp;
            RegPort = Properties.Settings.Default.RegPort;
            MesPort = Properties.Settings.Default.MesPort;
            IpDisplay = Properties.Settings.Default.IpDisplay;
            LoginDisplay = Properties.Settings.Default.LoginDisplay;
            MessageDisplay = Properties.Settings.Default.MessageDisplay;
            MessageCheckTime = Properties.Settings.Default.MessagesCheckTime;
            HostOfflineCheckTime = Properties.Settings.Default.HostOfflineCheckTime;
        }

        public void SaveSettings(string newIp, int newRegPort, int newMesPort, bool newIpDisplay, 
                                 bool newLoginDisplay, bool newMessageDisplay, int newMessageCheckTime,
                                 int newHostOfflineCheckTime)
        {
            if (newIp != Ip)
            {
                Ip = newIp;
                Properties.Settings.Default.ServerIp = newIp;
            }
            if (newRegPort != RegPort)
            {
                RegPort = newRegPort;
                Properties.Settings.Default.RegPort = newRegPort;
            }
            if (newMesPort != MesPort)
            {
                MesPort = newMesPort;
                Properties.Settings.Default.MesPort = newMesPort;
            }
            if (newIpDisplay != IpDisplay)
            {
                IpDisplay = newIpDisplay;
                Properties.Settings.Default.IpDisplay = newIpDisplay;
            }
            if (newLoginDisplay != LoginDisplay)
            {
                LoginDisplay = newLoginDisplay;
                Properties.Settings.Default.LoginDisplay = newLoginDisplay;
            }
            if (newMessageDisplay != MessageDisplay)
            {
                MessageDisplay = newMessageDisplay;
                Properties.Settings.Default.MessageDisplay = newMessageDisplay;
            }
            if (newMessageCheckTime != MessageCheckTime)
            {
                MessageCheckTime = newMessageCheckTime;
                Properties.Settings.Default.MessagesCheckTime = newMessageCheckTime;
            }
            if (newHostOfflineCheckTime != HostOfflineCheckTime)
            {
                HostOfflineCheckTime = newHostOfflineCheckTime;
                Properties.Settings.Default.HostOfflineCheckTime = newHostOfflineCheckTime;
            }

            Properties.Settings.Default.Save();
        }

        public override bool Equals(object obj)
        {
            Settings settingsY = (Settings)obj;

            if (this.Ip == settingsY.Ip && this.RegPort == settingsY.RegPort && this.MesPort == settingsY.MesPort 
                && this.IpDisplay == settingsY.IpDisplay && this.LoginDisplay == settingsY.LoginDisplay 
                && this.MessageDisplay == settingsY.MessageDisplay && this.MessageCheckTime == settingsY.MessageCheckTime
                && this.HostOfflineCheckTime == settingsY.HostOfflineCheckTime)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
