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
				
		public string getConnectionString()
		{
			string connection;
			
			connection = "Data Source=" + server_url + ";Initial Catalog=" + database + ";User Id=" + user + ";Password=" + password;
			
			return connection;
		}
		
		public SQLServerInfo()
		{
			// Get defaults
			this.user = inifile.getVal("user", "sa");
			this.password = inifile.getVal("password", "123456");
			this.server_url = inifile.getVal("server_url", "192.168.1.3\\SQLEXPRESS");
			this.trusted_connection = inifile.getVal("trusted_connection", true);
			this.database = inifile.getVal("database", "SCA5t");
			this.timeout = inifile.getVal("timeout", 30);
		}
	}
}
