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

        public HL7Service()
        {
        }

        public static void Main(string[] args)
        {
            HL7Service hl7 = new HL7Service();
			
			string folder = "/tmp/hl7";
			
			hl7.myFile = new FileHandler(folder);			
			hl7.myTCP = new TCPHandler(8901, folder);
        }
    }
}
