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
		//protected Thread? _packageReceived;
		protected Thread? _sendPacket;

		

		public string Name { get; set; } = "N/A";

		public bool ISConnected
        {
			get => m_tcpSocket != null && m_tcpSocket.Connected;
        }

		public Client(string name)
        {
			Name = name;
			_awaitServer = new Thread(OnPacketReceived);
			//_packageReceived = new Thread(OnPacketReceived);
			
			m_tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }


		protected void OnNewConnection()
        {
			//while (m_tcpSocket.Connected)
			//{
			//	OnPacketReceived();
			//	Console.WriteLine("1");
   //             string s = Console.ReadLine();
   //             Send(s);
   //         }
		}

		protected void OnPacketReceived()
        {
			while (true)
			{
				try
				{
					
					int set = 0;
					while (m_tcpSocket.Connected)
					{

						if (m_tcpSocket.Available != 0)
						{

							// 讀取資料
							// Socket.Receive();
							byte[] date = new byte[m_tcpSocket.Available];
							int count = m_tcpSocket.Receive(date);
							string msg = Encoding.UTF8.GetString(date, 0, count);
							Console.WriteLine("Client get msg: " + msg);
							set = 1;


						}
						else
						{
							Thread.Sleep(100);
						}
                        if (set == 1)
                        {
							string s = Console.ReadLine();
							Send(s);
							set = 0;
                        }

						
						


					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}



		}
		
		public void BuildEndPoint(string ipAddress, int port)
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

