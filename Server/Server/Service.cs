using System;
using ProtoBuff.Packet;
namespace Server {
	
	// For Service
	public partial class Server : TcpServer_Template {

		protected CommonDispatcher m_cmdDispatcher = new();


		protected void OnInitializeDispatcher() {


			// 註冊 Demo方法
			m_cmdDispatcher.Register("Forward", Forward); 
			m_cmdDispatcher.Register("Bulk", Bulk);
			m_cmdDispatcher.Register("ClientToServer_Ack", ClientToServer_Ack);

			//m_cmdDispatcher.ToString();
		}

		// 轉發
		protected void Forward(byte[] bytesPacket) {
			SamplePacket receivedPacket = new SamplePacket();
			receivedPacket.UnPack(bytesPacket);

			bool hasSend = false;

			var target_id = receivedPacket.TargetID;
			var sender_id = receivedPacket.SenderID;
			string msg = receivedPacket.Message;
            int function = receivedPacket.Function;
            var packetLength = receivedPacket.SizeOfPacket;

            foreach (var user in m_userList)
            {
                if (target_id != sender_id && target_id == user.user_ID)
                {
					// 重新變成封包
					var newBytesPacket = user.OnBuildPacket(msg, function, target_id);
					user.Send(newBytesPacket);
                    hasSend = true;
                    break;
                }
            }

            if (!hasSend)
            {
                string s = "Can't find target user";
                foreach (var user in m_userList)
                {
                    if (user.user_ID == sender_id)
                    {
						// 重新變成封包
						var newBytesPacket = user.OnBuildPacket(s, 4, sender_id);
                        user.Send(newBytesPacket);
                        break;
                    }
                }
				Console.WriteLine($"user [{sender_id}] {s} [{target_id}].");
            }
        }

		//群發
		protected void Bulk(byte[] bytesPacket)
        {
			SamplePacket receivedPacket = new SamplePacket();
			receivedPacket.UnPack(bytesPacket);

			var target_id = receivedPacket.TargetID;
			var sender_id = receivedPacket.SenderID;
			string msg = receivedPacket.Message;
			int function = receivedPacket.Function;
			var packetLength = receivedPacket.SizeOfPacket;

			foreach (var user in m_userList)
            {
                if (user.user_ID != sender_id)
                {
					// 重新變成封包
                    var newbBytesPacket = user.OnBuildPacket(msg, function, target_id);
                    user.Send(newbBytesPacket);
                    continue;
                }
            }
        }

		// client to server
		protected void ClientToServer_Ack(byte[] bytesPacket)
        {
			SamplePacket receivedPacket = new SamplePacket();
			receivedPacket.UnPack(bytesPacket);

			var senderid = receivedPacket.SenderID;
			var msg = receivedPacket.Message;

            Console.WriteLine("Server get msg from user[ " + senderid + " ]: " + msg);
		}

	}

}

