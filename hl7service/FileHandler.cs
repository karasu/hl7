using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Threading;

namespace hl7service
{

	public class FileHandler
	{
        private Thread listenThread;
		
		private string folder;
		
		private FileSystemWatcher watcher = null;

        public FileHandler(String folder)
        {
			this.folder = folder;

			Console.WriteLine("FileHandler: new thread started");

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
	
			// Set the filter to only catch hl7 files.
			watcher.Filter = "*.hl7";

			// Subscribe to the Created event.
			watcher.Created += new FileSystemEventHandler(watcher_FileCreated);

			// Set the path
			watcher.Path = this.folder;

			// Enable the FileSystemWatcher events.
			watcher.EnableRaisingEvents = true;
			
			Console.WriteLine("FileHandler: FileSystemWatcher created.");
		}
		
		void watcher_FileCreated(object sender, FileSystemEventArgs e)
		{
			// A new file has been created
            // Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
            // clientThread.Start(client);
			
			ProcessFile(e.FullPath);
		}
		
		void ProcessPreviousExistingFiles()
		{
			// Process the list of files found in the directory.
        	string [] fileEntries = Directory.GetFiles(this.folder);
			
			foreach(string fileName in fileEntries)
			{
				if (fileName.EndsWith("hl7"))
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
			bool hl7v2 = false;
			bool hl7v3 = false;
			
 			Console.WriteLine("FileHandler: Reading '{0}'.", fileName);
			
			string hl7message = string.Empty;
			
			try
			{
				StreamReader infile = new StreamReader(fileName);
				
				while (!infile.EndOfStream)
				{
					hl7message += infile.ReadLine();
				}
				
			}	
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}				
			
			PatientInfo p = new PatientInfo();
			
			string searchFor = ".v2.hl7";
			
			int first = fileName.IndexOf(searchFor);
			
			if (first != -1)
			{
				hl7v2 = true;
			}
			else
			{
				searchFor = ".v3.hl7";
				
				first = fileName.IndexOf(searchFor);
				
				if (first != -1)
				{
					hl7v3 = true;
				}
			}

			if (hl7v2)
			{
				p.fromHL7v2(hl7message);
				p.store();
			}
			else if (hl7v3)
			{
				p.fromHL7v3(hl7message);
				p.store();
			}
			
			
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

