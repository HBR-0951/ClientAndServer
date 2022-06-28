using System;
using System.Net;
using System.Net.Sockets;

namespace ClientAndServer
{
	public class User
	{
		private readonly string m_sessionId;
		private readonly Socket m_tcpSocket;
		protected Thread PacketReceivedThread { get; private set; }

		public bool IsConnected
		{
			get => m_tcpSocket.Connected;
		}

		public User(Socket m_tcpSocket, string session_Id)
		{
			this.m_tcpSocket = m_tcpSocket;
			this.m_sessionId = session_Id;

			// Thread
			PacketReceivedThread = new Thread(OnPacketReceived) { IsBackground = false };
			PacketReceivedThread.Start();
		}

		// 當封包到達時
		public virtual void OnPacketReceived()
		{

			// 服務器 及 客戶端 還處於接受連線狀態時，無限迴圈 -> 等待接受客戶端資料
			while (m_tcpSocket.Connected)
			{

				// 連線通道可讀資料 不為 0 時，則讀取資料:否則略過
				if (m_tcpSocket.Available != 0)
				{

					// 讀取資料
					// Socket.Receive();
					byte[] date = new byte[m_tcpSocket.Available];
					int count = m_tcpSocket.Receive(date);
					var msg = System.Text.Encoding.UTF8.GetString(date, 0, count);
					Console.WriteLine($"Server Data Received from [ User_ID : {m_sessionId} ]: " + msg);

					Send(msg.ToUpper());

				}
				else
				{
					Thread.Sleep(100);
				}

			}

		}

		public void Send(string msg)
        {
			if (!m_tcpSocket.Connected) return;

			byte[] bytesPacket;
			bytesPacket = System.Text.Encoding.UTF8.GetBytes(msg);
			Console.WriteLine($"Send to [ User_ID : {m_sessionId} ] : " + msg);
			m_tcpSocket.Send(bytesPacket);
		}

		


	}
}

