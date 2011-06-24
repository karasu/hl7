using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace hl7TCPClient
{
	class MainClass
	{
		private static char END_OF_BLOCK = (char)0x001c;
		private static char START_OF_BLOCK = (char)0x000b;
		private static char CARRIAGE_RETURN = (char)13;
		
		public static void showHelp()
		{
			Console.WriteLine("hl7TCPClient - A TCP/IP Health Level 7 Test Client");
			Console.WriteLine("Commands:");
			Console.WriteLine("--help : shows this help screen");
			Console.WriteLine("--daemon : sends messages non stop");
			Console.WriteLine("--nowait : does not wait between messages (by default waits one second)");
			Console.WriteLine("\n Coded by Karasu");
		}
		
		public static void Main (string[] args)
		{
			string [] messages = new string[2];
			int iMessage = 0;
			int miliseconds = 1000;
			bool daemon = false;
			
			foreach (string param in args)
			{
				if (param == "--help")
				{
					showHelp();
					return;
				}
				
				if (param == "--daemon")
				{
					daemon = true;
				}
				
				if (param == "--nowait")
				{
					miliseconds = 0;
				}
			}
			
			messages[0] = String.Empty;
			messages[0] += START_OF_BLOCK;
			messages[0] += "MSH|^~\\&|DDTEK LAB|ELAB-1|DDTEK OE|BLDG14|200502150930||ORU^R01^ORU_R01|CTRL-9876|P|2.4";
			messages[0] += CARRIAGE_RETURN;
			messages[0] += "PID|||010-11-1111||Estherhaus^Eva^E^^^^L|Smith|19720520|F|||256 Sherwood Forest Dr.^^Baton Rouge^LA^70809||(225)334-5232|(225)752-1213||||AC010111111||76-B4335^LA^20070520";
			messages[0] += CARRIAGE_RETURN;
			messages[0] += "OBR|1|948642^DDTEK OE|917363^DDTEK LAB|1554-5^GLUCOSE|||200502150730|||||||||020-22-2222^Levin-Epstein^Anna^^^^MD^^Micro-Managed Health Associates|||||||||F|||||||030-33-3333&Honeywell&Carson&&&&MD";
			messages[0] += CARRIAGE_RETURN;
			messages[0] += "OBX|1|SN|1554-5^GLUCOSE^^^POST 12H CFST:MCNC:PT:SER/PLAS:QN||^175|mg/dl|70_105|H|||F";
			messages[0] += CARRIAGE_RETURN;
			messages[0] += END_OF_BLOCK;
			messages[0] += CARRIAGE_RETURN;
			
			messages[1] = String.Empty;
			messages[1] += START_OF_BLOCK;
			messages[1] += "MSH|^~\\&|OAZIS||||20100826092310||ADT^A04|09585435|P|2.3||||||ASCII";
			messages[1] += CARRIAGE_RETURN;
			messages[1] += "EVN|A04|20100826092310||||201008260921";
			messages[1] += CARRIAGE_RETURN;
			messages[1] += "PID|1||4005181503||Slock^Willy Eduard^^^Dhr.||19400518|M|||Herritakkerlaan 24^^SLEIDINGE^^9940^B^H||09/3574031^^PH||NL|S||26438854^^^^VN|526852365|40051818751||||||B||||N";
			messages[1] += CARRIAGE_RETURN;
			messages[1] += "PD1||||135753^Keereman^Natascha||||||||N";
			messages[1] += CARRIAGE_RETURN;
			messages[1] += "PV1|1|O|AMB1^000^286^001^0^2|NULL|||135753^Keereman^Natascha||000075^Merckx^Luc|1950|||||||000075^Merckx^Luc|0|26438854^^^^VN|1^20100826|04||||||||||||||||||O|||||201008260921";
			messages[1] += CARRIAGE_RETURN;
			messages[1] += "PV2||003^^^04|NULL||||||201008262121|0|||||||||||0|N||||||||T||||||||0///09/";
			messages[1] += CARRIAGE_RETURN;
			messages[1] += "IN1|1|1|407000|Liberale Mutualiteit van Oost-Vlaanderen|Brabantddam^109^GENT^^9000^B|||||||20100101|||||0||||||||||||||||||||||||||||||130/130||40051818751			";
			messages[1] += END_OF_BLOCK;
			messages[1] += CARRIAGE_RETURN;
			
			do
			{
				try 
				{
					// Create a TcpClient.
					// Note, for this client to work you need to have a TcpServer 
					// connected to the same address as specified by the server, port
					// combination.
					TcpClient client = new TcpClient("127.0.0.1", 8901);
					
					// Translate the passed message into ASCII and store it as a Byte array.
					Byte[] data= System.Text.Encoding.ASCII.GetBytes(messages[iMessage]);         

					// Get a client stream for reading and writing.
					//  Stream stream = client.GetStream();
					
					NetworkStream stream = client.GetStream();
					
					// Send the message to the connected TcpServer. 
					stream.Write(data, 0, data.Length);
					
					Console.WriteLine("TCPClient sent: {0}", messages[iMessage]);
					
					// Receive the TcpServer.response.
					
					// Buffer to store the response bytes.
					data = new Byte[256];
					
					// String to store the response ASCII representation.
					String responseData = String.Empty;
					
					Console.WriteLine("TCPClient: Waiting for response...");
					
					// Read the first batch of the TcpServer response bytes.
					Int32 bytes = stream.Read(data, 0, data.Length);
					responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
					Console.WriteLine("TCPClient received: {0}", responseData);         
					
					// Close everything.
					stream.Close();         
					client.Close();         
					
					iMessage++;
					
					if (iMessage > messages.Length) iMessage = 0;
					
				} 
				catch (ArgumentNullException e) 
				{
					Console.WriteLine("TCPClient: ArgumentNullException: {0}", e);
				} 
				catch (SocketException e) 
				{
					Console.WriteLine("TCPClient: SocketException: {0}", e);
				}
				
				System.Threading.Thread.Sleep(miliseconds);
			}
			while (daemon);
		}
	}
}

