using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Ryd3rNetworkMonitor.Library
{
    public class HostMessageHandler
    {
        private TcpClient Client { get; set; }

        public HostMessageHandler(TcpClient client)
        {
            Client = client;
        }

        public HostMessage HandleMessage()
        {
            NetworkStream stream = Client.GetStream();
            IFormatter formatter = new BinaryFormatter();

            //Host host = (Host)formatter.Deserialize(stream);
            HostMessage mes = (HostMessage)formatter.Deserialize(stream);
            return mes;
        }
    }
}
