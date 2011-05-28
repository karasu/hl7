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
            
			this.listenThread = new Thread(new ThreadStart(Listener));
            this.listenThread.Start();
        }
        
        private void Listener()
        {
            this.tcpListener.Start();

            while (true)
            {
                // blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                // create a thread to handle communication 
                // with connected client
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
		      //blocks until a client sends a message
		      bytesRead = clientStream.Read(message, 0, 4096);
		    }
		    catch
		    {
		      //a socket error has occured
		      break;
		    }
		
		    if (bytesRead == 0)
		    {
		      //the client has disconnected from the server
		      break;
		    }
		
		    //message has successfully been received
		    ASCIIEncoding encoder = new ASCIIEncoding();
		    System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
		  }
		
		  tcpClient.Close();
		}				
	}
}

