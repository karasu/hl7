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
using System.IO;
using System.Collections;


namespace hl7service
{
    class TCPHandler
    {
        private Thread listenThread;

		private TcpListener tcpListener;

        public TCPHandler(int port)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            
			this.listenThread = new Thread(new ThreadStart(Listener));
            this.listenThread.Start();
        }
        
        private void Listener()
        {
            this.tcpListener.Start();

            while (true)
            {
                // blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                // create a thread to handle communication 
                // with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }
		
		private void HandleClientComm(object client)
		{
		  TcpClient tcpClient = (TcpClient)client;
		  NetworkStream clientStream = tcpClient.GetStream();
		
		  byte[] message = new byte[4096];
		  int bytesRead;
		
		  while (true)
		  {
		    bytesRead = 0;
		
		    try
		    {
		      //blocks until a client sends a message
		      bytesRead = clientStream.Read(message, 0, 4096);
		    }
		    catch
		    {
		      //a socket error has occured
		      break;
		    }
		
		    if (bytesRead == 0)
		    {
		      //the client has disconnected from the server
		      break;
		    }
		
		    //message has successfully been received
		    ASCIIEncoding encoder = new ASCIIEncoding();
		    System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
		  }
		
		  tcpClient.Close();
		}		
	    }

    class XMLHandler
    {
        private Thread listenThread;
		
		private string xmlFolder;

        public XMLHandler(String xmlFolder)
        {
			this.xmlFolder = xmlFolder;

			this.listenThread = new Thread(new ThreadStart(Listener));
			this.listenThread.Start();
        }
		
		private void Listener()
		{
			// Process the list of files found in the directory.
        	string [] fileEntries = Directory.GetFiles(xmlFolder);
			
			foreach(string fileName in fileEntries)
			{
				if (fileName.EndsWith("xml"))
				{
            		if (ProcessFile(fileName))
					{
						File.Delete(fileName);
					}
				}
			}
		}

        public bool ProcessFile(string fileName)
        {
			
 			Console.WriteLine("Reading '{0}'.", fileName);
			
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
			
			return true;
        }
    }

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
