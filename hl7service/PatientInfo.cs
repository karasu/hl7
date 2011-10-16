using System;

using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;

using System.Xml;


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
			
			switch(table)
			{
				case "SCAPersona": return sqlKeys_SCAPersona;
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
			
			Logger.Debug("SQL Command: " + sqlString);

			SqlConnection myConnection = new SqlConnection();		
			myConnection.ConnectionString = this.connectionString;
			
			bool allOk = true;
			
			string sqlCheckNHC = string.Empty;
			
			if (sql["NHC"] != "NULL")
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
				
				if (sql["NHC"] != "NULL" && sqlCheckNHC.Length > 0)
				{
					SqlCommand checkNHCCmd = new SqlCommand(sqlCheckNHC, myConnection);
					if ((Int32)checkNHCCmd.ExecuteScalar() == 0)
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
					addIt = true;
				}
				
				if (addIt)
				{
					SqlCommand myInsertCmd = new SqlCommand(sqlString, myConnection);
					myInsertCmd.ExecuteNonQuery();
				}

				myConnection.Close();
				
				Logger.Debug("Connection to SQL Server closed");
			}
			catch(Exception e)
			{
				allOk = false;
				Logger.Debug(sqlString);
				Logger.Fatal("Can't open connection to database server: " + e.ToString());
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

			Logger.Debug("Reading from a binary Excel file ('97-2003 format; *.xls)");
			IExcelDataReader excelReader;
			
			if (filePath.EndsWith("xls"))
			{
				excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
			}
			else if (filePath.EndsWith("xlsx"))
			{
				excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
			}

			/*
				 // Tipo Referencia Nombre Nombre1 Apellido1 Apellido2 NHC Field1 Field2 Field3 Field4 Field5 Field6 Field7 Field8 Field9 Field10 Alta 
				Per defecte els valors seran:
				Tipo = 2
				Referencia = NULL
				Nombre = Segona columna
				Nombre1 = Segona columna
				Apellido1 = NULL
				Apellido2 = NULL
				NHC = Primera Columna
				FieldX = NULL
				Alta = Data actual
			*/
					
			while (excelReader.Read())
			{
				//excelReader.GetInt32(0);
				
				string c1 = excelReader.GetString(0);
				string c2 = excelReader.GetString(1);
				string c3 = excelReader.GetString(2);

				string nom = "";
				string nhc = "";
				string refmuestra = "";
				
				if (c1 != null && c2 != null)
				{
					if (c3 != null)
					{
						refmuestra = c1;
						nhc = c2;
						nom = c3;
					}
					else
					{
						nhc = c1;
						nom = c2;
					}
					// Logger.Debug(c1);
					// Logger.Debug(c2);
					
					StringDictionary sql = new StringDictionary();
					sql.Clear();				
					
					sql.Add("Tipo","2");		
					sql.Add("Referencia", "NULL");
					sql.Add("Nombre", nom);
					sql.Add("Nombre1", nom);
					sql.Add("Apellido1", "NULL");
					sql.Add("Apellido2", "NULL");
					sql.Add("NHC", nhc);					
					
					// Field fields (not used)

					foreach (string key in field_keys)
					{
						sql.Add(key, "NULL");
					}
					
					// Alta
					
					DateTime today = DateTime.Today;
		
					sql.Add("Alta", today.ToString("yyyyMMdd"));
					
					if (storeSQL("SCAPersona", sql) == false)
					{
						return false;
					}
					
					if (c3 != null)
					{
						/*
						Si el xls té tres columnes, a part d’emplenar la taula SCAPersona tal i com ja ho fa,
						hauries d’emplenar també la taula SCAMuestra on la primera columna seria la referencia de la mostra.
						Tots els altres camps de SCAMuestra els hauries de posar amb un valor per defecte
						*/
						
						sql.Clear();
						
						sql.Add ("","");
						sql.Add ("Referencia", "NULL");
						
						if (storeSQL("SCAMuestra", sql) == false)
						{
							return false;
						}
					}
				}
			}

			excelReader.Close();

			return true;
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

