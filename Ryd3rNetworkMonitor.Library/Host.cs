using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Ryd3rNetworkMonitor.Library
{
    [Serializable]
    public class Host
    {
        public string HostId { get; set; }
        public string Ip { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PrinterMFP { get; set; }
        public bool UPS { get; set; }
        public bool Scanner { get; set; }
        public DateTime LastOnline { get; set; }
        public bool IsOnline { get; set; }

        public Host()
        {

        }

        public Host(string hostId, string ip, string name, string login, string password, string printerMfp, bool ups, bool scanner, DateTime online)
        {
            HostId = hostId;
            Ip = ip;
            Name = name;
            Login = login;
            Password = password;
            PrinterMFP = printerMfp;
            UPS = ups;
            Scanner = scanner;
            LastOnline = online;
        }

        /* создание объекта Host из xml-файла */
        public Host(string xmlFilePath)
        {
            XmlDocument file = new XmlDocument(); 
            file.Load(xmlFilePath);

            HostId = file.DocumentElement.SelectSingleNode("/HOST/HOSTID").InnerText;
            Ip = file.DocumentElement.SelectSingleNode("/HOST/IP").InnerText;
            Name = file.DocumentElement.SelectSingleNode("/HOST/NAME").InnerText;
            Login = file.DocumentElement.SelectSingleNode("/HOST/LOGIN").InnerText;
            Password = file.DocumentElement.SelectSingleNode("/HOST/PASS").InnerText;
            PrinterMFP = file.DocumentElement.SelectSingleNode("/HOST/PRINTER").InnerText;
            UPS = Convert.ToBoolean(file.DocumentElement.SelectSingleNode("/HOST/UPS").InnerText);
            Scanner = Convert.ToBoolean(file.DocumentElement.SelectSingleNode("/HOST/SCANNER").InnerText);
            LastOnline = Convert.ToDateTime(file.DocumentElement.SelectSingleNode("/HOST/LASTONLINE").InnerText); 
        }

        public bool InsertHostIdToFile()
        {
            try
            {
                XmlDocument file = new XmlDocument();
                file.Load(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml");

                file.DocumentElement.SelectSingleNode("/HOST/HOSTID").InnerText = this.HostId;

                file.Save(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml");

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public bool SaveHostFile(string filePath)
        {
            try
            {
                var file = new XDocument();

                var rootElement = new XElement("HOST");

                var hostIdElement = new XElement("HOSTID", HostId);
                rootElement.Add(hostIdElement);

                var ipElement = new XElement("IP", Ip);
                rootElement.Add(ipElement);

                var nameElement = new XElement("NAME", Name);
                rootElement.Add(nameElement);

                var loginElement = new XElement("LOGIN", Login);
                rootElement.Add(loginElement);

                var passElement = new XElement("PASS", Password);
                rootElement.Add(passElement);

                var printerElement = new XElement("PRINTER", PrinterMFP);
                rootElement.Add(printerElement);

                var upsElement = new XElement("UPS", UPS ? "TRUE" : "FALSE");
                rootElement.Add(upsElement);

                var scanElement = new XElement("SCANNER", Scanner ? "TRUE" : "FALSE");
                rootElement.Add(scanElement);

                var onlineElement = new XElement("LASTONLINE", LastOnline != null ? LastOnline.ToString("s") : DateTime.Now.ToString("s"));
                rootElement.Add(onlineElement);

                file.Add(rootElement);

                file.Save(filePath + "\\host.xml");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateLastOnlineTime()
        {
            try
            {
                XmlDocument file = new XmlDocument();
                file.Load(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml");

                file.DocumentElement.SelectSingleNode("/HOST/LASTONLINE").InnerText = DateTime.Now.ToString("s");;

                file.Save(AppDomain.CurrentDomain.BaseDirectory + "\\host.xml");

                return true;
            }
            catch
            {
                return false;
            }
        }
                
        public override bool Equals(Object y)
        {
            if (HostId == ((Host)y).HostId)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
