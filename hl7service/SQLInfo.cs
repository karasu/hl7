using System;

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
		public string table;
				
		public string getConnectionString()
		{
			string connection;
			
			connection = "Data Source=" + server_url + ";Initial Catalog=" + database + ";User Id=" + user + ";Password=" + password;
			
			return connection;
		}
			
		public SQLServerInfo()
		{
			// Get defaults
			inifile ini = new inifile(System.AppDomain.CurrentDomain.BaseDirectory + "hl7service.ini");
			
			this.user = ini.getValue("database", "user", "sa");
			this.password = ini.getValue("database", "password", "123456");
			this.server_url = ini.getValue("database", "server_url", "192.168.1.3\\SQLEXPRESS");
			this.trusted_connection = ini.getValue("database", "trusted_connection", true);
			this.database = ini.getValue("database", "database", "SCA5t");
			this.timeout = ini.getValue("database", "timeout", 30);
			this.table = ini.getValue("database", "table", "SCAPersona");
		}
	}
}
