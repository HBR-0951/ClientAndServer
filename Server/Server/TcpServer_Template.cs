using System;
using System.Net;
using System.Net.Sockets;

namespace Server {

	public abstract class TcpServer_Template {

		protected Socket m_tcpSocket;

		protected string m_host = "localhost";
		protected int m_port = 8888;

		// 執行緒(底線表示為執行緒變數)
		protected Thread? _awaitClient;
		protected Thread? _packetReceived;

		protected List<Socket> m_userList = new(10);

		public string Name { get; set; } = "N/A";
		public int Backlog { get; set; } = 5;

		public bool IsConnected {
			get => m_tcpSocket != null && m_tcpSocket.Connected;
		}

		public TcpServer_Template(string name) {
			Name = name;
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
#pragma warning disable CS8602 // 可能 null 參考的取值 (dereference)。
                _packetReceived.Start(userSocket); // 傳入用戶的Socket作為參數
#pragma warning restore CS8602 // 可能 null 參考的取值 (dereference)。

                m_userList.Add(userSocket); //保存用戶
			}

        }

		// 監聽目標地址(並未開始監聽，僅作為設定)
		public virtual void ListenTo(string ipAddress, int port) {
			m_host = ipAddress;
			m_port = port;

			var IPEndPoint = new IPEndPoint(IPAddress.Parse(m_host), m_port);
			m_tcpSocket.Bind(IPEndPoint); // 綁定監聽目標

			// 創建執行緒，綁定 OnNewConnection方法，並且設定執行緒在後台運行(此時此刻並未啟動)
			_awaitClient = new Thread(OnNewConnection) { IsBackground = false };

		}
		

		// 啟動服務器(啟動監聽)
		public virtual void OnStart() {
			m_tcpSocket.Listen(Backlog); // 開始監聽目標ip位址

#pragma warning disable CS8602 // 可能 null 參考的取值 (dereference)。
            _awaitClient.Start(); // 啟動等待客戶端連線執行緒
#pragma warning restore CS8602 // 可能 null 參考的取值 (dereference)。


        }

		// 關閉服務器
		public virtual void OnClosed()
		{
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

