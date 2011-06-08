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
				// Wait for changes on the designated folder
				System.Threading.Thread.Sleep(1000);
            }
        }
		
		public void CreateWatcher()
		{
			// Create a new FileSystemWatcher.
			watcher = new FileSystemWatcher();
	
			// Catch any file
			watcher.Filter = "*.*";

			// Subscribe to the Created event.
			watcher.Created += new FileSystemEventHandler(watcher_FileCreated);

			// Set the path
			watcher.Path = this.folder;

			// Enable the FileSystemWatcher events.
			watcher.EnableRaisingEvents = true;
		}
		
		public void watcher_FileCreated(object sender, FileSystemEventArgs e)
		{
			// A new file has been created

			// We don't start a new thread here, just process one file at a time.
			ProcessFile(e.FullPath);
		}
		
		public void ProcessPreviousExistingFiles()
		{
			// Process the list of files found in the directory when the service is started.
			
        	string [] fileEntries = Directory.GetFiles(this.folder);
			
			foreach(string fileName in fileEntries)
			{
				ProcessFile(fileName);
			}
		}
		
        public void ProcessFile(string fileName)
        {
			bool hl7v2 = false;
			bool hl7v3 = false;
			bool recognized = false;

			string [] extensions = new string [] {"txt","csv","hl7"};
			
			foreach(string ext in extensions)
			{
				if (fileName.EndsWith(ext))
				{
					recognized = true;
				}
			}
			
			if (!recognized)
			{
				Logger.Debug("Wrong format. Not a patient file");
				return;
			}
			
			string message = string.Empty;
			
			Logger.Debug("Processing " + fileName);
			
			try
			{
				StreamReader infile = new StreamReader(fileName);
				
				while (!infile.EndOfStream)
				{
					message += infile.ReadLine();
					message += "\n";
				}
				
			}	
			catch(Exception e)
			{
				Logger.Fatal(e.Message);
			}				
			
			PatientInfo p = new PatientInfo();
			
			if (fileName.EndsWith("hl7"))
			{
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
					p.fromHL7v2toSQL(message);

				}
				else if (hl7v3)
				{
					p.fromHL7v3toSQL(message);
				}

			}
			else if (fileName.EndsWith("csv"))
			{
				p.fromCSVtoSQL(message);
			}
			else if (fileName.EndsWith("txt"))
			{
				p.fromTXTtoSQL(message);
			}
		
			// everything ok, we can delete the file
			// File.Delete(fileName);

			return true;
        }
	}
}

