using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Ryd3rNetworkMonitor.Library
{
    public class RegistrationMessageHandler
    {
        private TcpClient Client { get; set; }

        public RegistrationMessageHandler(TcpClient client)
        {
            Client = client;
        }

        public Host HandleMessage()
        {
            NetworkStream stream = Client.GetStream();
            IFormatter formatter = new BinaryFormatter();
            string newHostGuid = Guid.NewGuid().ToString();

            Host host = (Host)formatter.Deserialize(stream);

            /* новый для сервера хост */
            if (host.HostId == string.Empty)
            {
                host.HostId = newHostGuid; //присвоен id новому хосту
                var data = Encoding.ASCII.GetBytes(newHostGuid); 
                stream.Write(data, 0, data.Length); //новый id передается клиенту
            }

            /* если у хоста есть hostId */
            else
            {
                //проверить наличие такого hostId в БД сервера
                //если такой хост есть, то host = null, клиентскому приложению отправить "OK"
                //если нет хоста с таким hostId - host != null                

                if (!DbServer.DbCheckHost(host))
                {
                    string okResponse = "OK";
                    var okData = Encoding.ASCII.GetBytes(okResponse);
                    stream.Write(okData, 0, okData.Length);

                    host = null;
                }
            }

            stream.Close();

            return host;
        }
    }
}
