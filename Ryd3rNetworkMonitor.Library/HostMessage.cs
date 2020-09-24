using System;
using System.Collections.Generic;
using System.Text;

namespace Ryd3rNetworkMonitor.Library
{

    [Serializable]
    public class HostMessage
    {
        //сделать конструктор, в кот. передается объект Host и InnerMessage
        //по InnerMessage ориентироваться что делать с хостом - регистрировать, проверять регистрацию или сохранять сообщения        
        public Host Host { get; private set; }
        public InnerMessage InnerMessage { get; private set; }
        public DateTime MessageTime { get; private set; }

        public HostMessage()
        {

        }

        public HostMessage(Host host, InnerMessage message)
        {
            Host = host;
            MessageTime = DateTime.Now;
            InnerMessage = message;
        }
    }
}
