using System;

namespace hl7service
{
	public class SQLInfo
	{
		public string user;
		public string password;
		public string server_url;
		public bool trusted_connection;
		public string database;
		public int timeout;
		
		public string getConnectionString()
		{
			string connectionString;
			string trusted;
			
			if (trusted_connection)
			{
				trusted = "yes";
			}
			else
			{
				trusted = "no";
			}
			
			connectionString = "user id=" + user + ";" + 
                               "password=" + password + ";" +
			                   "server=" + server_url + ";" + 
                               "Trusted_Connection=" + trusted + ";" + 
                               "database=" + database + "; " + 
                               "connection timeout=" + timeout.ToString();
			
			return connectionString;
		}
		
		public SQLInfo()
		{
			// TODO: Set defaults
			this.user = "karasu";
			this.password = "123456";
			this.server_url = "localhost";
			this.trusted_connection = true;
			this.database = "";
			this.timeout = 30;
		}
}
