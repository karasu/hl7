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
		private Queue _files = new Queue();
		private Queue files;
		
		private string folder;
		private char csv_field_delimiter;
		
		private FileSystemWatcher watcher = null;

		string [] extensions = new string [] {"txt","csv","hl7"};
		
        public FileHandler(String folder, char csv_field_delimiter)
        {
			this.folder = folder;
			this.csv_field_delimiter = csv_field_delimiter;
			
			files = Queue.Synchronized(_files);
			
			Logger.Debug("Starting file handler thread. Watching '" + folder + "' folder.");
			
			this.watchThread = new Thread(new ThreadStart(WatchForFiles));
			this.watchThread.Start();
        }
		
		public void WatchForFiles()
		{
			// First process all existing files
			
			AddPreviousExistingFiles();
			
			// Now watch directory for changes
			
			CreateWatcher();
			
            while (true)
            {
				// Try to process one file of our queue
				
				if (files.Count > 0)
				{
					ProcessFile(files.Dequeue().ToString());
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
			// A new file has been created, we just add it to the queue.

			if (ExtensionRecognised(e.FullPath))
			{
				Logger.Debug("Added " + e.FullPath + " to queue.");
				files.Enqueue(e.FullPath);
			}
		}
		
		public void AddPreviousExistingFiles()
		{
			// Add to the queue the list of files found in the directory when the service is started.
			
        	string [] fileEntries = Directory.GetFiles(this.folder);
			
			foreach(string fileName in fileEntries)
			{
				if (ExtensionRecognised(fileName))
				{
					Logger.Debug("Added " + fileName + " to queue.");
					files.Enqueue(fileName);
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
				p.fromCSVtoSQL(message, csv_field_delimiter);
			}
			else if (fileName.EndsWith("txt"))
			{
				p.fromTXTtoSQL(message);
			}
		
			Logger.Debug(fileName + " done.");
			// File.Move(fileName, fileName + ".done");
			File.Delete(fileName);
		}
	}
}

