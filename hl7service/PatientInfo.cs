using System;

using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace hl7service
{
	public class PatientInfo
	{
		public string connectionString;
		
		// SQL Patient info
		public string table = "SCAPersona";

		// HL7 v2 Patient info
		
		protected string PATIENT_ID = "PID";
		protected string FIELD_SEPARATOR = "|";

		protected StringDictionary hl7v2 = new StringDictionary();

		protected string [] hl7v2Keys = new string [] {
			"PID",
			"PatientID","ExternalID","InternalID","AlternatePatientID","PatientName","MothersMaidenName",
			"DateTimeofBirth","Sex","PatientAlias","Race","PatientAddress","CountyCode","PhoneNumberHome",
			"PhoneNumberBusiness","PrimaryLanguage","MaritalStatus","Religion","PatientAccountNumber",
			"SSNNumber","DriversLicenseNumber","MothersIdentifier","EthnicGroup","BirthPlace",
			"MultipleBirthIndicator","BirthOrder","Citizenship","VeteransMilitaryStatus",
			"Nationality","PatientDeathDateTime","PatientDeathIndicator" };
		
		protected StringDictionary sql = new StringDictionary();
					
		protected string [] sqlKeys = new string [] {
			"Tipo","Referencia","Nombre","Nombre1","Apellido1","Apellido2","NHC",
			"Field1","Field2","Field3","Field4","Field5","Field6","Field7","Field8",
			"Field9","Field10","Alta" };
	
			
		public PatientInfo()
		{
			SQLServerInfo sqlInfo = new SQLServerInfo();
			
			this.connectionString = sqlInfo.getConnectionString();
			
			hl7v2.Clear();
		}
		
		public void fromSQL()
		{
		}
		
		protected void toSQL()
		{		
		}
		
		public string fromSQLtoHL7v2()
		{
			// TODO : this is just sample code!
			/*
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
			*/		
			return "";
		}

		public string fromSQLtoHL7v3()
		{
			// TODO

			return "";
		}
		
		public void fromHL7v2toSQL(string text)
		{
			// Parse HL7 v2 Message
			
			// search for PID field
			int first = text.IndexOf(PATIENT_ID);
			
			if (first != -1)
			{
				hl7v2.Clear();
				
				// Read patient info
				
				string [] hl7 = text.Substring(first).Split(new Char [] {'|'});
				
				for (int index = 0; index < hl7v2Keys.Length; index++)
				{
					hl7v2.Add(hl7v2Keys[index], hl7[index]);
				}
			}
			
			// Now convert it to SQL
			StringDictionary sql = new StringDictionary();
			
			sql.Add("Tipo","2");

			// Calculem quins seran els camps Nombre1, Apellido1 i Apellido2
			
			string fullName = hl7v2["PatientName"];
			
			// separem per ^. Primer ve el 1r cognom i després el nom.
			string [] split = fullName.Split(new Char [] {'^'});
			
			sql.Add("Apellido1", split[0]);
			sql.Add("Apellido2", string.Empty);
			
			if (hl7v2.ContainsKey("MothersMaidenName"))
			{
				sql["Apellido2"] = hl7v2["MothersMaidenName"];
			}

			// La i comença a 1 expressament en el for perquè el primer "split" és el 1r cognom
			sql.Add("Nombre1", string.Empty);
			for (int i=1; i<split.Length; i++)
			{
				sql["Nombre1"] += split[i] + " ";
			}
			
			// Nombre = Apellido1 Apellido2, Nombre1		
			if (sql["Apellido2"].Length > 0)
			{
				sql.Add("Nombre", sql["Apellido1"] + " " + sql["Apellido2"] + "," + sql["Nombre1"]);
			}
			else
			{
				sql.Add("Nombre", sql["Apellido1"] + "," + sql["Nombre1"]);
			}
			
			// NHC
			sql.Add("NHC", string.Empty);
			if (hl7v2.ContainsKey("InternalID"))
			{
				sql["NHC"] = hl7v2["InternalID"];
			}
			
			// Alta
			
			DateTime today = DateTime.Today;

			sql.Add("Alta", today.ToString("yyyyMMdd"));
			
			storeSQL();
		}
		
		protected void storeSQL()
		{
			Logger.Debug("PatientInfo connection string: " + this.connectionString);	
					
			// Create SQL String
			// Tipo Referencia Nombre Nombre1 Apellido1 Apellido2 NHC Field1 Field2 Field3 Field4 Field5 Field6 Field7 Field8 Field9 Field10 Alta			toSQL();
			
			string sqlString = "INSERT INTO SCAPersona (Tipo, Nombre, Nombre1, Apellido1, Apellido2, NHC, ";

			string [] field_keys = new string [] { "Field1, Field2 Field3 Field4 Field5 Field6 Field7 Field8 Field9 Field10" };

			if (sql.ContainsKey("Field1"))
			{
				foreach (string key in field_keys)
				{
					sqlString += key + ", ";
				}
			}	

			sqlString += "Alta) VALUES (";
			
			sqlString += sql["Tipo"] + ",";
			sqlString += "'" + sql["Nombre"] + "',";
			sqlString += "'" + sql["Nombre1"] + "',";
			sqlString += "'" + sql["Apellido1"] + "',";
			sqlString += "'" + sql["Apellido2"] + "',";
         	sqlString += "'" + sql["NHC"] + "',";

			if (sql.ContainsKey("Field1"))
			{
				foreach (string key in field_keys)
				{
					sqlString += "'" + sql[key] + "',";
				}
			}

			sqlString += "'" + sql["Alta"] + "');";
			
			Logger.Debug("PatientInfo SQL Command: " + sqlString);			

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
				Logger.Fatal("Can't open connection to database server: " + e.ToString());
			}
		}
		
		public void fromCSVtoSQL(string text)
		{
			string [] lines = Regex.Split(text, "\r\n");
			
			foreach (string line in lines)
			{
				// Hem de recòrrer la línia. Les comes separen els camps, però s'ha de vigilar amb les
				// comes que estan dins de les cometes que marquen un string.
				
				sql.Clear();
				
				int i = 0;
				bool insideString = false;
				string field = string.Empty;
							
				foreach(string key in sqlKeys)
				{
					while (i < line.Length)
					{
						if (line[i] == '\"')
						{
							insideString = !insideString;
						}
						else if (line[i] == ',' && !insideString)
						{
							sql.Add(key, field);
							field = string.Empty;
						}
						else
						{
							field += line[i];
						}

						i++;
					}
				}
				
				storeSQL();
			}
		}

		public void fromTXTtoSQL(string text)
		{
			string [] lines = Regex.Split(text, "\r\n");
			
			foreach (string line in lines)
			{
				// each field is separated by a tab char
				string [] split = line.Split(new Char [] {'\t'});
				
				// Convert it to SQL
				sql.Clear();

				if (sqlKeys.Length == split.Length)
				{
					for (int i=0; i<split.Length; i++)
					{
						sql.Add(sqlKeys[i], split[i]);
					}
				}
				else
				{
					Logger.Fatal("Wrong file format (TXT). Remember to separate fields with a TAB character.");
				}
			
				// Alta
				
				DateTime today = DateTime.Today;
				
				sql.Add("Alta", today.ToString("yyyyMMdd"));

				storeSQL();
			}			
		}
		
		public void fromHL7v3toSQL(string xml)
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

