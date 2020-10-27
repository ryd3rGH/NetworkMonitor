using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading;

namespace Ryd3rNetworkMonitor.Library
{
    public class DbServer : IServer
    {
        public static string ConnStr { get; set; }

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
            using (SQLiteConnection conn = new SQLiteConnection(ConnStr))            
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
            using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
            {
                var ups = host.UPS ? 1 : 0;
                var scan = host.Scanner ? 1 : 0;

                conn.Open();

                using (SQLiteCommand checkHost = new SQLiteCommand($"SELECT COUNT(*) FROM HOSTS " +
                                                                   $"WHERE HOSTID = '{host.HostId}' AND IP = '{host.Ip}' AND NAME = '{host.Name}' AND LOGIN = '{host.Login}' " +
                                                                   $"AND PASS = '{host.Password}' AND PRINTER = '{host.PrinterMFP}' AND UPS = {ups} AND SCANNER = {scan}", conn))
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
            using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
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
                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
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
            using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
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
                                                                      $"SET LASTONLINE = '{host.LastOnline.ToString("s")}'" +
                                                                      $"WHERE HOSTID = '{host.HostId}'", conn))
                {
                    updateOnline.ExecuteNonQuery();
                }
            }
        }

        public static bool DBAddWholeTime(string hostId, DateTime dt, int workSeconds)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();
                    int count = 0;

                    using (SQLiteCommand checkDay = new SQLiteCommand($"SELECT COUNT (*) FROM [DAYS_TIME] WHERE HOSTID = '{hostId}' AND DATE = '{dt.Date.ToString("s")}'", conn))
                    {
                        count = Convert.ToInt32(checkDay.ExecuteScalar());

                        if (count > 0)
                        {
                            using (SQLiteCommand addSeconds = new SQLiteCommand($"UPDATE [DAYS_TIME] " +
                                                                                $"SET WHOLE_TIME = WHOLE_TIME + {workSeconds} " +
                                                                                $"WHERE HOSTID = '{hostId}' AND DATE = '{dt.Date.ToString("s")}'", conn))
                            {
                                addSeconds.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (SQLiteCommand insertSeconds = new SQLiteCommand($"INSERT INTO [DAYS_TIME] (HOSTID, DATE, WHOLE_TIME) " +
                                                                                   $"VALUES ('{hostId}', '{dt.Date.ToString("s")}', {workSeconds})", conn))
                            {
                                insertSeconds.ExecuteNonQuery();
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DbAddInteraval(string hostId, DateTime startTime, DateTime endTime)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();

                    using (SQLiteCommand addInterval = new SQLiteCommand($"INSERT INTO [TIME_INTERVALS] (HOSTID, START_DT, END_DT) " +
                                                                         $"VALUES ('{hostId}', '{startTime.ToString("s")}', '{endTime.ToString("s")}')", conn))
                    {
                        addInterval.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DbAddMKCount(string hostId, DateTime? start, DateTime? end, int mcl, int kcl)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();

                    using (SQLiteCommand addMk = new SQLiteCommand($"INSERT INTO [MK_COUNT] (HOSTID, START, END, MOUSE_CL, KEYBOARD_CL) " +
                                                                   $"VALUES ('{hostId}', '{start.Value.ToString("s")}', '{end.Value.ToString("s")}', {mcl}, {kcl}); SELECT last_insert_rowid();", conn))
                    {
                        addMk.ExecuteNonQuery();
                        //int clks = Convert.ToInt32(addMk.ExecuteScalar());
                        //; ;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DbGetIntervalMK(string hostId, IntervalTemplate interval, out int clicks, out int presses)
        {
            try
            {
                clicks = 0;
                presses = 0;

                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();

                    using (SQLiteCommand getCp = new SQLiteCommand($"SELECT START, END, MOUSE_CL, KEYBOARD_CL FROM [MK_COUNT] " +
                                                                   $"WHERE HOSTID = '{hostId}'", conn))
                    {
                        //using (SQLiteDataAdapter adap = new SQLiteDataAdapter(getCp))
                        //{
                        //    DataSet data = new DataSet();
                        //    adap.Fill(data);

                        //    if (data.Tables.Count > 0)
                        //    {
                        //        foreach (DataRow dr in data.Tables[0].Rows)
                        //        {
                        //            if (interval.StartTime >= Convert.ToDateTime(dr[2]) && interval.EndTime <= Convert.ToDateTime(dr[3]))
                        //            {
                        //                clicks += Convert.ToInt32(dr[4]);
                        //                presses += Convert.ToInt32(dr[5]);
                        //            }
                        //        }
                        //    }
                        //}

                        using (SQLiteDataReader dr = getCp.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                if (interval.StartTime <= Convert.ToDateTime(dr.GetValue(0)) && interval.EndTime >= Convert.ToDateTime(dr.GetValue(1)))
                                {
                                    clicks += Convert.ToInt32(dr.GetValue(2));
                                    presses += Convert.ToInt32(dr.GetValue(3));
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                clicks = 0;
                presses = 0;

                return false;
            }
        }

        public static int DbGetIntervalMK(int id)
        {
            try
            {
                int clks = 0;

                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();

                    using (SQLiteCommand getCp = new SQLiteCommand($"SELECT MOUSE_CL FROM [MK_COUNT] " +
                                                                   $"WHERE ID = '{id}'", conn))
                    {
                        using (SQLiteDataAdapter adap = new SQLiteDataAdapter(getCp))
                        {
                            clks = (int)getCp.ExecuteScalar();
                        }
                    }
                }

                return clks;
            }
            catch
            {
                return 0;
            }
        }

        public static int? DBGetDayTime(string hostId, DateTime dt)
        {
            int? wholeSeconds = null;

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();

                    using (SQLiteCommand getSeconds = new SQLiteCommand($"SELECT WHOLE_TIME FROM [DAYS_TIME] WHERE HOSTID = '{hostId}' AND DATE = '{dt.Date.ToString("s")}'", conn))
                        wholeSeconds = Convert.ToInt32(getSeconds.ExecuteScalar());
                }
            }
            catch
            {
                wholeSeconds = null;
            }

            return wholeSeconds;
        }

        public static List<IntervalTemplate> DBGetIntervals(string hostId)
        {
            List<IntervalTemplate> intervals = new List<IntervalTemplate>();

            using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();

                using (SQLiteCommand getIntervals = new SQLiteCommand($"SELECT * FROM [TIME_INTERVALS] WHERE HOSTID = '{hostId}'", conn))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(getIntervals))
                    {
                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        if (data.Tables.Count > 0)
                        {
                            foreach (DataRow dr in data.Tables[0].Rows)
                            {
                                intervals.Add(new IntervalTemplate()
                                {
                                    HostId = hostId,
                                    StartTime = dr.ItemArray[2] != null ? Convert.ToDateTime(dr.ItemArray[2]) : DateTime.MinValue,
                                    EndTime = dr.ItemArray[3] != null ? Convert.ToDateTime(dr.ItemArray[3]) : DateTime.MinValue
                                });
                            }
                        }
                    }
                }
            }               

            return intervals;
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
                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    conn.Open();

                    /* общая таблица хостов */
                    using (SQLiteCommand hostsTbl = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [HOSTS] (ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, HOSTID TEXT NOT NULL, IP TEXT NOT NULL, " +
                                                                      "NAME TEXT NULL, LOGIN TEXT NULL, PASS TEXT NULL, " +
                                                                      "PRINTER TEXT NULL, UPS INTEGER NULL, SCANNER INTEGER NULL, LASTONLINE TEXT NOT NULL)", conn))
                    {
                        hostsTbl.ExecuteNonQuery();
                    }

                    /* таблица с общим временем работы за определенные сутки, DATE - только дата */
                    using (SQLiteCommand dayTbl = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [DAYS_TIME] (ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, HOSTID TEXT NOT NULL, " +
                                                                    "DATE TEXT NOT NULL, WHOLE_TIME INTEGER NOT NULL)", conn))
                    {
                        dayTbl.ExecuteNonQuery();
                    }

                    /* таблица с периодами работы */
                    using (SQLiteCommand periodsTbl = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [TIME_INTERVALS] (ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, HOSTID TEXT NOT NULL, " +
                                                                        "START_DT TEXT NOT NULL, END_DT TEXT NOT NULL)", conn))
                    {
                        periodsTbl.ExecuteNonQuery();
                    }

                    /* таблица кликов и нажатий клавиатуры относительно периода онлайна */
                    using (SQLiteCommand countTbl = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [MK_COUNT] (ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, HOSTID TEXT NOT NULL, " +
                                                                      "START TEXT NOT NULL, END TEXT NOT NULL, MOUSE_CL INT NOT NULL, KEYBOARD_CL INT NOT NULL)", conn))
                    {
                        countTbl.ExecuteNonQuery();
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
