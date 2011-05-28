using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Threading;

namespace hl7service
{

	public class XMLHandler
	{
        private Thread listenThread;
		
		private string xmlFolder;
		
		private FileSystemWatcher watcher = null;

        public XMLHandler(String xmlFolder)
        {
			this.xmlFolder = xmlFolder;

			Console.WriteLine("XMLHandler: new thread started");

			this.listenThread = new Thread(new ThreadStart(WatchForFiles));
			this.listenThread.Start();
        }
		
		public void WatchForFiles()
		{
			// First process all existing xml files
			
			ProcessPreviousExistingFiles();
			
			// Now watch directory for changes
			
			CreateWatcher();
			
            while (true)
            {
				/*
                // blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                // create a thread to handle communication 
                // with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
                */
				
				System.Threading.Thread.Sleep(1000);
            }
        }
		
		public void CreateWatcher()
		{
			// Create a new FileSystemWatcher.
			watcher = new FileSystemWatcher();
	
			// Set the filter to only catch XML files.
			watcher.Filter = "*.xml";

			// Subscribe to the Created event.
			watcher.Created += new FileSystemEventHandler(watcher_FileCreated);

			// Set the path
			watcher.Path = this.xmlFolder;

			// Enable the FileSystemWatcher events.
			watcher.EnableRaisingEvents = true;
		}
		
		void watcher_FileCreated(object sender, FileSystemEventArgs e)
		{
			// A new .xml file has been created
            // Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
            // clientThread.Start(client);
			
			ProcessFile(e.FullPath);
		}
		
		void ProcessPreviousExistingFiles()
		{
			// Process the list of files found in the directory.
        	string [] fileEntries = Directory.GetFiles(xmlFolder);
			
			foreach(string fileName in fileEntries)
			{
				if (fileName.EndsWith("xml"))
				{
            		if (ProcessFile(fileName))
					{
						// File.Delete(fileName);
					}
				}
			}
		}
		
        public bool ProcessFile(string fileName)
        {
 			Console.WriteLine("XMLHandler: Reading '{0}'.", fileName);
			
			/*
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
            */
			
			return true;
        }
	}
}

