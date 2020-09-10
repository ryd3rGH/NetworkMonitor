using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ryd3rNetworkMonitor.Library
{
    public class RegistrationServer : Server, IServer
    {
        public RegistrationServer(string ip, int port)
        {
            Ip = ip;
            Port = port;
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
                        RegistrationMessageHandler client = new RegistrationMessageHandler(newClient);

                        Host newHost = new Host();

                        Thread trd = new Thread(() => { newHost = client.HandleMessage(); });
                        trd.Start();
                        trd.Join();

                        if (newHost != null)
                        {
                            RegisterHost(newHost);
                        }
                    }
                }
            }
            catch
            {
                
            }
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
