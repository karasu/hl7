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
			using(StreamWriter writer = File.AppendText(fullPath))
			{
				writer.WriteLine(DateTime.Now.ToLongTimeString() + ": " + str, args);
				Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + str, args);
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

        public HL7Service()
        {
			inifile ini = new inifile(System.AppDomain.CurrentDomain.BaseDirectory + "hl7service.ini");
			
			ini.setValue("database","user", "sa");
			ini.setValue("database","password", "123456");
			ini.setValue("database","server_url", "192.168.1.3\\SQLEXPRESS");
			ini.setValue("database","trusted_connection", true);
			ini.setValue("database","database", "SCA5t");
			ini.setValue("database","timeout", 30);
			ini.setValue("database","Table", "SCAPersona");

			ini.setValue("service","folder", "/tmp/hl7");
			ini.setValue("service","port", 8901);
			
			folder = ini.getValue("service", "folder", "/tmp/hl7");
			port = ini.getValue("service", "port", 8901);
			
			myFile = new FileHandler(folder);			
			
			myTCP = new TCPHandler(port, folder);
        }

        public static void Main(string[] args)
        {
            HL7Service hl7 = new HL7Service();
			
        }
    }
}
