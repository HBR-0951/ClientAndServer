using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ProtoBuff.Packet;

namespace Client
{
	public partial class Client
	{
		protected Socket? m_tcpSocket;

		protected string m_host = "localhost";
		protected int m_port = 8888;
		protected IPEndPoint? IPEndPoint;

		protected Thread? _awaitServer;
		protected Thread? _sendPacket;

		// Timer
		

		//userid
		public int user_id { get; set; }
		public bool hasGetUserID = false;

		
		// packet
		protected int packetLength = 0;
		protected byte[] dataBuffer = new byte[1024];
		protected int IndexOfBuffer = 0;
		protected int dataBufferLength = 0;
		protected bool hasOverPacket = false;

		// MQ
		public static Queue<byte[]> MQ_Queue = new Queue<byte[]>();
		protected Thread _queueEvent;

		// Login
		protected string Login_UserID = "1";
		protected string Login_Password = "1234";
		protected bool isLogin = false;

		public string Name { get; set; } = "N/A";

		public bool IsConnected
		{
			get => m_tcpSocket != null && m_tcpSocket.Connected;
		}

		public Client(string name)
		{
			Name = name;
			_awaitServer = new Thread(OnPacketReceived);
			_sendPacket = new Thread(OnSendPacket);
			_queueEvent = new Thread(OnQueueEventHandler);

			m_tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			OnInitialize();

		}

		protected void OnInitialize()
        {
			OnInitializeDispatcher();
		}

		protected void OnPacketReceived()
		{


			while (IsConnected)
			{

				var packet = OnUnPack();

				if (packet != null)
				{
					MQ_Queue.Enqueue(packet);
				}
				else
				{
					Thread.Sleep(100);

				}
			}
			Console.WriteLine("unconnected");
			OnClosed();







		}

		// 等待接收要傳送的訊息(Thread)
		protected void OnSendPacket()
		{
			while (IsConnected)
			{

                try
				{
					string? s = Console.ReadLine();

					

                    if (s != null)
                    {
                        Console.WriteLine("send msg");
						int serviceID = (int)ProtoBuff.Packet.Type.Forward;
                        byte[] bytesPacket = OnBuildMsgPacket(s, serviceID, 1);
                        Send(bytesPacket);
                    }

					// 測試用連發訊息
                    //for (int i = 0; i < 100; i++)
                    //               {
                    //	string str = "";
                    //	Random random = new Random();
                    //	for(int j = 0; j < random.Next(10,100); j++)
                    //                   {
                    //		str += "t";
                    //                   }
                    //                   byte[] bytesPacket = OnBuildPacket(str + ": " + (i+1).ToString(), 2, 1);
                    //                   Send(bytesPacket);
                    //                   Thread.Sleep(100);
                    //               }


                }
                catch (Exception ex)
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

#pragma warning disable CS8602 // 可能 null 參考的取值 (dereference)。
            m_tcpSocket.Close();
#pragma warning restore CS8602 // 可能 null 參考的取值 (dereference)。
            Console.WriteLine("Client has closed.");
		}

		public void OnStart()
		{
			try
			{
				m_tcpSocket.Connect(IPEndPoint);
				Console.WriteLine("Connect");
				_awaitServer.Start();
				//_sendPacket.Start();
				_queueEvent.Start();
				
				
			}
			catch (SocketException e)
			{
				Console.WriteLine("Warning :" + e);
			}

		}

		public void Send(byte[] bytesPacket)
		{
			packetLength = bytesPacket.Length;

			byte[] newbytesPacket = new byte[sizeof(int) + packetLength];

			// 外殼 + bytesPAcket = newbytesPacket
			System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packetLength)).CopyTo(newbytesPacket, 0);
			bytesPacket.CopyTo(newbytesPacket, sizeof(int));

			m_tcpSocket.Send(newbytesPacket);
		}

		/// <summary>
		/// Function: 1: 傳送userID給client
		///           2: server轉發,
		///           3: 給server,
		///           4: server 給 client       
		/// </summary>
		public byte[] OnBuildMsgPacket(string msg, int function, int target_id)
		{
			var packet = new MsgPacket();
			packet.ID = 12;
			packet.Code = 123;
			packet.TargetID = target_id;
			packet.SenderID = this.user_id;
			packet.Function = function;
			packet.Message = msg;
			//Console.WriteLine("sender_id: " + packet.SenderID);
			//Console.WriteLine("target_id: " + target_id);
			byte[] bytesPacket = packet.ToPacketup();



			return bytesPacket;
		}
		public byte[] OnBuildLoginPacket(string id, string password, int function)
        {
			var packet = new LoginPacket();
			packet.ID = 1;
			packet.Code = 123;
			packet.Function = function;
			packet.TargetID = -1;
			packet.SenderID = this.user_id;
			packet.LoginID = id;
			packet.LoginPsd = password;

			byte[] bytesPacket = packet.ToPacketup();

			return bytesPacket;

        }



		// new 解封包 
		public byte[]? OnUnPack()
		{
			bool hasPcketLength = false;
			byte[] fullPacket;

			int packetLength = 0;


			while (IsConnected)
			{

				if (m_tcpSocket.Available != 0 || hasOverPacket == true)
				{
					if (hasOverPacket)
					{
						// 宣告一個新的空byte[]來讓dataBuffer覆蓋，以免他超出範圍
						byte[] tempBuffer = new byte[1024];
						Array.Copy(dataBuffer, IndexOfBuffer, tempBuffer, 0, dataBufferLength);
						dataBuffer = tempBuffer;
						IndexOfBuffer = 0;
						hasOverPacket = false;
					}
					else
					{
						// Put all receiveData in dataBuffer
						byte[] temp = new byte[m_tcpSocket.Available];
						m_tcpSocket.Receive(temp);

						int dataLength = temp.Length;
						// 宣告一個新的空byte[]來讓dataBuffer覆蓋，以免他超出範圍
						byte[] tempBuffer = new byte[1024];
						Array.Copy(dataBuffer, IndexOfBuffer, tempBuffer, 0, dataBufferLength);

						dataBuffer = tempBuffer;

						temp.CopyTo(dataBuffer, dataBufferLength);

						dataBufferLength += dataLength;


						IndexOfBuffer = 0;
					}







					// 假設還沒得到packet指定長度
					if (!hasPcketLength)
					{
						// 假設buffer不足4bytes，就繼續等待收到資料
						if (dataBufferLength < 4)
						{
							Thread.Sleep(100);
							continue;
						}
						else
						{
							byte[] tempLength = MsgPacket.Extract(dataBuffer, IndexOfBuffer, 4);
							IndexOfBuffer += 4;
							dataBufferLength -= 4;
							packetLength = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(tempLength, 0));
							hasPcketLength = true;

						}
					}

					if (hasPcketLength)
					{
						// 安全性檢查
						if (packetLength <= 0)
						{
							Console.WriteLine("Has Wrong");
							break;
						}
						// 假設長度符合
						if (dataBufferLength >= packetLength)
						{

							fullPacket = MsgPacket.Extract(dataBuffer, IndexOfBuffer, packetLength);
							IndexOfBuffer += packetLength;
							dataBufferLength -= packetLength;


							if (dataBufferLength > 0)
							{
								hasOverPacket = true;
							}
							return fullPacket;
						}
						else
						{
							Thread.Sleep(100);
							continue;
						}


					}

				}
				else
				{
					Thread.Sleep(100);
				}
			}


			return null;


		}

		protected void OnQueueEventHandler()
		{
			while (IsConnected)
			{
				// 假設queue裡有packet
				if (MQ_Queue.Count != 0)
				{
					byte[] bytesPacket = MQ_Queue.Dequeue();
					MsgPacket packet = new MsgPacket();
					packet.UnPack(bytesPacket);

					

					string service = ((ProtoBuff.Packet.Type)(packet.Function)).ToString();

					m_cmdDispatcher.Dispatch(service, bytesPacket);
				}
				Thread.Sleep(100);
			}
		}




	}
}
