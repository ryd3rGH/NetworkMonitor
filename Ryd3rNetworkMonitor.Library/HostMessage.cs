using System;
using System.Collections.Generic;
using System.Text;

namespace Ryd3rNetworkMonitor.Library
{

    [Serializable]
    public class HostMessage
    {
        public string HostId { get; private set; }
        public string Ip { get; private set; }
        public string HostName { get; private set; }
        public bool IsExit { get; private set; }
        public bool IsRegistration { get; private set; }
        public DateTime MessageTime { get; private set; }
        public bool DelFlag { get; private set; }

        public HostMessage()
        {

        }

        public HostMessage(string hostId, string ip, string name, bool isExit, bool isRegistration)
        {
            HostId = hostId;
            Ip = ip;
            HostName = name;
            IsExit = isExit;
            IsRegistration = isRegistration;
            MessageTime = DateTime.Now;
            DelFlag = false;
        }

        public void SetMessageForDelete()
        {
            DelFlag = true;
        }
    }
}
