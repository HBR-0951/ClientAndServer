using System.Net.Sockets;
using System.Timers;

namespace Server {


    public class User {
		private readonly string m_sessionId;
		private readonly Socket m_tcpSocket;
		protected Thread PacketReceivedThread { get; private set; }

		protected static System.Timers.Timer? aTimer;
		public bool isOffLine = false;

		// Timer: local1 - local2
		protected DateTime local1;
		protected DateTime local2;
		protected TimeSpan interval;

		public bool hasMsg = false;
		public string receiveMsg;
        

		public bool IsConnected {
			get => m_tcpSocket.Connected;
		}

		public User(Socket tcpSocket, string session_Id) {
			this.m_tcpSocket = tcpSocket;
			this.m_sessionId = session_Id;

			// Thread
			PacketReceivedThread = new Thread(OnPacketReceived) { IsBackground = false };
			PacketReceivedThread.Start();
			local1 = DateTime.Now;
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
					byte[] date = new byte[m_tcpSocket.Available];
					int count = m_tcpSocket.Receive(date);
					string msg = System.Text.Encoding.UTF8.GetString(date, 0, count);
                    if (msg == "$")
                    {
						continue;
                    }    
					else{
						Console.WriteLine($"Server Data Received from [ User_ID : {m_sessionId} ]: " + msg);
						receiveMsg = msg;
						hasMsg = true;
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

        public void Send(string msg) {
			if (!m_tcpSocket.Connected) return;

			byte[] bytesPacket;
			bytesPacket = System.Text.Encoding.UTF8.GetBytes(msg);
			Console.WriteLine($"Send to [ User_ID : {m_sessionId} ] : " + msg);
			m_tcpSocket.Send(bytesPacket);
		}

		

		public void OnClosed()
        {
			Console.WriteLine($"Close Client [ User_ID : {m_sessionId} ]");
			
			m_tcpSocket.Close();
        }

	}
}

