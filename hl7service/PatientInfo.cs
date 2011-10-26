using System;

using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;

using System.Xml;

// just for testing purposes
using Mono.Data.Sqlite;

using System.IO;

using Excel;

namespace hl7service
{
	public class PatientInfo
	{
		public string connectionString;
		
		// SQL Patient info
		
		//public string table = string.Empty;

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
		
		protected StringDictionary hl7v3 = new StringDictionary();
		
		protected string [] hl7v3Keys = new string [] {
			"classCode", "determinerCode", "id", "given", "family", "administrativeGenderCode", "birthTime",
			"deceasedInd", "deceasedTime", "multipleBirthInd", "multipleBirthOrderNumber", "organDonorInd",
			"maritalStatusCode", "educationLevelCode", "disabilityCode", "livingArrangementCode",
			"religiousAffiliationCode",	"raceCode", "ethnicGroupCode"};						
		
		protected string [] field_keys = new string [] {
			"Field1","Field2","Field3","Field4","Field5","Field6","Field7","Field8","Field9","Field10" };

		protected string [] optional_keys = new string [] {
			"Optional1","Optional2","Optional3","Optional4","Optional5","Optional6","Optional7","Optional8","Optional9","Optional10" };
				
		public PatientInfo()
		{
			SQLServerInfo sqlInfo = new SQLServerInfo();
			
			this.connectionString = sqlInfo.connectionString;
			
			hl7v2.Clear();
		}
		
		public bool fromHL7v2toSQL(string text)
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
			else
			{
				// It must have a PATIENT_ID
				return false;
			}
			
			// Now convert it to SQL
			StringDictionary sql = new StringDictionary();
			
			sql.Clear();
			
			sql.Add("Tipo","2");
			
			sql.Add("Referencia", "NULL");

			// Calculem quins seran els camps Nombre1, Apellido1 i Apellido2
			
			string fullName = hl7v2["PatientName"];
			
			// separem per ^. Primer ve el 1r cognom i després el nom.
			string [] split = fullName.Split(new Char [] {'^'});
			
			sql.Add("Apellido1", split[0]);
			sql.Add("Apellido2", "NULL");
			
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
			if (sql["Apellido2"] != "NULL")
			{
				sql.Add("Nombre", sql["Apellido1"] + " " + sql["Apellido2"] + "," + sql["Nombre1"]);
			}
			else
			{
				sql.Add("Nombre", sql["Apellido1"] + "," + sql["Nombre1"]);
			}
			
			// NHC
			sql.Add("NHC", "NULL");
			if (hl7v2.ContainsKey("InternalID"))
			{
				sql["NHC"] = hl7v2["InternalID"];
			}
			
			// Field fields (not used)
			
			foreach (string key in field_keys)
			{
				sql.Add(key, "NULL");
			}
			
			// Alta
			
			DateTime today = DateTime.Today;

			sql.Add("Alta", today.ToString("yyyyMMdd"));
			
			return storeSQL("SCAPersona", sql);
		}
		
		string [] getSQLKeys(string table)
		{
			string [] sqlKeys_SCAPersona = new string [] {
				"Tipo","Referencia","Nombre","Nombre1","Apellido1","Apellido2","NHC",
				"Field1","Field2","Field3","Field4","Field5","Field6","Field7","Field8",
				"Field9","Field10","Alta" };
			
			string [] sqlKeys_SCAMuestra = new string [] {
				"IdPersona", "IdEspecie", "IdCentro", "IdDoctor", "Referencia", "FechaAnalisis",
				"HoraAnalisis", "Volumen", "FechaObtencion", "HoraObtencion", "HoraEntrega",
				"IdMetodoObtencion", "DiasAbstinencia", "pH", "Temperatura", "IdColor", "IdViscosidad",
				"IdLicuefaccion", "IdAspecto", "IdOlor", "IdAglutinaciones",
				"IdOtrasPropiedades", "Observaciones", "local",
				"Other", "Confirmed", "Optional1", "Optional2", "Optional3", "Optional4",
				"Optional5", "Optional6", "Optional7", "Optional8", "Optional9", "Optional10",
				"IdCollection1", "IdCollection2" };
			
			switch(table)
			{
				case "SCAPersona": return sqlKeys_SCAPersona;
				case "SCAMuestra": return sqlKeys_SCAMuestra;
			}
			
			return null;
		}
		
		protected bool storeSQL(string table, StringDictionary sql)
		{
			// Logger.Debug("PatientInfo connection string: " + this.connectionString);	
					
			// Create SQL String
			
			// SCAPersona
			// Tipo Referencia Nombre Nombre1 Apellido1 Apellido2 NHC Field1 Field2 Field3 Field4 Field5 Field6 Field7 Field8 Field9 Field10 Alta
			
			string sqlString = "INSERT INTO " + table + " (";
			
			string [] sqlKeys = getSQLKeys(table);
			
			if (sqlKeys == null)
			{
				Logger.Debug("SQL Table " + table + "is unknown.");
				return false;
			}
			
			foreach (string key in sqlKeys)
			{
				sqlString += key + ", ";
			}

			// treu la coma final que s'ha afegit de més.
			sqlString = sqlString.TrimEnd(new Char [] {' ',','});

			sqlString += ") VALUES (";

			if (sql["ALTA"] == "NULL") // Alta can't be NULL
			{
				sql["ALTA"] = DateTime.Today.ToString("yyyyMMdd");
			}
			
			foreach (string key in sqlKeys)
			{
				if (sql[key] == "NULL")
				{
					sqlString += "NULL,";
				}
				else
				{
					sqlString += "'" + sql[key] + "',";
				}
			}

			// treu la coma final que s'ha afegit de més.
			sqlString = sqlString.TrimEnd(new Char [] {' ',','});
			
			sqlString += ");";
					
			//using sqlite3 for testing purposes
			string connectionString = "URI=file:/home/karasu/hl7/sca.sqlite3,version=3";
			SqliteConnection myConnection = new SqliteConnection(connectionString);
			
			// SqlConnection myConnection = new SqlConnection();		
			// myConnection.ConnectionString = this.connectionString;
			
			bool allOk = true;
			
			string sqlCheckNHC = string.Empty;
			
			if (table == "SCAPersona" && sql["NHC"] != "NULL")
			{
				// Before adding our patient we must check that there is not already in our database.
				// To do that, we check its NHC number
				
				sqlCheckNHC = "SELECT COUNT(*) FROM SCAPersona WHERE NHC = '" + sql["NHC"] + "'";
			}
			
			bool addIt = false;

			try 
			{
				myConnection.Open();
				Logger.Debug("Connected to SQL Server");
			}
			catch(Exception e)
			{
				Logger.Fatal("Error connecting to SQL Server: " + e.ToString());
				return false;
			}
			
			try
			{
				if (table == "SCAPersona" && sql["NHC"] != "NULL" && sqlCheckNHC.Length > 0)
				{
					SqliteCommand checkNHCCmd = new SqliteCommand(sqlCheckNHC, myConnection);
					// SqlCommand checkNHCCmd = new SqlCommand(sqlCheckNHC, myConnection);
					
					int num = 0;
					
					// num = (Int32)checkNHCCmd.ExecuteScalar();
					
					object val = checkNHCCmd.ExecuteScalar();
 
					if (val != null)
				    {
				        num = Convert.ToInt32(val.ToString());
				    }
					
					if (num == 0)
					{
						// Ok, it does not exist, we can add it.
						addIt = true;
					}
					else
					{
						Logger.Debug("Sorry, patient with NHC '" + sql["NHC"] + "' already exists in DB.");
						addIt = false;
					}
				}
				else
				{
					// NHC is null, we add it.
					// Should we check it's name here? Or it's ok to risk having duplicates ¿?
					// What about other tables? (only SCAPersona is checked)
					addIt = true;
				}
				
				if (addIt)
				{
					SqliteCommand myInsertCmd = new SqliteCommand(sqlString, myConnection);
					// SqlCommand myInsertCmd = new SqlCommand(sqlString, myConnection);

					myInsertCmd.ExecuteNonQuery();

					// Logger.Debug("SQL Command: " + sqlString);
					Logger.Debug("Insert done.");
				}

				myConnection.Close();
				
				Logger.Debug("Connection to SQL Server closed.");
				
			}
			catch(Exception e)
			{
				myConnection.Close();
				// Logger.Debug(sqlString);
				Logger.Fatal("INSERT Error: " + e.ToString());
				return false;
			}
			
			return allOk;
		}
		
		public bool fromCSVtoSQL(string text, char csv_field_delimiter)
		{
			string [] lines = text.Split(new Char [] {'\n'});
			
			foreach (string line in lines)
			{
				if (line.Length > 0)
				{
					// Logger.Debug(line);
	
					// Hem de recòrrer la línia. El caràcter csv_field_delimiter separa els camps
					// però s'ha de vigilar perquè aquest mateix caràcter pot estar dins d'un string.
					
					StringDictionary sql = new StringDictionary();
					sql.Clear();
					
					int i = 0, k = 0;
					bool insideString = false;
					string field = string.Empty;
					
					string [] sqlKeys = getSQLKeys("SCAPersona");
								
					while (i < line.Length)
					{
						if (line[i] == '\"')
						{
							insideString = !insideString;
						}
						else if (line[i] == csv_field_delimiter && !insideString)
						{
							sql.Add(sqlKeys[k++], field);
							field = string.Empty;
						}
						else
						{
							field += line[i];
						}

						i++;
					}
	
					if (storeSQL("SCAPersona", sql) == false)
					{
						return false;
					}
				}
			}

			return true;
		}

		public bool fromTXTtoSQL(string text)
		{
			string [] lines = text.Split(new Char [] {'\n'});
			
			foreach (string line in lines)
			{
				if (line.Length > 0)
				{
					// Logger.Debug("Line: " + line);
					
					// each field is separated by a tab char
					string [] split = line.Split(new Char [] {'\t'});
					
					// Convert it to SQL
					StringDictionary sql = new StringDictionary();
					sql.Clear();
					
					int keyIndex = 0;
					
					string [] sqlKeys = getSQLKeys("SCAPersona");
					
					foreach(string s in split)
					{
						sql.Add(sqlKeys[keyIndex++], s);
					}		
				
					if (storeSQL("SCAPersona", sql) == false)
					{
						return false;
					}
				}
			}			

			return true;
		}
		
		public bool fromXLStoSQL(string filePath)
		{
			Logger.Debug(filePath);
			FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		
			IExcelDataReader excelReader = null;
			
			if (filePath.EndsWith("xls"))
			{
				Logger.Debug("Reading from a binary Excel file ('97-2003 format; *.xls)");
				excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
			}
			else if (filePath.EndsWith("xlsx"))
			{
				Logger.Debug("Reading from a OpenXml Excel file (2007 format; *.xlsx)");
				excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
			}
			else
			{
				// wrong file extension (can't happen, but...)
			
				stream.Close();
				return false;
			}
			
			while (excelReader != null && excelReader.Read())
			{
				int numColumns = excelReader.FieldCount;
	
				// Logger.Debug("Found " + numColumns + " columns in excel file");
				
				if (numColumns < 2 || numColumns > 3)
				{
					// file format not supported
					stream.Close();
					return false;
				}
			
				string [] columns = new string [numColumns];

				try
				{
					for (int i=0; i<numColumns; i++)
					{
						columns[i] = excelReader.GetString(i);
						// Logger.Debug("Column " + i.ToString() + ": " + columns[i]);
					}
				}
				catch(Exception e)
				{
					Logger.Fatal("Can't read xls file: " + e.ToString());
					excelReader.Close();
					stream.Close();
					return false;
				}

				string nom = null;
				string nhc = null;
				string referencia = null;
				
				if (numColumns >= 3)
				{
					referencia = columns[0];
					nhc = columns[1];
					nom = columns[2];
				}
				else
				{
					nhc = columns[0];
					nom = columns[1];
				}
								
				if (nom != null && nhc != null)
				{
					StringDictionary sql = new StringDictionary();
						
					setSQLTableDefaults("SCAPersona", ref sql);

					sql.Add("Nombre", nom);
					sql.Add("Nombre1", nom);
					sql.Add("NHC", nhc);					
					
					if (storeSQL("SCAPersona", sql) == false)
					{
						excelReader.Close();
						stream.Close();
						return false;
					}
					
					if (numColumns >= 3 && referencia != null)
					{
						/*
						Si el xls té tres columnes, a part d’emplenar
						la taula SCAPersona tal i com ja ho fa,
						hauries d’emplenar també la taula SCAMuestra
						on la primera columna seria la referencia de la mostra.
						Tots els altres camps de SCAMuestra els hauries de posar
						amb un valor per defecte
						*/
						
						sql.Clear();
	
						setSQLTableDefaults("SCAMuestra", ref sql);
						
						sql.Add("Referencia", referencia);
						
						if (storeSQL("SCAMuestra", sql) == false)
						{
							excelReader.Close();
							stream.Close();
							return false;
						}
					}
				}
				else
				{
					Logger.Debug("Empty fields or wrong number of them. Going to next row.");
				}
			}

			excelReader.Close();	
			stream.Close();

			return true;
		}
		
		public void setSQLTableDefaults(string table, ref StringDictionary sql)
		{
			//sql = new StringDictionary();
			
			sql.Clear();				

			string today = DateTime.Today.ToString("yyyyMMdd");
			string now = DateTime.Now.ToShortTimeString();
			
			if (table == "SCAPersona")
			{
				// Setting SCAPersona defaults
				
				sql.Add("Tipo","2");		
				sql.Add("Referencia", "NULL");
				sql.Add("Apellido1", "NULL");
				sql.Add("Apellido2", "NULL");
				
				// Field fields (not used)
				foreach (string key in field_keys)
				{
					sql.Add(key, "NULL");
				}
				
				// Alta
				sql.Add("Alta", today);
			}
			else if (table == "SCAMuestra")
			{
				// Setting SCAMuestra defaults
				
				sql.Add ("IdEspecie", "NULL");
				sql.Add ("IdCentro", "1");
				sql.Add ("IdDoctor", "1");
				sql.Add ("FechaAnalisis", today);
				sql.Add ("HoraAnalisis", now);
				sql.Add ("Volumen", "2.5");
				sql.Add ("FechaObtencion", today);
				sql.Add ("HoraObtencion", now);
				sql.Add ("HoraEntrega", now);
				sql.Add ("IdMetodoObtencion", "1");
				sql.Add ("DiasAbstinencia", "3");
				sql.Add ("pH", "7.5");
				sql.Add ("Temperatura", "37");
				sql.Add ("IdColor", "1");
				sql.Add ("IdViscosidad", "1");
				sql.Add ("IdLicuefaccion", "1");
				sql.Add ("IdAspecto", "1");
				sql.Add ("IdOlor", "1");
				sql.Add ("IdAglutinaciones", "1");
				sql.Add ("IdOtrasPropiedades", "1");
				sql.Add ("Observaciones", "NULL");
				sql.Add ("local", "1");
				sql.Add ("Other", "NULL");
				sql.Add ("Confirmed", "0");
				sql.Add ("IdCollection1", "1");
				sql.Add ("IdCollection2", "1");
				
				// Optional fields (not used)
				foreach (string key in optional_keys)
				{
					sql.Add(key, "NULL");
				}
			}
		}
		
		public bool fromHL7v3toSQL(string xml)
		{
			XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(xml));
			
			hl7v3.Clear();
			
			foreach (string key in hl7v3Keys)
			{
				hl7v3.Add(key, "NULL");				
			}
			
            while (reader.Read())
            {
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (hl7v3.ContainsKey(reader.Name))
					{
						if (reader.HasValue)
						{
							hl7v3[reader.Name] = reader.Value;
						}
						else
						{
							hl7v3[reader.Name] = reader.GetAttribute("value");
						}
					}
				}
			}
			
			/*
 			"classCode", "determinerCode", "id", "given", "family", "administrativeGenderCode", "birthTime",
			"deceasedInd", "deceasedTime", "multipleBirthInd", "multipleBirthOrderNumber", "organDonorInd",
			"maritalStatusCode", "educationLevelCode", "disabilityCode", "livingArrangementCode",
			"religiousAffiliationCode",	"raceCode", "ethnicGroupCode"};
			*/
		
			// Now convert it to SQL
			StringDictionary sql = new StringDictionary();
			sql.Clear();
			
			sql.Add("Tipo", "2");
			
			sql.Add("Referencia", "NULL");

			// Calculem quins seran els camps Nombre1, Apellido1 i Apellido2
			
			sql.Add("Nombre1", hl7v3["given"]);
			
			sql.Add("Apellido1", hl7v3["family"]);
			sql.Add("Apellido2", "NULL");
			
			// Nombre = Apellido1 Apellido2, Nombre1		
			if (sql["Apellido2"] != "NULL")
			{
				sql.Add("Nombre", sql["Apellido1"] + " " + sql["Apellido2"] + "," + sql["Nombre1"]);
			}
			else
			{
				sql.Add("Nombre", sql["Apellido1"] + "," + sql["Nombre1"]);
			}
			
			// NHC
			
			sql.Add("NHC", "NULL");
			if (hl7v3.ContainsKey("id"))
			{
				sql["NHC"] = hl7v3["id"];
			}
			
			// Field fields (not used)
			
			foreach (string key in field_keys)
			{
				sql.Add(key, "NULL");
			}
			
			// Alta
			
			DateTime today = DateTime.Today;

			sql.Add("Alta", today.ToString("yyyyMMdd"));
			
			return storeSQL("SCAPersona", sql);
		}
	}
}

