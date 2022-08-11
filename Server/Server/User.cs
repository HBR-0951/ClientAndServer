using System.Net.Sockets;
using System.Timers;
using ProtoBuff.Packet;
using System.Net;


namespace Server {


	public class User {
		private string m_sessionId { get; }
		private readonly Socket m_tcpSocket;
		protected Thread PacketReceivedThread { get; private set; }

		protected static System.Timers.Timer? aTimer;
		public bool isOffLine = false;
		public int user_ID { get; }





        // packet
        protected int packetLength = 0;
        protected bool hasOverPacket = false;
		protected byte[] dataBuffer = new byte[1024];
		protected int IndexOfBuffer = 0;
		protected int dataBufferLength = 0;
		protected int serviceID;
		


		// MQ
		//public delegate void PacketMQ(SamplePacket packet);
		//public event PacketMQ PacketMQEvent;

		public bool IsConnected {
			get => m_tcpSocket.Connected;
		}

		// Login
		protected Thread m_CheckLogin;

		//public delegate void PacketHandler(byte[] bytesPacket);

		//public event PacketHandler PacketEvent;

#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        public User(Socket tcpSocket, string session_Id, int user_Id) {
			this.m_tcpSocket = tcpSocket;
			this.m_sessionId = session_Id;
			this.user_ID = user_Id;

			// Thread
			PacketReceivedThread = new Thread(OnPacketReceived) { IsBackground = false };
			PacketReceivedThread.Start();

			string msg = $"Connect to [ Target IP:{m_tcpSocket.RemoteEndPoint} ] Successful : [ User_id: {this.user_ID} ]";
			serviceID = (int)ProtoBuff.Packet.Type.ServerToClient;
			byte[] bytesPacket = OnBuildPacket(msg, serviceID, this.user_ID);
			Send(bytesPacket);

			// 傳給client自己的userID
			string userid = this.user_ID.ToString();
			serviceID = (int)ProtoBuff.Packet.Type.SendUserID;
			byte[] data = OnBuildPacket(userid, serviceID, this.user_ID);
			Send(data);
			Console.WriteLine("has send user");			
		}
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。



		// 當封包到達時
		public virtual void OnPacketReceived()
		{


			// 服務器 及 客戶端 還處於接受連線狀態時，無限迴圈 -> 等待接受客戶端資料
			while (IsConnected)
			{
#pragma warning disable CS8600 // 正在將 Null 常值或可能的 Null 值轉換為不可為 Null 的型別。
                byte[] packet = OnUnPack();
#pragma warning restore CS8600 // 正在將 Null 常值或可能的 Null 值轉換為不可為 Null 的型別。

                if (packet!= null)
                {
					Server.MQ_Queue.Enqueue(packet);
                    
				}

			}

			OnClosed();



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





		public void Send(byte[] bytesPacket) {
			
			if (!m_tcpSocket.Connected)
            {
				return;
			}

			// 加上外殼
			packetLength = bytesPacket.Length;
			byte[] newbytesPacket = new byte[sizeof(int) + packetLength];

			//Console.WriteLine("packetLength" + packetLength);

			System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packetLength)).CopyTo(newbytesPacket, 0);
			bytesPacket.CopyTo(newbytesPacket, sizeof(int));

			m_tcpSocket.Send(newbytesPacket);
		}

		

		public void OnClosed()
        {
			Console.WriteLine($"Close Client [ User_ID : {m_sessionId} ]");
			
			m_tcpSocket.Close();
        }

		//private void OnPacket(byte[] bytesPacket)
  //      {
		//	if (PacketEvent != null)
		//	{
		//		PacketEvent.Invoke(bytesPacket);
		//	}
		//}


		/// <summary>
		/// Function: 1: 傳送userID給client
		///           2: server轉發,
		///           3: 給server,
		///           4: server 給 client       
		/// </summary>
		public byte[] OnBuildPacket(string msg, int function, int target_id)
		{
			var packet = new MsgPacket();
			packet.ID = 123;
			packet.Code = 0;
			packet.TargetID = target_id;
			if(function == 4)
            {
				packet.SenderID = -1;
			}
            else
            {
				packet.SenderID = this.user_ID;
			}
			
			packet.Function = function;
			packet.Message = msg;
			
			packetLength = packet.SizeOfPacket;
			//Console.WriteLine("sender_id: " + this.user_ID);
			//Console.WriteLine("target_id: " + target_id);
			byte[] bytesPacket = packet.ToPacketup();

			return bytesPacket;
		}


		
	
	}
}

