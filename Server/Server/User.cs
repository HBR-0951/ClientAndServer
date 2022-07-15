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

		

		// Timer: local1 - local2
		protected DateTime local1;
		protected DateTime local2;
		protected TimeSpan interval;

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
		



		public bool IsConnected {
			get => m_tcpSocket.Connected;
		}

		public delegate void PacketHandler(byte[] bytesPacket);

		public event PacketHandler PacketEvent;

#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        public User(Socket tcpSocket, string session_Id, int user_Id) {
			this.m_tcpSocket = tcpSocket;
			this.m_sessionId = session_Id;
			this.user_ID = user_Id;

			// Thread
			PacketReceivedThread = new Thread(OnPacketReceived) { IsBackground = false };
			PacketReceivedThread.Start();
			local1 = DateTime.Now;

			string msg = $"[ Time: {local1} ]  Connect to [ Target IP:{m_tcpSocket.RemoteEndPoint} ] Successful : [ User_id: {this.user_ID} ]";
			byte[] bytesPacket = OnBuildPacket(msg, 4, this.user_ID);
			Send(bytesPacket);
			//Thread.Sleep(100);

			// 傳給client自己的userID
			string userid = this.user_ID.ToString();

			byte[] data = OnBuildPacket(userid, 1, this.user_ID);
			Send(data);
			Console.WriteLine("has send user");
		}
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。



        // 當封包到達時
        public virtual void OnPacketReceived() {

            // 服務器 及 客戶端 還處於接受連線狀態時，無限迴圈 -> 等待接受客戶端資料
            while (m_tcpSocket.Connected && !isOffLine)
            {
				// 連線通道可讀資料 不為 0 時，則讀取資料:否則略過
				if (m_tcpSocket.Available != 0)
				{
					
					local1 = DateTime.Now;

					// 讀取資料
					byte[] data = new byte[m_tcpSocket.Available];
					m_tcpSocket.Receive(data);

					//解封包
					OnUnPack(data);


				}
                else
                {
					local2 = DateTime.Now;
					interval = local2 - local1;
					if(interval.Seconds > 10)
                    {
						isOffLine = true;
						local1 = DateTime.Now;
					}
					Thread.Sleep(100);
                }
			}

			OnClosed();



        }

        public void Send(byte[] bytesPacket) {
			
			if (!m_tcpSocket.Connected)
            {
				return;
			}
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

		private void OnPacket(byte[] bytesPacket)
        {
			if (PacketEvent != null)
			{
				PacketEvent.Invoke(bytesPacket);
			}
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
					//Console.WriteLine("receivePacketLength: " + receivePacketLength);

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
				//Console.WriteLine("1");
				var targetid = receivePacket.TargetID;
				var sendid = receivePacket.SenderID;
				string msg = receivePacket.Message;
				int function = receivePacket.Function;
				packetLength = receivePacket.SizeOfPacket;

                //Console.WriteLine(receivePacket.ToString());
        //        Console.WriteLine("\ntargetid: " + targetid
        //                        + "\nsenderid: " + sendid
        //                        + "\nfunction: " + function
								//+ "\npacketLength: " + packetLength
								//+ "\nmsg: " + msg);
                switch (function)
				{
					case 0: // client給空封包
						break;
					case 2: // server 轉發
						this.OnPacket(receiveData); // 執行訂閱的event
						break;
					case 3: // client傳給server
						var senderid = receivePacket.SenderID;
						Console.WriteLine("Server get msg from user[ " + senderid + " ]: " + msg);
						break;
					case 5: // 群發
						this.OnPacket(receiveData);
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

