using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading;

namespace Ryd3rNetworkMonitor.Library
{
    public class DbServer : IServer
    {
        public bool Start()
        {
            try
            {
                if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db"))
                {
                    CreateDb();
                    CreateTables();
                }
                
                return true;
            }

            catch
            {
                return false;
            }
        }

        public void StartListening()
        {
            
        }

        public bool Stop()
        {
            return false;
        }

        public static void DbAddHost(Host host)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"DataSource={AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db; Version=3;"))
            {
                conn.Open();

                var ups = host.UPS ? 1 : 0;
                var scan = host.Scanner ? 1 : 0;

                using (SQLiteCommand addHost = new SQLiteCommand($"INSERT INTO [HOSTS] (HOSTID, IP, NAME, LOGIN, PASS, PRINTER, UPS, SCANNER, LASTONLINE) " +
                                                                 $"VALUES ('{host.HostId}', '{host.Ip}', '{host.Name}', '{host.Login}', '{host.Password}', '{host.PrinterMFP}', {ups}, {scan}, '{host.LastOnline.ToString("s")}')", conn))
                {
                    try
                    {
                        int res = addHost.ExecuteNonQuery();
                        Thread.Sleep(1);

                        List<Host> hosts = DbGetHosts();
                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        public static bool DbCheckHost(Host host)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"DataSource={AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db; Version=3;"))
            {
                conn.Open();

                using (SQLiteCommand checkHost = new SQLiteCommand($"SELECT COUNT(*) FROM HOSTS " +
                                                                   $"WHERE HOSTID = '{host.HostId}' AND IP = '{host.Ip}'", conn))
                {
                    var res = Convert.ToInt32(checkHost.ExecuteScalar());
                    if (res == 0)                    
                        return true;                    
                    else
                    {
                        DbUpdateHost(host);
                        return false;
                    }
                }
            }
        }

        public static bool DbDeleteHost(Host host)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"DataSource={AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db; Version=3;"))
            {
                conn.Open();

                using (SQLiteCommand delHost = new SQLiteCommand($"DELETE FROM HOSTS " +
                                                                 $"WHERE HOSTID = '{host.HostId}' and IP = '{host.Ip}'", conn))
                {
                    try
                    {
                        delHost.ExecuteNonQuery();
                        return true;
                    }
                    catch (SQLiteException)
                    {
                        return false;
                    }
                }
            }
        }

        public static List<Host> DbGetHosts()
        {
            List<Host> hosts = new List<Host>();

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection($"DataSource={AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db; Version=3;"))
                {
                    conn.Open();

                    using (SQLiteCommand getHosts = new SQLiteCommand("SELECT * FROM HOSTS", conn))
                    {
                        using (SQLiteDataReader dr = getHosts.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Host host = new Host(
                                                dr.GetString(1), dr.GetString(2), dr.GetString(3), dr.GetString(4),
                                                dr.GetString(5), dr.GetString(6), Convert.ToBoolean(dr.GetValue(7)),
                                                Convert.ToBoolean(dr.GetValue(8)), Convert.ToDateTime(dr.GetValue(9)));

                                hosts.Add(host);
                            }
                        }
                    }
                }

                return hosts;
            }
            catch (SQLiteException)
            {
                return null;
            }
        }
        
        public static void DbUpdateHost(Host host)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"DataSource={AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db; Version=3;"))
            {
                conn.Open();

                var ups = host.UPS ? 1 : 0;
                var scan = host.Scanner ? 1 : 0;

                using (SQLiteCommand updateHost = new SQLiteCommand($"UPDATE HOSTS " +
                                                                    $"SET NAME = '{host.Name}', LOGIN = '{host.Login}', PASS = '{host.Password}', PRINTER = '{host.PrinterMFP}', UPS = {ups}, SCANNER = {scan} " +
                                                                    $"WHERE HOSTID = '{host.HostId}' AND IP = '{host.Ip}'", conn))
                {
                    updateHost.ExecuteNonQuery();
                }
            }
        }

        public static void DbUpdateLastOnline(Host host)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"DataSource={AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db; Version=3;"))
            {
                conn.Open();

                using (SQLiteCommand updateOnline = new SQLiteCommand($"UPDATE HOSTS " +
                                                                      $"SET LASTONLINE = '{host.LastOnline.ToString("s")}'", conn))
                {
                    updateOnline.ExecuteNonQuery();
                }
            }
        }

        private bool CreateDb()
        {
            try
            {
                SQLiteConnection.CreateFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CreateTables()
        {

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection($"DataSource={AppDomain.CurrentDomain.BaseDirectory}\\Monitor.db; Version=3;"))
                {
                    conn.Open();

                    using (SQLiteCommand hostsTbl = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [HOSTS] (ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, HOSTID TEXT NOT NULL, IP TEXT NOT NULL, " +
                                                                      "NAME TEXT NULL, LOGIN TEXT NULL, PASS TEXT NULL, " +
                                                                      "PRINTER TEXT NULL, UPS INTEGER NULL, SCANNER INTEGER NULL, LASTONLINE TEXT NOT NULL)", conn))
                    {
                        hostsTbl.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }        
    }
}
