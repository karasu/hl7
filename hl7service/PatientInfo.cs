using System;

using System.Data.SqlClient;


namespace hl7service
{
	public class PatientInfo
	{
		public string connectionString;
		
		// TODO: all PatientInfo properties!
		
		public PatientInfo()
		{
			SQLServerInfo sqlInfo = new SQLServerInfo();
			
			this.connectionString = sqlInfo.getConnectionString();
		}

		public PatientInfo (SQLServerInfo sqlInfo)
		{
			this.connectionString = sqlInfo.getConnectionString();
		}
		
		public PatientInfo (string connectionString)
		{
			this.connectionString = connectionString;
		}
		
		public void fromSQL()
		{
			// TODO : this is just sample code!
			SqlConnection myConnection = new SqlConnection();
			
			myConnection.ConnectionString = this.connectionString;
			
			try 
			{
				myConnection.Open();
			    SqlDataReader myReader = null;
			    SqlCommand    myCommand = new SqlCommand("select * from table", 
			                                             myConnection);
			    myReader = myCommand.ExecuteReader();
			    while(myReader.Read())
			    {
			        Console.WriteLine(myReader["Column1"].ToString());
			        Console.WriteLine(myReader["Column2"].ToString());
			    }
			}
			catch(Exception e)
			{
				Console.WriteLine("PatientInfo: " + e.ToString());
			}		
		}
		
		public string toSQL()
		{		
			string sqlString = "INSERT INTO table (col1, col2) VALUES (val1, 'val2')";
					
			return sqlString;
		}
		
		public void store()
		{
			// store patient info in database
			
			// get SQL
			
			string sqlString = this.toSQL();
			
			Console.WriteLine("PatientInfo: " + sqlString);
			
			// store

			SqlConnection myConnection = new SqlConnection();		
			myConnection.ConnectionString = this.connectionString;

			try 
			{

				myConnection.Open();
				
				SqlCommand myCmd = new SqlCommand(sqlString, myConnection);
				
				myCmd.ExecuteNonQuery();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public string toHL7v2()
		{
			// TODO
			return "";
		}

		public string toHL7v3()
		{
			// TODO
			return "";
		}
		
		public void fromHL7v2(string text)
		{
			// TODO	
		}
		
		public void fromHL7v3(string xml)
		{
			// TODO		
		}
	}
}

