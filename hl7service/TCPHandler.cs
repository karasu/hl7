using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace hl7service
{
	public class TCPHandler
	{
    private Thread listenThread;

		private TcpListener tcpListener;

        public TCPHandler(int port)
        {
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
		
		  byte[] message = new byte[4096];
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
		
		    //message has successfully been received
		    ASCIIEncoding encoder = new ASCIIEncoding();
		    Console.WriteLine(encoder.GetString(message, 0, bytesRead));
		  }
		
		  tcpClient.Close();
		}				
	}
}

