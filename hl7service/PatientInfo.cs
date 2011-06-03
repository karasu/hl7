using System;
namespace hl7service
{
	public class PatientInfo
	{
		public PatientInfo ()
		{
		}
		
		public string toSQL()
		{
			
		}
		
		public string toXML()
		{
			
		}
		
		public string toHL7(int version)
		{
			
		}

		public string toHL7v2()
		{
			
		}

		public string toHL7v3()
		{
			
		}
		
		public void fromHL7(string text, int version)
		{
			if (version == 2)
			{
				
			}
			
		}
		
		public void fromHL7v2(string text)
		{}
		
		public void fromHL7v3(string xml)
		{}
	}
}

