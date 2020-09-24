using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ryd3rNetworkMonitor.Library
{
    public class MessageServer : Server, IServer
    {
        public List<HostMessage> Messages { get; set; }

        public MessageServer(string ip, int port)
        {
            Ip = ip;
            Port = port;

            Messages = new List<HostMessage>();
        }

        public bool Start()
        {
            Listener = new TcpListener(IPAddress.Parse(Ip), Port);
            Listener.Start();

            ServerState = true;

            Hosts = new List<Host>();

            if (Listener == null)
                return false;
            else
                return true;
        }

        public bool Stop()
        {
            if (Listener != null)
            {
                Listener.Stop();
                ServerState = false;
                return true;
            }

            else
                return false;
        }

        public void SetDbHosts(List<Host> hosts)
        {
            Hosts = hosts;
        }

        public void SetHostState(string hostId, bool isOnline)
        {
            if (Hosts != null && Hosts.Count > 0)
            {
                for (int i = 0; i < Hosts.Count; i++)
                {
                    if (Hosts[i].HostId == hostId)
                    {
                        Hosts[i].IsOnline = isOnline;
                        Hosts[i].LastOnline = DateTime.Now;
                        DbServer.DbUpdateLastOnline(Hosts[i]);
                        break;
                    }
                }
            }
        }

        public void StartListening()
        {
            try
            {
                if (Listener != null)
                {
                    while (true)
                    {
                        TcpClient newClient = Listener.AcceptTcpClient();
                        HostMessageHandler client = new HostMessageHandler(newClient);

                        HostMessage mes = new HostMessage();
                        Thread trd = new Thread(()=> { mes = client.HandleMessage(); });
                        trd.Start();
                        trd.Join();

                        if (mes.InnerMessage.Type == InnerMessageTypes.Registration)
                            RegisterHost(mes.Host);

                        Messages.Add(mes);
                    }
                }
            }
            catch
            {

            }
        }

        private void RegisterHost(Host host)
        {
            //записать в БД новый хост
            if (host != null)
            {
                DbServer.DbAddHost(host);
            }
        }
    }
}
