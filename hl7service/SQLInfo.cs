using System;
using System.IO;
using System.Data.SqlClient;
using System.Xml;

namespace hl7service
{
	public class SQLServerInfo
	{
		public string user;
		public string password;
		public string server_url;
		public bool trusted_connection;
		public string database;
		public int timeout;
				
		public string connectionString;
				
		public SQLServerInfo()
		{
			// If opciones.xml exists, we read SQL Server connection info from it
			connectionString = "";
			
			if (loadXml("Opciones.xml") == false)
			{
				loadIni("hl7service.ini");
			}
			
			Logger.Debug("Connection string: " + connectionString);
		}
		
		public void loadIni(string fileName)
		{
			// Get SQL Server connection info from INI file (or use defaults)
			
			inifile ini = new inifile(System.AppDomain.CurrentDomain.BaseDirectory + fileName);
			
			this.user = ini.getValue("database", "user", "sa");
			this.password = ini.getValue("database", "password", "123456");
			this.server_url = ini.getValue("database", "server_url", "192.168.1.3\\SQLEXPRESS");
			this.trusted_connection = ini.getValue("database", "trusted_connection", true);
			this.database = ini.getValue("database", "database", "SCA5t");
			this.timeout = ini.getValue("database", "timeout", 30);
			
			connectionString = "Data Source=" + server_url + ";Initial Catalog=" + database + ";User Id=" + user + ";Password=" + password;
		}
	
		public bool loadXml(string fileName)
		{
			fileName = System.AppDomain.CurrentDomain.BaseDirectory + fileName;
			
			if (File.Exists (fileName))
			{
				StreamReader reader = new StreamReader(fileName, System.Text.Encoding.UTF8);
				XmlTextReader xml = new XmlTextReader (reader);
				
				while(xml.Read())
				{
					if (xml.NodeType == XmlNodeType.Element && xml.Name == "constr")
					{
               			connectionString = xml.ReadString();
						xml.Close();
       				}
				}
				
				xml.Close();
			}
			
			if (connectionString.Length > 0 && connectionString.ToLower() != "none")
			{
				return true;
			}
			
			return false;
		}
		
		public bool checkConnection()
		{
			SqlConnection myConnection = new SqlConnection();		
			myConnection.ConnectionString = this.connectionString;
			
			bool allOk = true;

			try 
			{
				Logger.Debug("Trying to connect with the database server...");
				myConnection.Open();
			}
			catch(Exception e)
			{
				allOk = false;
				Logger.Fatal("Can't open connection to database server: " + e.ToString());
			}
			
			Logger.Debug("Connection established. Service running.");
			
			return allOk;
		}
	}
}
