using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

namespace hl7service
{
	public class TCPHandler
	{
		private static char END_OF_BLOCK = (char)0x001c;
		private static char START_OF_BLOCK = (char)0x000b;
		private static char CARRIAGE_RETURN = (char)13;
		
    	private Thread listenThread;

		private TcpListener tcpListener;
		
		private string folder;

        public TCPHandler(int port, string folder)
        {
			this.folder = folder;
			
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            
			this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }
        
        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                // blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                // create a thread to handle communication 
                // with connected client
				
				Console.WriteLine("TCPHandler: Create a thread to handle communication with connected client");
				
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }
		
		private void HandleClientComm(object client)
		{
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream clientStream = tcpClient.GetStream();
			
			byte[] message = new byte[8192];
			int bytesRead;
						                 
			while (true)
			{
				bytesRead = 0;
				
				try
				{
					Console.WriteLine("TCPHandler: block until a client sends a message");
					
					bytesRead = clientStream.Read(message, 0, 4096);
				}
				catch
				{
					Console.WriteLine("TCPHandler: a socket error has occured");
					break;
				}
				
				if (bytesRead == 0)
				{
					Console.WriteLine("TCPHandler: the client has disconnected from the server");
					break;
				}
				
				// message has successfully been received
				ASCIIEncoding encoder = new ASCIIEncoding();
				Console.WriteLine(encoder.GetString(message, 0, bytesRead));
			
				string fileName = this.folder + "/" + System.Guid.NewGuid().ToString();

				if (message[0] != START_OF_BLOCK)
				{
					Console.WriteLine("TCPHandler: Not HL7 v2 version, assuming v3");
					
					fileName += ".v3.hl7";
				}
				else
				{
					Console.WriteLine("TCPHandler: HL7 v2 message received.");
					fileName += ".v2.hl7";
				}
								
				// Write message to disk
				StreamWriter outfile = new StreamWriter(fileName);
				outfile.Write(message);
				outfile.Close();
			}
			
			tcpClient.Close();
		}				
	}
}

