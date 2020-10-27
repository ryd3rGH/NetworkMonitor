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

            HostMessage mes = (HostMessage)formatter.Deserialize(stream);

            switch (mes.InnerMessage.Type)
            {
                //обновление времени, когда хост был последний раз онлайн
                case InnerMessageTypes.Exit:
                    DbServer.DbUpdateLastOnline(mes.Host);
                    break;

                //присвоение нового HostId
                case InnerMessageTypes.Registration:
                    string newHostGuid = Guid.NewGuid().ToString();

                    if (String.IsNullOrWhiteSpace(mes.Host.HostId) || mes.Host.HostId == string.Empty)
                    {
                        mes.Host.HostId = newHostGuid;
                        var data = Encoding.ASCII.GetBytes(newHostGuid);
                        stream.Write(data, 0, data.Length);
                    }
                    break;
                
                //проверка хоста, если HostId уже есть
                case InnerMessageTypes.RegistrationCheck:
                    if (!DbServer.DbCheckHost(mes.Host))
                    {
                        string okResponse = "OK";
                        var okData = Encoding.ASCII.GetBytes(okResponse);
                        stream.Write(okData, 0, okData.Length);
                    }
                    break;
            }

            stream.Close();

            return mes;
        }
    }
}
