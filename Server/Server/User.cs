using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
	public class User
	{
		private string m_sessionId;
		private Socket m_tcpSocket;

		public User(Socket m_tcpSocket, string session_Id)
		{
			this.m_tcpSocket = m_tcpSocket;
			this.m_sessionId = session_Id;
		}

		public void SendTo(User target, byte[] bytesPacket)
        {
			if (target.m_tcpSocket.Connected)
			{
				
				target.m_tcpSocket.Send(bytesPacket);
				string s = System.Text.Encoding.UTF8.GetString(bytesPacket, 0, bytesPacket.Length);
				Console.WriteLine("Send msg: " + s + " to Client:" + target.m_sessionId);
			}
		}

		public bool IsConnected
        {
			get => m_tcpSocket.Connected;
        }


	}
}

