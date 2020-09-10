using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ryd3rNetworkMonitor.Library
{
    public abstract class Server
    {
        public string Ip { get; protected set; }
        public int Port { get; protected set; }
        public TcpListener Listener { get; protected set; }
        public bool ServerState { get; protected set; }
        public List<Host> Hosts { get; protected set; }        
    }
}
