using System;
namespace Server {

	// For Service
	public partial class Server : TcpServer_Template {

		protected CommonDispatcher m_cmdDispatcher = new();

		protected void OnInitializeDispatcher() {
			m_cmdDispatcher.Register("Demo", Demo); // 註冊 Demo方法

			m_cmdDispatcher.ToString();
		}

		protected void Demo(byte[] bytesPacket) {
            Console.WriteLine("Test: Demo");
        }

	}

}

