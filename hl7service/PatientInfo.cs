using System;

using System.Data.SqlClient;


namespace hl7service
{
	public class PatientInfo
	{
		
		public string connectionString;

		// TODO: all PatientInfo properties!

		public PatientInfo ()
		{
			// TODO
		}
		
		public void fromSQL(SQLInfo i)
		{
			// TODO : this is just sample code!
			SqlConnection myConnection = new SqlConnection();
			
			myConnection.ConnectionString = i.getConnectionString();
			
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
				Console.WriteLine(e.ToString());
			}		
		}
		
		public string toSQL()
		{
			// TODO : this is just sample code!
			SqlConnection myConnection = new SqlConnection();
			
			myConnection.ConnectionString = i.getConnectionString();
			
			try 
			{
				myConnection.Open();
				
				string sqlString = "INSERT INTO table (col1, col2) VALUES (val1, 'val2')";
				
				SqlCommand myCmd = new SqlCommand(sqlString, myConnection);
				
				myCmd.ExecuteNonQuery();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			
			
		}
		
		public string toXML()
		{
			// TODO		
		}

		public string toHL7v2()
		{
			// TODO
		}

		public string toHL7v3()
		{
			// TODO
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

