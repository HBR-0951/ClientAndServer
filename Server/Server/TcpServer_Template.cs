using System;
using System.Net;
using System.Net.Sockets;

namespace Server {

	public abstract class TcpServer_Template {

		protected Socket m_tcpSocket;

		protected string m_host = "localhost";
		protected int m_port = 8888;

		// 執行緒(底線表示為執行緒變數)
		protected Thread _awaitClient;
		protected Thread _packetReceived;

		protected List<Socket> m_userList = new(10);

		public string Name { get; set; } = "N/A";
		public int Backlog { get; set; } = 5;

		public bool IsConnected {
			get => m_tcpSocket != null && m_tcpSocket.Connected;
		}

		public TcpServer_Template(string name) {
			Name = name;
			// 創建執行緒，綁定 OnNewConnection方法，並且設定執行緒在後台運行(此時此刻並未啟動)
			_awaitClient = new Thread(OnNewConnection) { IsBackground = true };
			// 創建執行緒，綁定 OnPacketReceived方法，並且設定執行緒在後台運行(此時此刻並未啟動)
			_packetReceived = new Thread(OnPacketReceived) { IsBackground = true };
			m_tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		// 服務器基本設定初始化(配置一些必要參數)
		protected abstract void OnInitialize();

		// 當新連線連接時
		protected virtual void OnNewConnection() {
			// 服務器還處於接受連線狀態時，無限迴圈 -> 執行等待客戶
			while (m_tcpSocket.Connected) {

				var userSocket = m_tcpSocket.Accept(); //程式會卡在此處等待客戶端的連線

				//為 user 開啟 一個執行緒，持續等待該用戶的資料
				_packetReceived.Start(userSocket); // 傳入用戶的Socket作為參數

				m_userList.Add(userSocket); //保存用戶
			}

        }

		// 當封包到達時
		protected virtual void OnPacketReceived(object? sender) {
			if (sender == null) return;

			var userSocket = (Socket)sender; //強制將目標轉型成 Socket，因為執行緒啟動的時候傳入了Socket型態的參數

			// 服務器 及 客戶端 還處於接受連線狀態時，無限迴圈 -> 等待接受客戶端資料
			while (m_tcpSocket.Connected && userSocket.Connected) {

				// 連線通道可讀資料 不為 0 時，則讀取資料:否則略過
				if(userSocket.Available != 0) {

					// 讀取資料
					// Socket.Receive();


				} else {
					Thread.Sleep(100);
                }

			}

		}

		// 監聽目標地址(並未開始監聽，僅作為設定)
		public virtual void ListenTo(string ipAddress, int port) {
			m_host = ipAddress;
			m_port = port;

			var IPEndPoint = new IPEndPoint(IPAddress.Parse(m_host), m_port);
			m_tcpSocket.Bind(IPEndPoint); // 綁定監聽目標
			
		}
		

		// 啟動服務器(啟動監聽)
		public virtual void OnStart() {
			m_tcpSocket.Listen(Backlog); // 開始監聽目標ip位址
			
			_awaitClient.Start(); // 啟動等待客戶端連線執行緒
			

		}

		// 關閉服務器
		public virtual void OnClose() {
			m_tcpSocket.Close();
        }

		// 廣播
		public virtual void OnBroadcast() {
            foreach (var userSocket in m_userList) {
				// userSocket.Send(data);
            }
        }

	}

}

