using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration;
using System.Configuration.Install;

using System.Threading;

namespace hl7service
{
	static class Logger 
	{
		private static string fullPath = System.AppDomain.CurrentDomain.BaseDirectory + "hl7service.log";
			                
		public static void Debug (string str)
		{
			DebugFormat (str);
		}

		public static void Fatal (string str)
		{
			DebugFormat (str);
		}

		public static void DebugFormat (string str, params object [] args)
		{
			try
			{
				StreamWriter writer = File.AppendText(fullPath);
				writer.WriteLine(DateTime.Now.ToLongTimeString() + ": " + str, args);
				writer.Flush();
				writer.Close();
				
				Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + str, args);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
	
	
	/*		
    class HL7Service:ServiceBase
    {
        public TCPHandler myTCP = null;
        public FileHandler myFile = null;

        public HL7Service()
        {
            ServiceName = "hl7service";
        }

        public static void Main(string[] args)
        {
            ServiceBase.Run(new HL7Service());
        }

        protected override void OnStart(string[] args)
        {
			string folder = "/tmp/hl7";
			
			this.myFile = new FileHandler(folder);			
			this.myTCP = new TCPHandler(8901, folder);
        }
    }
*/
	
    class HL7Service
    {
        public TCPHandler myTCP = null;
        public FileHandler myFile = null;
		
		public string folder;
		public int port;
		public char csv_field_delimiter;

        public HL7Service()
        {
			inifile ini = new inifile(System.AppDomain.CurrentDomain.BaseDirectory + "hl7service.ini");
			
			folder = ini.getValue("service", "folder", "/tmp/hl7");
			port = ini.getValue("service", "port", 8901);
			csv_field_delimiter = ini.getValue("service", "csv", ',');
        }

		public void createDefaultIni()
		{
			inifile ini = new inifile(System.AppDomain.CurrentDomain.BaseDirectory + "hl7service.ini");
			
			ini.setValue("database", "user", "sa");
			ini.setValue("database", "password", "123456");
			ini.setValue("database", "server_url", "192.168.1.3\\SQLEXPRESS");
			ini.setValue("database", "trusted_connection", true);
			ini.setValue("database", "database", "SCA5t");
			ini.setValue("database", "timeout", 30);
			ini.setValue("database", "table", "SCAPersona");
			ini.setValue("service", "folder", "/tmp/hl7");
			ini.setValue("service", "port", 8901);
			ini.setValue("service", "csv", ',');
			
			ini.Save(System.AppDomain.CurrentDomain.BaseDirectory + "hl7service.ini");
		}
		
		public bool checkSQLConnection()
		{
			/*
			SQLServerInfo sqlInfo = new SQLServerInfo();
			return sqlInfo.checkConnection();
			*/
			return true;
		}
		
		public void run()
		{
			if (checkSQLConnection())
			{
				myFile = new FileHandler(folder, csv_field_delimiter);			
				myTCP = new TCPHandler(port, folder);
			}
			else
			{
				Logger.Fatal("Can't connect to the SQL Database server");
			}
		}

        public static void Main(string[] args)
        {
            HL7Service hl7 = new HL7Service();
			
			// hl7.createDefaultIni();
			
			hl7.run();
        }
    }
}
