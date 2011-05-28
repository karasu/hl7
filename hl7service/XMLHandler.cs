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
}

