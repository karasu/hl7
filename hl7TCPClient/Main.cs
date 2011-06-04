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
		
		public static void Main (string[] args)
		{
			string message = String.Empty;
			
			message += START_OF_BLOCK;
			message += "MSH|^~\\&|DDTEK LAB|ELAB-1|DDTEK OE|BLDG14|200502150930||ORU^R01^ORU_R01|CTRL-9876|P|2.4";
			message += CARRIAGE_RETURN;
			message += "PID|||010-11-1111||Estherhaus^Eva^E^^^^L|Smith|19720520|F|||256 Sherwood Forest Dr.^^Baton Rouge^LA^70809||(225)334-5232|(225)752-1213||||AC010111111||76-B4335^LA^20070520";
			message += CARRIAGE_RETURN;
			message += "OBR|1|948642^DDTEK OE|917363^DDTEK LAB|1554-5^GLUCOSE|||200502150730|||||||||020-22-2222^Levin-Epstein^Anna^^^^MD^^Micro-Managed Health Associates|||||||||F|||||||030-33-3333&Honeywell&Carson&&&&MD";
			message += CARRIAGE_RETURN;
			message += "OBX|1|SN|1554-5^GLUCOSE^^^POST 12H CFST:MCNC:PT:SER/PLAS:QN||^175|mg/dl|70_105|H|||F";
			message += CARRIAGE_RETURN;
			message += END_OF_BLOCK;
			message += CARRIAGE_RETURN;
			
			try 
			{
				// Create a TcpClient.
				// Note, for this client to work you need to have a TcpServer 
				// connected to the same address as specified by the server, port
				// combination.
				TcpClient client = new TcpClient("127.0.0.1", 8901);
				
				// Translate the passed message into ASCII and store it as a Byte array.
				Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);         
				
				// Get a client stream for reading and writing.
				//  Stream stream = client.GetStream();
				
				NetworkStream stream = client.GetStream();
				
				// Send the message to the connected TcpServer. 
				stream.Write(data, 0, data.Length);
				
				Console.WriteLine("TCPClient sent: {0}", message);         
				
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
			} 
			catch (ArgumentNullException e) 
			{
				Console.WriteLine("TCPClient: ArgumentNullException: {0}", e);
			} 
			catch (SocketException e) 
			{
				Console.WriteLine("TCPClient: SocketException: {0}", e);
			}
			
			Console.WriteLine("\n TCPClient: Press Enter to continue...");
			Console.Read();
		}
	}
}

