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
		protected DateTime local1;
		protected DateTime local2;
		protected TimeSpan interval;
		protected int limit_Interival = 5;

		//userid
		public int user_id { get; set; }
		public bool hasGetUserID = false;

		// packet
		protected byte[] receiveLength = new byte[sizeof(int)];
		protected byte[] receiveData;
		protected SamplePacket receivePacket = new SamplePacket();
		protected byte[] overData;
		protected int packetLength = 0;
		protected int receivePacketLength = 0;
		protected int countLength = 0;
		protected bool isFullPacket = false;
		protected bool isFirst = true;
		protected int IndexOfReveiveData = 0;
		protected int IndexOfLastLength = 0;
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


			while (m_tcpSocket.Connected)
			{

				if (m_tcpSocket.Available != 0)
				{
					// 讀取資料
					byte[] data = new byte[m_tcpSocket.Available];
					m_tcpSocket.Receive(data);

					//解封包
					OnUnPack(data);



				}
				else
				{
					if (hasGetUserID)
					{
						local2 = DateTime.Now;
						interval = local2 - local1;
						if (interval.Seconds >= limit_Interival)
						{
							byte[] bytesPacket = OnBuildPacket("$", 0, -1);
							Send(bytesPacket);
							local1 = DateTime.Now;
						}

					}
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

					local1 = DateTime.Now;

                    if (s != null)
                    {
                        Console.WriteLine("send msg");
                        byte[] bytesPacket = OnBuildPacket(s, 2, 1);
                        Send(bytesPacket);
                    }
     //               for (int i = 0; i < 100; i++)
					//{
					//	byte[] bytesPacket = OnBuildPacket((i + 1).ToString(), 2, 1);
					//	Send(bytesPacket);
					//	Thread.Sleep(100);
					//}


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
				local1 = DateTime.Now;
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

		// 解封包
		public void OnUnPack(byte[] data)
		{
			// 計算得到的資料長度有沒有符合 4bytes，沒有的話就繼續等待下次接收資料
			countLength += data.Length;


			// 假設收到的資料長度 < 4
			if (countLength < 4)
			{
				// 先把值取出來，然後再把他複製到reveiveLength上，然後記住他存到哪個index
				byte[] tempLength = SamplePacket.Extract(data, 0, data.Length);
				Array.Copy(tempLength, 0, receiveLength, IndexOfLastLength, data.Length);

				IndexOfLastLength += data.Length;


			}
			// 假設取到的資料長度 >= 4
			else
			{
				// 假設是第一次（可以取長度的時候）
				if (isFirst)
				{
					// 把封包長度的值複製過去並負值給reveivePAcketLength
					byte[] tempLength = SamplePacket.Extract(data, 0, (4 - IndexOfLastLength));
					Array.Copy(tempLength, 0, receiveLength, IndexOfLastLength, (4 - IndexOfLastLength));
					receivePacketLength = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(receiveLength, 0));


					// 給receiveData存取的資料大小空間
					receiveData = new byte[receivePacketLength];
					int IndexOfstartData = (4 - IndexOfLastLength);
					// 假設得到的資料等於封包長度
					if (countLength - 4 == receivePacketLength)
					{
						// 把全部的data給receiveData
						receiveData = SamplePacket.Extract(data, IndexOfstartData, countLength - 4);
						receivePacket.UnPack(receiveData);
						isFullPacket = true;
					}
					// 假設得到的資料比給的封包長度還大
					else if (countLength - 4 > receivePacketLength)
					{
						hasOverPacket = true;

						// 符合封包長度大小的data部分把值給receiveData
						receiveData = SamplePacket.Extract(data, IndexOfstartData, receivePacketLength);
						receivePacket.UnPack(receiveData);
						isFullPacket = true;

						// 把剩下的封包放進一個暫存的byte[] overData
						int IndexOfOverData = IndexOfstartData + receivePacketLength;
						int length = data.Length - IndexOfOverData;
						overData = SamplePacket.Extract(data, IndexOfOverData, length);

					} // 假設得到的資料長度 < 封包長度 
					else
					{
						//剩餘data的長度
						int length = data.Length - IndexOfstartData;
						// 剩下的data部分把值給receiveData
						receiveData = SamplePacket.Extract(data, IndexOfstartData, length);

						// 記錄資料存到receiveData的位置，因為是第一次，所以存的index等於length-1
						IndexOfReveiveData = length - 1;

					}
					isFirst = false;
				}
				// 假設不是第一次，reveiveData 要用字元相加
				else
				{
					// 假設得到的資料(扣掉資料長度) == 封包長度
					if (countLength - 4 == receivePacketLength)
					{
						// 把所有資料加進receiveData
						byte[] tempData = SamplePacket.Extract(data, 0, data.Length);
						Array.Copy(tempData, 0, receiveData, IndexOfReveiveData, data.Length);

						receivePacket.UnPack(receiveData);
						isFullPacket = true;
					}
					// 假設得到的資料(扣掉資料長度) > 封包長度
					else if (countLength - 4 > receivePacketLength)
					{
						hasOverPacket = true;

						// 符合封包長度大小的data部分 加上 receiveData
						int length = receivePacketLength - IndexOfReveiveData;
						byte[] tempData = SamplePacket.Extract(data, 0, length);
						Array.Copy(tempData, 0, receiveData, IndexOfReveiveData, length);
						receivePacket.UnPack(receiveData);
						isFullPacket = true;

						// 把剩下的封包放進一個暫存的byte[] overData
						int IndexOfOverData = length;
						length = data.Length - IndexOfOverData;
						overData = SamplePacket.Extract(data, IndexOfOverData, length);

					} // 假設得到的資料長度 < 封包長度 
					else
					{
						// 剩下的資料長度
						int length = data.Length;
						// 剩下的data部分把值給receiveData
						receiveData = SamplePacket.Extract(data, IndexOfReveiveData, length);

						// 記錄資料存到的位置
						IndexOfReveiveData += length;

					}
				}


			}

			// 假設是完整的封包
			if (isFullPacket)
			{
				string msg = receivePacket.Message;
				int function = receivePacket.Function;

				switch (function)
				{
					case 1: // 得到user_id
						this.user_id = Int32.Parse(msg);
						Console.WriteLine("Client get userid: " + this.user_id);
						hasGetUserID = true;
						break;
					case 2: // server 轉發
						var target_id = receivePacket.TargetID;
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
				isFullPacket = false;
				isFirst = true;
				countLength = 0;
			}
			// 假設有overPacket，遞迴呼叫 OnUnPack() 解剩下的封包
			if (hasOverPacket)
			{
				hasOverPacket = false;
				OnUnPack(overData);
			}
		}


	}
}
