using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ProtoBuff.Packet;

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

		//userid
		public int user_id { get; set; }

	
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
					m_tcpSocket.Receive(date);

					var receivePacket = new SamplePacket();
					receivePacket.UnPack(date);
					string msg = receivePacket.Message;
					int function = receivePacket.Function;

					switch (function)
					{
						case 1: // 得到user_id
							this.user_id = Int32.Parse(msg);
							Console.WriteLine("Client get userid: " + this.user_id);
							break;
						case 2: // server 轉發
							var target_id = receivePacket.TargetID;
							if (target_id == this.user_id)
							{
								Console.WriteLine("Client get msg: " + msg);
							}
							else
							{
								Console.WriteLine("The msg is sent to wrong place");
							}
							break;
						case 4: // server 傳給 client
							Console.WriteLine("Server send msg: " + msg);
							break;
						default:
							break;
					}
				}
				else
				{
					local2 = DateTime.Now;
					interval = local2 - local1;
					if (interval.Seconds > 5)
					{
						byte[] bytesPacket = OnBuildPacket("$", 0, -1);
						Send(bytesPacket);
						local1 = DateTime.Now;
					}
					Thread.Sleep(100);
				}
			}
			Console.WriteLine("unconnected");
			OnClosed();

			
				
			



		}

		protected void OnSendPacket()
        {
            while (m_tcpSocket.Connected)
            {
                try
                {
					string? s = Console.ReadLine();
					local1 = DateTime.Now;

                    if (s != null)
                    {
						byte[] bytesPacket = OnBuildPacket(s, 2, 1);
						Send(bytesPacket);
					}
				}
				catch(Exception ex)
                {
					Console.WriteLine(ex.Message);
                }
				
            }
        }
		
		public void OnBuildEndPoint(string ipAddress, int port)
        {
			m_host = ipAddress;
			m_port = port;

			IPEndPoint = new IPEndPoint(IPAddress.Parse(m_host), m_port);
            
			
		}

		public void OnClosed()
        {
			_awaitServer.Interrupt(); // Thread.Abort()過時
			_sendPacket.Interrupt();
			m_tcpSocket.Close();
			Console.WriteLine("Client has closed.");
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

		public void Send(byte[] bytesPacket)
        {
			m_tcpSocket.Send(bytesPacket);
			Console.WriteLine("Client send msg");
		}

		/// <summary>
		/// Function: 0: 傳送空封包,
		///			  1: 傳送userID給client
		///           2: server轉發,
		///           3: 給server,
		///           4: server 給 client       
		/// </summary>
		public byte[] OnBuildPacket(string msg, int function, int target_id)
		{
			var packet = new SamplePacket();
			packet.ID = 123;
			packet.Code = 0;
			packet.TargetID = target_id;
			packet.SenderID = this.user_id;
			packet.Function = function;
			packet.Message = msg;
			//Console.WriteLine("sender_id: " + packet.SenderID);
			//Console.WriteLine("target_id: " + target_id);
			byte[] bytesPacket = packet.ToPacketup();

			return bytesPacket;
		}


	}
}

