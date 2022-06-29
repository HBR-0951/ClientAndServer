using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
	public class Client
	{
		protected Socket? m_tcpSocket;

		protected string m_host = "localhost";
		protected int m_port = 8888;
		protected IPEndPoint? IPEndPoint;

		protected Thread? _awaitServer;	
		protected Thread? _sendPacket;

		public bool isOffLine = false;

		// Timer
		protected DateTime local1;
		protected DateTime local2;
		protected TimeSpan interval;

		

		

		public string Name { get; set; } = "N/A";

		public bool ISConnected
        {
			get => m_tcpSocket != null && m_tcpSocket.Connected;
        }

		public Client(string name)
        {
			Name = name;
			_awaitServer = new Thread(OnPacketReceived);
			_sendPacket = new Thread(OnSendPacket);
			
			m_tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

		protected void OnPacketReceived()
        {
			
					
			while (m_tcpSocket.Connected)
			{

				if (m_tcpSocket.Available != 0)
				{
					local1 = DateTime.Now;
					// 讀取資料
					// Socket.Receive();
					byte[] date = new byte[m_tcpSocket.Available];
					int count = m_tcpSocket.Receive(date);
					string msg = Encoding.UTF8.GetString(date, 0, count);
					Console.WriteLine("Client get msg: " + msg);
							


				}
				else
				{
					local2 = DateTime.Now;
					interval = local2 - local1;
					if(interval.Seconds > 5)
                    {
						string s = "$";
						Send(s);
						local1 = DateTime.Now;
                    }
					Thread.Sleep(100);
				}
			}
				
			



		}

		protected void OnSendPacket()
        {
            while (m_tcpSocket.Connected)
            {
				string s = Console.ReadLine();
				local1 = DateTime.Now;
				Send(s);
            }
        }
		
		public void OnBuildEndPoint(string ipAddress, int port)
        {
			m_host = ipAddress;
			m_port = port;

			IPEndPoint = new IPEndPoint(IPAddress.Parse(m_host), m_port);
            
			
		}
		public void OnStart()
        {
			try
			{
				m_tcpSocket.Connect(IPEndPoint);
				Console.WriteLine("Connect");
				_awaitServer.Start();
				_sendPacket.Start();
				local1 = DateTime.Now;
			}
			catch (SocketException e)
			{
				Console.WriteLine("Warning :" + e);
			}
			
        }

		public void Send(string msg)
        {
			
			m_tcpSocket.Send(Encoding.UTF8.GetBytes(msg));
			Console.WriteLine("Client send msg: " + msg);
		}


	}
}

