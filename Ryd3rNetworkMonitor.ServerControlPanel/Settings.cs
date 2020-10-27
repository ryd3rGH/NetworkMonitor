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

        public void GetSettings()
        {
            Ip = Properties.Settings.Default.ServerIp;
            RegPort = Properties.Settings.Default.RegPort;
            MesPort = Properties.Settings.Default.MesPort;
        }

        public void SaveSettings(string newIp, int newRegPort, int newMesPort)
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

            Properties.Settings.Default.Save();
        }

        public override bool Equals(object obj)
        {
            Settings settingsY = (Settings)obj;

            if (this.Ip == settingsY.Ip && this.RegPort == settingsY.RegPort && this.MesPort == settingsY.MesPort)
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
