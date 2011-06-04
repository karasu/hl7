using System;

using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;


namespace hl7service
{
	public class PatientInfo
	{
		public string connectionString;
		
		// SQL Patient info
		public string table = "SCAPersona";

		public StringDictionary SQLPatientInfo = new StringDictionary();
		public string [] SQLPatientInfoKeys = new string [] {
			"IdPersona", "Tipo", "Referencia", "Nombre", "Nombre1",
			"Apellido1", "Apellido2", "NHC", "Alta" };
		
		// HL7 v2 Patient info
		
		public string PATIENT_ID = "PID";
		public string FIELD_SEPARATOR = "|";
		public StringDictionary hl7v2PatientInfo = new StringDictionary();

		public string [] hl7v2PatientInfoKeys = new string [] {
			"PID",
			"PatientID","ExternalID","InternalID","AlternatePatientID","PatientName","MothersMaidenName",
			"DateTimeofBirth","Sex","PatientAlias","Race","PatientAddress","CountyCode","PhoneNumberHome",
			"PhoneNumberBusiness","PrimaryLanguage","MaritalStatus","Religion","PatientAccountNumber",
			"SSNNumber","DriversLicenseNumber","MothersIdentifier","EthnicGroup","BirthPlace",
			"MultipleBirthIndicator","BirthOrder","Citizenship","VeteransMilitaryStatus",
			"Nationality","PatientDeathDateTime","PatientDeathIndicator" };
		
		 
		public PatientInfo()
		{
			SQLServerInfo sqlInfo = new SQLServerInfo();
			
			this.connectionString = sqlInfo.getConnectionString();
			
			SQLPatientInfo.Clear();
			hl7v2PatientInfo.Clear();
		}

		public PatientInfo (SQLServerInfo sqlInfo)
		{
			this.connectionString = sqlInfo.getConnectionString();
			
			SQLPatientInfo.Clear();
			hl7v2PatientInfo.Clear();
		}
		
		public PatientInfo (string connectionString)
		{
			this.connectionString = connectionString;
			
			SQLPatientInfo.Clear();
			hl7v2PatientInfo.Clear();
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
			// TODO must be rewritten using StringDictionary
			string sqlString = "INSERT INTO " + this.table + " ( " +
				"IdPersona,Tipo,Referencia,Nombre,Nombre1,Apellido1,Apellido2,NHC,Alta) " +
				"VALUES (" +
				this.IdPersona.ToString() + "," +
				this.Tipo.ToString() + "," +
				"'" + this.Referencia + "'," +
                "'" + this.Nombre + "'," +
                "'" + this.Nombre1 + "'," +
                "'" + this.Apellido1 + "'," +
                "'" + this.Apellido2 + "'," +
                "'" + this.NHC + "'," +
                "'" + this.Alta.ToString() + "');";

			return sqlString;
		}
		
		public void store()
		{
			// store patient info in database
			
			// get SQL
			
			string sqlString = this.toSQL();
					
			Console.WriteLine("PatientInfo connection string: " + this.connectionString);
			
			Console.WriteLine("PatientInfo SQL Command: " + sqlString);			
			
			// store

			SqlConnection myConnection = new SqlConnection();		
			myConnection.ConnectionString = this.connectionString;

			try 
			{

				myConnection.Open();
				
				Console.WriteLine("Connection opened");
				
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
			// Parse HL7 v2 Message
			int first = text.IndexOf(PATIENT_ID);
			
			if (first != -1)
			{
				hl7v2PatientInfo.Clear();
				
				// Read patient info
				
				string [] split = text.Substring(first).Split(new Char [] {'|'});
				
				for (int index = 0; index < hl7v2PatientInfoKeys.Length; index++)
				{
					string s = split[index];
					Console.WriteLine(s);
					hl7v2PatientInfo.Add(hl7v2PatientInfoKeys[index],s);
				}
			}
			
			// Now get hl7v2 fields that we need and put them in our SQL fields
			
			
		}
		
		public void fromHL7v3(string xml)
		{
			// TODO		
		}
	}
}

