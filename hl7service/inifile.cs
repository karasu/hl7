using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace hl7service
{
    class inifile
    {
        Dictionary<string, NameValueCollection> data = 
            new Dictionary<string,NameValueCollection>();

        static readonly Regex regRemoveEmptyLines =
            new Regex
            (
                @"(\s*;[\d\D]*?\r?\n)+|\r?\n(\s*\r?\n)*", 
                RegexOptions.Multiline | RegexOptions.Compiled
            );

        static readonly Regex regParseIniData =
            new Regex
            (
                @"
                (?<IsSection>
                    ^\s*\[(?<SectionName>[^\]]+)?\]\s*$
                )
                |
                (?<IsKeyValue>
                    ^\s*(?<Key>[^(\s*\=\s*)]+)?\s*\=\s*(?<Value>[\d\D]*)$
                )",
                RegexOptions.Compiled | 
                RegexOptions.IgnoreCase | 
                RegexOptions.IgnorePatternWhitespace
            );

        public inifile()
        {
            readIniData(null, null);
        }

        public inifile(string fileName)
            : this(fileName, Encoding.UTF8)
        { }

        public inifile(string fileName, Encoding encoding)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                readIniData(fs, encoding);
            }
        }

        private void readIniData(Stream stream, Encoding encoding)
        {
            string lastSection = string.Empty;
            
			data.Add(lastSection, new NameValueCollection());
            
			if (stream != null && encoding != null)
            {
                string iniData;
                using 
                (
                    StreamReader reader = 
                        new StreamReader(stream, encoding)
                )
                    iniData = reader.ReadToEnd();

                iniData = regRemoveEmptyLines.Replace(iniData, "\n");
                
                string [] lines = iniData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in lines)
                {
                    Match m = regParseIniData.Match(s);
                    if (m.Success)
                    {
                        if (m.Groups["IsSection"].Length > 0)
                        {
                            string sName = m.Groups["SectionName"].Value.ToLowerInvariant();

                            if (lastSection != sName)
                            {
                                lastSection = sName;
                                if (!data.ContainsKey(sName))
                                {
                                    data.Add(sName, new NameValueCollection());
                                }
                            }
                        }
                        else if (m.Groups["IsKeyValue"].Length > 0)
                        {
                            data[lastSection].Add(m.Groups["Key"].Value, m.Groups["Value"].Value);
                        }
                    }
                }
            }
        }

        public NameValueCollection this[string section]
        {
            get
            {
                section = section.ToLowerInvariant();
                if (!data.ContainsKey(section))
				{
                    data.Add(section, new NameValueCollection());
				}
                return data[section];
            }
        }

        public string this[string section, string key]
        {
            get
            {
                return this[section][key];
            }
            set
            {
                this[section][key] = value;
            }
        }

        public object this[string section, string key, Type t]
        {
            get
            {
                if (t == null || t == Type.Missing)
				{
                    return this[section][key];
				}
                return getValue(section, key, null, t);
            }
            set
            {
                if (t == null || t == Type.Missing)
				{
                    this[section][key] = String.Empty;
				}
                else
				{
                    setValue(section, key, value);
				}
            }
        }

        public string[] SectionNames
        {
            get
            {
                string[] all = new string[data.Count];
                data.Keys.CopyTo(all, 0);
                return all;
            }
        }

        public string[] KeyNames(string section)
        {
            return this[section].AllKeys;
        }

        public string[] SectionValues(string section)
        {
			return this[section].GetValues(0);
        }

        public object getValue(string section, string key, object defaultValue, Type t)
        {
            if (!data.ContainsKey(section))
                return defaultValue;
            string v = data[section][key];
            if (string.IsNullOrEmpty(v))
                return defaultValue;
            TypeConverter conv = TypeDescriptor.GetConverter(t);
            if (conv == null)
                return defaultValue;
            if (!conv.CanConvertFrom(typeof(string)))
                return defaultValue;
            try
            {
                return conv.ConvertFrom(v);
            }
            catch
            {
                return defaultValue;
            }
        }

        public T getValue<T>(string section, string key, T defaultValue)
        {
            return (T)getValue(section, key, defaultValue, typeof(T));
        }
        
        public T getValue<T>(string section, string key)
        {
            return getValue<T>(section, key, default(T));
        }

        public Boolean GetBoolean(string section, string key, Boolean defaultValue)
        {
            return getValue<Boolean>(section, key);
        }
       
        public Boolean GetBoolean(string section, string key)
        {
            return GetBoolean(section, key, default(Boolean));
        }
        
        public Byte GetByte(string section, string key, Byte defaultValue)
        {
            return getValue<Byte>(section, key);
        }
        
        public Byte GetByte(string section, string key)
        {
            return GetByte(section, key, default(Byte));
        }
        
        public SByte GetSByte(string section, string key, SByte defaultValue)
        {
            return getValue<SByte>(section, key);
        }
        
        public SByte GetSByte(string section, string key)
        {
            return GetSByte(section, key, default(SByte));
        }
        
        public Int16 GetInt16(string section, string key, Int16 defaultValue)
        {
            return getValue<Int16>(section, key);
        }
        
        public Int16 GetInt16(string section, string key)
        {
            return GetInt16(section, key, default(Int16));
        }
        
        public UInt16 GetUInt16(string section, string key, UInt16 defaultValue)
        {
            return getValue<UInt16>(section, key);
        }
        
        public UInt16 GetUInt16(string section, string key)
        {
            return GetUInt16(section, key, default(UInt16));
        }
        
        public Int32 GetInt32(string section, string key, Int32 defaultValue)
        {
            return getValue<Int32>(section, key);
        }
        
        public Int32 GetInt32(string section, string key)
        {
            return GetInt32(section, key, default(Int32));
        }
        
        public UInt32 GetUInt32(string section, string key, UInt32 defaultValue)
        {
            return getValue<UInt32>(section, key);
        }
        
        public UInt32 GetUInt32(string section, string key)
        {
            return GetUInt32(section, key, default(UInt32));
        }
        
        public Int64 GetInt64(string section, string key, Int64 defaultValue)
        {
            return getValue<Int64>(section, key);
        }
        
        public Int64 GetInt64(string section, string key)
        {
            return GetInt64(section, key, default(Int64));
        }
        
        public UInt64 GetUInt64(string section, string key, UInt64 defaultValue)
        {
            return getValue<UInt64>(section, key);
        }
        
        public UInt64 GetUInt64(string section, string key)
        {
            return GetUInt64(section, key, default(UInt64));
        }
        
        public Single GetSingle(string section, string key, Single defaultValue)
        {
            return getValue<Single>(section, key);
        }
        
        public Single GetSingle(string section, string key)
        {
            return GetSingle(section, key, default(Single));
        }
        
        public Double GetDouble(string section, string key, Double defaultValue)
        {
            return getValue<Double>(section, key);
        }
        
        public Double GetDouble(string section, string key)
        {
            return GetDouble(section, key, default(Double));
        }

        public Decimal GetDecimal(string section, string key, Decimal defaultValue)
        {
            return getValue<Decimal>(section, key);
        }

        public Decimal GetDecimal(string section, string key)
        {
            return GetDecimal(section, key, default(Decimal));
        }

        public DateTime GetDateTime(string section, string key, DateTime defaultValue)
        {
            return getValue<DateTime>(section, key);
        }

        public DateTime GetDateTime(string section, string key)
        {
            return GetDateTime(section, key, default(DateTime));
        }

        public void setValue(string section, string key, object value)
        {
            if (value == null)
            {
                this[section][key] = String.Empty;
            }
            else
            {
                TypeConverter conv = TypeDescriptor.GetConverter(value);
                if (conv == null || !conv.CanConvertTo(typeof(string)))
                {
                    this[section][key] = value.ToString();
                }
                else
                {
                    this[section][key] = (string)conv.ConvertTo(value, typeof(string));
                }
            }
        }

        public void setValue(string section, string key, Boolean value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, Byte value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, SByte value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, Int16 value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, Int32 value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, Int64 value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, UInt16 value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, UInt32 value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, UInt64 value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, Single value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, Double value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, Decimal value)
        {
            setValueToString(section, key, value);
        }

        public void setValue(string section, string key, DateTime value)
        {
            setValueToString(section, key, value);
        }

        public bool HasSection(string section)
        {
            return data.ContainsKey(section.ToLowerInvariant());
        }

        public bool HasKey(string section, string key)
        {
            return
                data.ContainsKey(section) &&
                !string.IsNullOrEmpty(data[section][key]);
        }

        public void Save(string fileName)
        {
            Save(fileName, Encoding.UTF8);
        }

        public void Save(string fileName, Encoding encoding)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Save(fs, encoding);
            }
        }

        public void Save(Stream stream)
        {
            Save(stream, Encoding.UTF8);
        }

        public void Save(Stream stream, Encoding encoding)
        {
            if (stream == null || stream == Stream.Null)
			{
                throw new ArgumentNullException("stream");
			}
            
			if (encoding == null)
			{
                throw new ArgumentNullException("encoding");
			}
			
            using (StreamWriter sw = new StreamWriter(stream, encoding))
            {
                Dictionary<string, NameValueCollection>.Enumerator en = data.GetEnumerator();
                while (en.MoveNext())
                {
                    KeyValuePair<string, NameValueCollection> cur = en.Current;
                    
					if (!string.IsNullOrEmpty(cur.Key))
                    {
                        sw.WriteLine("[{0}]", cur.Key);
                    }
                    
					NameValueCollection col = cur.Value;
                    
					foreach (string key in col.Keys)
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            string value = col[key];
                            if (!string.IsNullOrEmpty(value))
							{
                                sw.WriteLine("{0}={1}", key, value);
							}
                        }
                    }
                }
                
				sw.Flush();
            }
        }

        void setValueToString(string section, string key, object value)
        {
            this[section][key] = value.ToString();
        }

    }
}

