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
			// Set defaults
			this.user = "sa";
			this.password = "123456";
			this.server_url = "192.168.1.3\\SQLEXPRESS";
			this.trusted_connection = true;
			this.database = "SCA5t";
			this.timeout = 30;
		}
	}
}
