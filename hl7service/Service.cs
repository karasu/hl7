using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration;
using System.Configuration.Install;

using System.Net.Sockets;
using System.Threading;
using System.Net;

using System.Xml;


namespace hl7service
{
    class TCPServer
    {
        private TcpListener tcpListener;
        private Thread listenThread;

        public TCPServer()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }
        
        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }
    }

    class XMLLoader
    {
        
        public XMLLoader()
        { 

        }

        public Read(string fileName)
        {

            XmlTextReader reader = new XmlTextReader(fileName);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Console.Write("<" + reader.Name);
                        while (reader.MoveToNextAttribute())
                            Console.Write(" " + reader.Name + "='" + reader.Value + "'");
                            Console.WriteLine(">");
                        break;

                    case XmlNodeType.Text:
                        Console.WriteLine (reader.Value);
                        break;
                    case XmlNodeType.EndElement: //Mostrar el final del elemento.
                        Console.Write("</" + reader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
        }
    }

    class HL7Service:ServiceBase
    {
        public TCPServer tcpServer = null;
        public XMLLoader xmlLoader = null;

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
        }
    }
}
