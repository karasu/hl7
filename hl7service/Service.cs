using System;
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
    class HL7Service:ServiceBase
    {
        public TCPHandler myTCP = null;
        public XMLHandler myXML = null;

        public HL7Service()
        {
            ServiceName = "HL7Service";
        }

        public static void Main(string[] args)
        {
            ServiceBase.Run(new HL7Service());
        }

        protected override void OnStart(string[] args)
        {
			myTCP = new TCPHandler(5757);
			myXML = new XMLHandler("xml");
        }
    }
}
