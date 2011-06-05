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
	
			// Set the filter to only catch our hl7 files.
			watcher.Filter = "*.hl7";

			// Subscribe to the Created event.
			watcher.Created += new FileSystemEventHandler(watcher_FileCreated);

			// Set the path
			watcher.Path = this.folder;

			// Enable the FileSystemWatcher events.
			watcher.EnableRaisingEvents = true;
		}
		
		void watcher_FileCreated(object sender, FileSystemEventArgs e)
		{
			// A new file has been created

			// We don't start a new thread here, just process one file at a time.
			ProcessFile(e.FullPath);
		}
		
		void ProcessPreviousExistingFiles()
		{
			// Process the list of files found in the directory when the service is started.
			
        	string [] fileEntries = Directory.GetFiles(this.folder);
			
			foreach(string fileName in fileEntries)
			{
				if (fileName.EndsWith("hl7"))
				{
            		if (ProcessFile(fileName))
					{
						// once processed we delete it.
						// File.Delete(fileName);
					}
				}
			}
		}
		
        public bool ProcessFile(string fileName)
        {
			bool hl7v2 = false;
			bool hl7v3 = false;
			
			string hl7message = string.Empty;
			
			Logger.Debug("Processing " + fileName);
			
			try
			{
				StreamReader infile = new StreamReader(fileName);
				
				while (!infile.EndOfStream)
				{
					hl7message += infile.ReadLine();
					hl7message += "\n";
				}
				
			}	
			catch(Exception e)
			{
				Logger.Fatal(e.Message);
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
			else
			{
				return false;
			}
		
			return true;
        }
	}
}

