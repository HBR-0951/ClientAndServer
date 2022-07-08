using System.Net.Sockets;
using System.Timers;
using ProtoBuff.Packet;

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

        public bool IsConnected {
			get => m_tcpSocket.Connected;
		}

		public delegate void PacketHandler(byte[] bytesPacket);

		public event PacketHandler PacketEvent;

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

			// 傳給client自己的userID
			string userid = this.user_ID.ToString();

			bytesPacket = OnBuildPacket(userid, 1, this.user_ID);
			Send(bytesPacket);
		}

        

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
					// Socket.Receive();
					byte[] data = new byte[m_tcpSocket.Available];
					m_tcpSocket.Receive(data);
					var receivePacket = new SamplePacket();
                    receivePacket.UnPack(data);

					var targetid = receivePacket.TargetID;
					var sendid = receivePacket.SenderID;
                    string msg = receivePacket.Message;
					int function = receivePacket.Function;

					//Console.WriteLine(receivePacket.ToString());
					//Console.WriteLine("\ntargetid: " + targetid
					//				+ "\nsenderid: " + sendid
					//				+ "\nfunction: " + function
					//				+ "\nmsg: " + msg);
					switch (function)
                    {
						case 0: // client給空封包
							break;
						case 2: // server 轉發
							this.OnPacket(data); // 執行訂閱的event
							break;
						case 3: // client傳給server
							var senderid = receivePacket.SenderID;
							Console.WriteLine("Server get msg from user[ " + senderid +" ]: " + msg);
							break;
						default:
							break;
					}
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
			m_tcpSocket.Send(bytesPacket);
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

			//Console.WriteLine("sender_id: " + this.user_ID);
			//Console.WriteLine("target_id: " + target_id);
			byte[] bytesPacket = packet.ToPacketup();

			return bytesPacket;
		}
	}
}

