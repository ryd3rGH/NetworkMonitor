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
        public int MesPort { get; private set; }
        public bool IpDisplay { get; private set; }
        public bool LoginDisplay { get; private set; }
        public bool MessageDisplay { get; private set; }
        public int MessageCheckTime { get; private set; }
        public int HostOfflineCheckTime { get; private set; }
        public int WorkingHours { get; private set; }
        public int LogMaxLines { get; private set; }
        public bool SaveOnClose { get; private set; }
        public bool SaveOnClear { get; private set; }

        public Settings() { }

        public Settings(string ip, int mesPort, bool ipDisplay, bool loginDisplay, bool messageDisplay, int mesCheckTime, int hostOfflineCheckTime,
                        int workingHours, int logMaxLines, bool saveOnClose, bool saveOnClear)
        {
            Ip = ip;
            MesPort = mesPort;
            IpDisplay = ipDisplay;
            LoginDisplay = loginDisplay;
            MessageDisplay = messageDisplay;
            MessageCheckTime = mesCheckTime;
            HostOfflineCheckTime = hostOfflineCheckTime;
            WorkingHours = workingHours;
            LogMaxLines = logMaxLines;
            SaveOnClose = saveOnClose;
            SaveOnClear = saveOnClear;
        }

        public void GetSettings()
        {
            Ip = Properties.Settings.Default.ServerIp;
            MesPort = Properties.Settings.Default.MesPort;
            IpDisplay = Properties.Settings.Default.IpDisplay;
            LoginDisplay = Properties.Settings.Default.LoginDisplay;
            MessageDisplay = Properties.Settings.Default.MessageDisplay;
            MessageCheckTime = Properties.Settings.Default.MessagesCheckTime;
            HostOfflineCheckTime = Properties.Settings.Default.HostOfflineCheckTime;
            WorkingHours = Properties.Settings.Default.WorkingHours;
            LogMaxLines = Properties.Settings.Default.LogMaxLines;
            SaveOnClose = Properties.Settings.Default.SaveOnClose;
            SaveOnClear = Properties.Settings.Default.SaveOnClear;
        }

        public void SaveSettings(string newIp, int newMesPort, bool newIpDisplay, 
                                 bool newLoginDisplay, bool newMessageDisplay, int newMessageCheckTime,
                                 int newHostOfflineCheckTime, int newWorkingHours, int newLogMaxLines, 
                                 bool newSaveOnClose, bool newSaveOnClear)
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
            if (newWorkingHours != WorkingHours)
            {
                WorkingHours = newWorkingHours;
                Properties.Settings.Default.WorkingHours = newWorkingHours;
            }
            if (LogMaxLines != newLogMaxLines)
            {
                LogMaxLines = newLogMaxLines;
                Properties.Settings.Default.LogMaxLines = newLogMaxLines;
            }  
            if (SaveOnClose != newSaveOnClose)
            {
                SaveOnClose = newSaveOnClose;
                Properties.Settings.Default.SaveOnClose = newSaveOnClose;
            }
            if (SaveOnClear != newSaveOnClear)
            {
                SaveOnClear = newSaveOnClear;
                Properties.Settings.Default.SaveOnClear = newSaveOnClear;
            }

            Properties.Settings.Default.Save();
        }

        public override bool Equals(object obj)
        {
            Settings settingsY = (Settings)obj;

            if (this.Ip == settingsY.Ip && this.MesPort == settingsY.MesPort 
                && this.IpDisplay == settingsY.IpDisplay && this.LoginDisplay == settingsY.LoginDisplay 
                && this.MessageDisplay == settingsY.MessageDisplay && this.MessageCheckTime == settingsY.MessageCheckTime
                && this.HostOfflineCheckTime == settingsY.HostOfflineCheckTime
                && this.WorkingHours == settingsY.WorkingHours && this.LogMaxLines == settingsY.LogMaxLines
                && this.SaveOnClose == settingsY.SaveOnClose && this.SaveOnClear == settingsY.SaveOnClear)
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
