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


			while (ISConnected)
			{

				var packet = OnUnPack();

				if (packet != null)
				{
					PacketEvent(packet);
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
			while (m_tcpSocket.Connected)
			{
				try
				{
					string? s = Console.ReadLine();

					

                    if (s != null)
                    {
                        Console.WriteLine("send msg");
                        byte[] bytesPacket = OnBuildPacket(s, 5, 1);
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
		public byte[] OnBuildPacket(string msg, int function, int target_id)
		{
			var packet = new SamplePacket();
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

		// new 解封包 
		public SamplePacket OnUnPack()
		{
			bool hasPcketLength = false;
			byte[] fullPacket;
			SamplePacket aFullPacket = new SamplePacket();
			int packetLength = 0;


			while (ISConnected)
			{

				if (m_tcpSocket.Available != 0 || hasOverPacket == true)
				{
					Console.WriteLine("aa");
                    if (hasOverPacket)
                    {
						// 宣告一個新的空byte[]來讓dataBuffer覆蓋，以免他超出範圍
						byte[] tempBuffer = new byte[1024];
						Array.Copy(dataBuffer, IndexOfBuffer, tempBuffer, 0, dataBufferLength);
						dataBuffer = tempBuffer;
						IndexOfBuffer = 0;
						Console.WriteLine("BufferLength: " + dataBufferLength);
						hasOverPacket = false;
                    }
                    else
                    {
						// Put all receiveData in dataBuffer
						byte[] temp = new byte[m_tcpSocket.Available];
						m_tcpSocket.Receive(temp);

						int dataLength = temp.Length;
						Console.WriteLine("dataLength: " + dataLength);
						// 宣告一個新的空byte[]來讓dataBuffer覆蓋，以免他超出範圍
						byte[] tempBuffer = new byte[1024];
						Array.Copy(dataBuffer, IndexOfBuffer, tempBuffer, 0, dataBufferLength);

						dataBuffer = tempBuffer;

						temp.CopyTo(dataBuffer, dataBufferLength);

						dataBufferLength += dataLength;

						Console.WriteLine("BufferLength: " + dataBufferLength);

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
							byte[] tempLength = SamplePacket.Extract(dataBuffer, IndexOfBuffer, 4);
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
							
							fullPacket = SamplePacket.Extract(dataBuffer, IndexOfBuffer, packetLength);
							IndexOfBuffer += packetLength;
							dataBufferLength -= packetLength;
							aFullPacket.UnPack(fullPacket);

							if(dataBufferLength > 0)
                            {
								hasOverPacket = true;
                            }
							return aFullPacket;
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

		protected void PacketEvent(SamplePacket packet)
        {
			var target_id = packet.TargetID;
			int sender_id = packet.SenderID;
			string msg = packet.Message;
			int function = packet.Function;
			var packetLength = packet.SizeOfPacket;

			//Console.WriteLine(receivePacket.ToString());
			//        Console.WriteLine("\ntargetid: " + targetid
			//                        + "\nsenderid: " + sendid
			//                        + "\nfunction: " + function
			//+ "\npacketLength: " + packetLength
			//+ "\nmsg: " + msg);
			switch (function)
			{
				case 1: // 得到user_id
					this.user_id = Int32.Parse(msg);
					Console.WriteLine("Client get userid: " + this.user_id);
					hasGetUserID = true;
					break;
				case 2: // server 轉發
					Console.WriteLine("target_id: " + target_id);
					Console.WriteLine("user_id: " + this.user_id);
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


	}
}
