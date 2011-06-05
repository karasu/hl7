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

		protected StringDictionary SQLPatientInfo = new StringDictionary();
		protected string [] SQLPatientInfoKeys = new string [] {
			"IdPersona", "Tipo", "Referencia", "Nombre", "Nombre1",
			"Apellido1", "Apellido2", "NHC", "Alta" };
		
		// HL7 v2 Patient info
		
		protected string PATIENT_ID = "PID";
		protected string FIELD_SEPARATOR = "|";
		protected StringDictionary hl7v2PatientInfo = new StringDictionary();

		protected string [] hl7v2PatientInfoKeys = new string [] {
			"PID",
			"PatientID","ExternalID","InternalID","AlternatePatientID","PatientName","MothersMaidenName",
			"DateTimeofBirth","Sex","PatientAlias","Race","PatientAddress","CountyCode","PhoneNumberHome",
			"PhoneNumberBusiness","PrimaryLanguage","MaritalStatus","Religion","PatientAccountNumber",
			"SSNNumber","DriversLicenseNumber","MothersIdentifier","EthnicGroup","BirthPlace",
			"MultipleBirthIndicator","BirthOrder","Citizenship","VeteransMilitaryStatus",
			"Nationality","PatientDeathDateTime","PatientDeathIndicator" };
		
		protected StringDictionary hl7v2toSQL = new StringDictionary();
		
		public PatientInfo()
		{
			SQLServerInfo sqlInfo = new SQLServerInfo();
			
			this.connectionString = sqlInfo.getConnectionString();
			
			SQLPatientInfo.Clear();
			hl7v2PatientInfo.Clear();
			
			createHL7v2toSQL();
		}
		
		public void createHL7v2toSQL()
		{
			hl7v2toSQL.Clear();
			
			hl7v2toSQL.Add("PatientName", "Nombre");
			hl7v2toSQL.Add("MothersMaidenName", "Apellido2");
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
			string sqlString = "INSERT INTO " + this.table + " (";
			
			foreach (string key in SQLPatientInfoKeys)
			{
				sqlString += "'" + key + "',";
			}

			// removes final comma
			sqlString = sqlString.TrimEnd(new char [] {','});
			
			sqlString += ") VALUES (";
			
			// TODO: what about differences between int and string fields?
			foreach (string key in SQLPatientInfoKeys)
			{
				if (SQLPatientInfo.ContainsKey(key))
				{
					sqlString += "'" + SQLPatientInfo[key] + "',";
				}
				else
				{
					// no value given for this field
					sqlString += ",";
				}
			}
			
			// removes final comma
			sqlString = sqlString.TrimEnd(new char [] {','});
            
			sqlString += ");";

			return sqlString;
		}
		
		public void store()
		{
			// store patient info in database
			
			// get SQL
			
			string sqlString = this.toSQL();
					
			Logger.Debug("PatientInfo connection string: " + this.connectionString);
			
			Logger.Debug("PatientInfo SQL Command: " + sqlString);			
			
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
				Logger.Fatal("Can't open connection to database server: " + e.ToString());
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
						/*
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
            */

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
					hl7v2PatientInfo.Add(hl7v2PatientInfoKeys[index],s);
				}
	
				// Now get hl7v2 fields that we need and put them in our SQL fields
				SQLPatientInfo.Clear();
				
				// agafem els camps que necessitem. És molt xapussero, hauríem de tenir
				// un diccionari d'equivalències.
				
				foreach (string hl7v2key in hl7v2PatientInfoKeys)
				{
					// get SQL equivalency
					if (hl7v2toSQL.ContainsKey(hl7v2key))
					{
						string sqlkey = hl7v2toSQL[hl7v2key];
						
						SQLPatientInfo.Add(sqlkey, hl7v2PatientInfo[hl7v2key]);
					}
				}
				
			}
		}
		
		public void showStringDictionary(StringDictionary sd)
		{
			DictionaryEntry[] dict = new DictionaryEntry[sd.Count];
      		sd.CopyTo(dict, 0);

			// Displays the values in the array.
			Logger.Debug("Displays the elements in the array:" );

			for (int i=0; i<dict.Length; i++)
			{
				Logger.Debug(dict[i].Key + "=" + dict[i].Value);
			}
		}
		
		public void fromHL7v3(string xml)
		{
			// TODO		
			/*
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
            */
		}
	}
}

