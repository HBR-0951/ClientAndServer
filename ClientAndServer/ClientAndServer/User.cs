using System;
using System.Net.Sockets;

namespace ClientAndServer
{
	public class User
	{
		private string m_sessionId;
		private Socket m_tcpSocket;
		
		public User(Socket m_tcpSocket, string session_Id) {
			this.m_tcpSocket = m_tcpSocket;
			this.m_sessionId = session_Id;
		}

		//public void Send(byte[] bytesPacket) {
		//	if (m_tcpSocket.Connected)
		//	{
		//		var dataNumber = -1;
		//		while (dataNumber > 0)
		//		{
		//			dataNumber = m_tcpSocket.Available;
		//		}


		//		m_tcpSocket.Send(bytesPacket);
		//		Console.WriteLine("");
		//	}
  //      }

		public void SendTo(User target, byte[] bytesPacket)
        {
            if (target.m_tcpSocket.Connected)
            {
				var datanumber = target.m_tcpSocket.Available;
				if(datanumber > 0)
                {
					target.m_tcpSocket.Send(bytesPacket);
					string s = System.Text.Encoding.UTF8.GetString(bytesPacket, 0, bytesPacket.Length);
					Console.WriteLine("Send msg " + s +  "to: " + target.m_sessionId);

				}

			}

        }


	}
}

