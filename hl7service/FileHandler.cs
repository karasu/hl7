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

		string [] extensions = new string [] {"txt", "csv", "hl7", "xls", "xlsx" };
		
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
			
			EnqueueExistingFiles();
			
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
				files.Enqueue(e.FullPath);
				Logger.Debug("Added " + e.FullPath + " to queue.");
			}
		}
		
		public void EnqueueExistingFiles()
		{
			// Add to the queue the list of files found in the directory when the service is started.
			
        	string [] fileEntries = Directory.GetFiles(this.folder);
			
			foreach(string fileName in fileEntries)
			{
				if (ExtensionRecognised(fileName))
				{
					files.Enqueue(fileName);
					Logger.Debug("Added " + fileName + " to queue.");
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
			
			bool SQLstored = false;
			
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
				
				if (hl7v2 == false && hl7v3 == false)
				{
					// Not a TCP received file. Perhaps somebody has put it in our directory.
					// Let's assume it is a hl7v2. If it isn't fromHL7v2toSQL will simply fail and return false :p
					hl7v2 = true;
				}
	
				if (hl7v2)
				{
					SQLstored = p.fromHL7v2toSQL(message);

				}
				else if (hl7v3)
				{
					SQLstored = p.fromHL7v3toSQL(message);
				}

			}
			else if (fileName.EndsWith("csv"))
			{
				SQLstored = p.fromCSVtoSQL(message, csv_field_delimiter);
			}
			else if (fileName.EndsWith("txt"))
			{
				SQLstored = p.fromTXTtoSQL(message);
			}
			else if (fileName.EndsWith("xls") || fileName.EndsWith("xlsx"))
			{
				SQLstored = p.fromXLStoSQL(fileName);
			}
		
			if (SQLstored)
			{
				Logger.Debug(fileName + " done!");
				// File.Move(fileName, fileName + ".done");
				File.Delete(fileName);
			}
			else
			{
				// Something bad happened (wrong data, wrong SQL...)
				// We put it at the end of the queue to try again later.
				Logger.Debug("Error with " + fileName + ", putting it at the end of the queue.");
				files.Enqueue(fileName);
			}
		}
	}
}

