using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace hl7service
{
	public class FileHandler
	{
        private Thread watchThread;
		private List <string> files = new List <string>();
		
		private string folder;
		
		private FileSystemWatcher watcher = null;

		string [] extensions = new string [] {"txt","csv","hl7"};
		
        public FileHandler(String folder)
        {
			this.folder = folder;
			
			this.watchThread = new Thread(new ThreadStart(WatchForFiles));
			this.watchThread.Start();
        }
		
		public void WatchForFiles()
		{
			// First process all existing files
			
			ProcessPreviousExistingFiles();
			
			// Now watch directory for changes
			
			CreateWatcher();
			
            while (true)
            {
				// Try to process our file list if necessary
				
				if (files.Count > 0)
				{
					foreach(string fileName in files)
					{
						ProcessFile(fileName);
					}
					
					files.Clear();
				}
				else
				{
					// Wait for changes on the designated folder
					System.Threading.Thread.Sleep(500);
				}
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

			// We don't start a new thread here, just add the new file to the queue.

			if (ExtensionRecognised(e.FullPath))
			{
				files.Add(e.FullPath);
			}
		}
		
		public void ProcessPreviousExistingFiles()
		{
			// Add to the queue the list of files found in the directory when the service is started.
			
        	string [] fileEntries = Directory.GetFiles(this.folder);
			
			foreach(string fileName in fileEntries)
			{
				if (ExtensionRecognised(fileName))
				{
					files.Add(fileName);
				}
			}
		}
		
		public bool ExtensionRecognised(string fileName)
		{
			bool recognised = false;
			
			foreach(string ext in extensions)
			{
				if (fileName.EndsWith(ext))
				{
					recognised = true;
				}
			}
			
			return recognised;
		}
		
        public void ProcessFile(string fileName)
        {
			bool hl7v2 = false;
			bool hl7v3 = false;
			
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
				
				infile.Close();
				
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
		
			// File.Delete(fileName);
			// File.Move(fileName, fileName + ".done");
        }
	}
}

