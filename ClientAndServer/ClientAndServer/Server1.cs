using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ClientAndServer
{
	public class Server1
	{
		public Socket? serverSocket;
		public Socket? clientSocket;
		public string ipAddress;
		public int port;
		public User? user;

		public Server1(string ipAddress, int port)
		{
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			user = new User(serverSocket, "1");
			this.ipAddress = ipAddress;
			this.port = port;
		}
		public void Start() {
			IPAddress ipaddress = IPAddress.Parse(ipAddress);
			IPEndPoint ipendpoint = new IPEndPoint(ipaddress, port);

			serverSocket.Bind(ipendpoint);
			serverSocket.Listen(5);
		}
		public void Close() { }
		public void SendTo(int target) { }
		public void Broadcast() { }
	}
}

