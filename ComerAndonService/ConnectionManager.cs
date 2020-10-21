using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComerAndonService
{
    class ConnectionManager
    {
        public static bool TimeOut = false;
        static string ConString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
        static string APath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static SqlConnection GetConnection()
        {
            bool writeDown = false;
            DateTime dt = DateTime.Now;
            SqlConnection conn = new SqlConnection(ConString);
            do
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    if (writeDown == false)
                    {
                        dt = DateTime.Now.AddHours(2);
                        Logger.WriteErrorLog(ex.ToString());
                        writeDown = true;
                    }
                    if (dt < DateTime.Now)
                    {
                        Logger.WriteErrorLog(ex.ToString());
                        writeDown = false;
                    }
                    Thread.Sleep(1000);
                }
            } while (conn.State != ConnectionState.Open);
            return conn;
        }
    }
}
