using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace hl7service
{
	public static class inifile
	{
		
		private static string fullPath = System.AppDomain.CurrentDomain.BaseDirectory + "hl7service.ini";
		
		public static string getVal(string key, string defaultValue)
		{
			return GetIniFileString(fullPath, "GENERAL", key, defaultValue);	
		}
		
		public static int getVal(string key, int defaultValue)
		{
			return Convert.ToInt32(GetIniFileString(fullPath, "GENERAL", key, defaultValue.ToString()));
		}

		public static bool getVal(string key, bool defaultValue)
		{
			return Convert.ToBoolean(GetIniFileString(fullPath, "GENERAL", key, defaultValue.ToString()));
		}
		
		public static void setVal(string key, string val)
		{
			SetIniFileString(fullPath, "GENERAL", key, val);	
		}

		public static void setVal(string key, int val)
		{
			SetIniFileString(fullPath, "GENERAL", key, Convert.ToString(val));	
		}
		
		public static void setVal(string key, bool val)
		{
			SetIniFileString(fullPath, "GENERAL", key, Convert.ToString(val));	
		}

        private static string GetIniFileString(string iniFile, string category, string key, string defaultValue)
        {
            return GetPrivateProfileString(category, key, defaultValue, iniFile);
        }

		private static void SetIniFileString(string iniFile, string category, string key, string val)
        {
            WritePrivateProfileString(category, key, val, iniFile);
        }

        
		private static string GetPrivateProfileString(string category, string key, string defaultValue, string iniFile)
		{
			/*
				StreamReader sr = null;
				try
				{
					// *** Clear local cache ***
					m_Sections.Clear();

					// *** Open the INI file ***
					try
					{
						sr = new StreamReader(m_FileName);
					}
					catch (FileNotFoundException)
					{
						return;
					}

					// *** Read up the file content ***
					Dictionary<string, string> CurrentSection = null;
					string s;
					while ((s = sr.ReadLine()) != null)
					{
						s = s.Trim();
						
						// *** Check for section names ***
						if (s.StartsWith("[") && s.EndsWith("]"))
						{
							if (s.Length > 2)
							{
								string SectionName = s.Substring(1,s.Length-2);
								
								// *** Only first occurrence of a section is loaded ***
								if (m_Sections.ContainsKey(SectionName))
								{
									CurrentSection = null;
								}
								else
								{
									CurrentSection = new Dictionary<string, string>();
									m_Sections.Add(SectionName,CurrentSection);
								}
							}
						}
						else if (CurrentSection != null)
						{
							// *** Check for key+value pair ***
							int i;
							if ((i=s.IndexOf('=')) > 0)
							{
								int j = s.Length - i - 1;
								string Key = s.Substring(0,i).Trim();
								if (Key.Length  > 0)
								{
									// *** Only first occurrence of a key is loaded ***
									if (!CurrentSection.ContainsKey(Key))
									{
										string Value = (j > 0) ? (s.Substring(i+1,j).Trim()) : ("");
										CurrentSection.Add(Key,Value);
									}
								}
							}
						}
					}
				}
				finally
				{
					// *** Cleanup: close file ***
					if (sr != null) sr.Close();
					sr = null;
				}
				*/
			
		}
		
	}
}

