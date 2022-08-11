using System;
using ProtoBuff.Packet;
namespace Client {
	
	// For Service
	public partial class Client {

		protected CommonDispatcher m_cmdDispatcher = new();


		protected void OnInitializeDispatcher() {


			// 註冊 Demo方法
			m_cmdDispatcher.Register("SendUserID",SendUserID_Ack);
			m_cmdDispatcher.Register("Forward", Forward_Ack);
			m_cmdDispatcher.Register("ServerToClient", ServerToClient_Ack);
			m_cmdDispatcher.Register("Bulk", Bulk_Ack);
			m_cmdDispatcher.Register("Login_Ack", Login_Ack);


			//m_cmdDispatcher.ToString();
		}

		// 得到userID後的動作
		protected void SendUserID_Ack(byte[] bytesPacket)
        {
			MsgPacket receivedPacket = new MsgPacket();
			receivedPacket.UnPack(bytesPacket);

			string msg = receivedPacket.Message;

			this.user_id = Int32.Parse(msg);
            Console.WriteLine("Client get userid: " + this.user_id);
            hasGetUserID = true;

            //// 確認連線後輸入 Login: id, password
            //Console.Write("ID: ");
            //Login_UserID = Console.ReadLine();

            //Console.Write("Password: ");
            //Login_Password = Console.ReadLine();

            int serviceID = (int)ProtoBuff.Packet.Type.Login;
			byte[] Packet = OnBuildLoginPacket(Login_UserID, Login_Password, serviceID);
			Send(Packet);
			
            



		}

		// 得到轉發的訊息後的動作
		protected void Forward_Ack(byte[] bytesPacket) {
			MsgPacket receivedPacket = new MsgPacket();
			receivedPacket.UnPack(bytesPacket);

			var target_id = receivedPacket.TargetID;
			string msg = receivedPacket.Message;

			if (target_id == this.user_id)
            {
                Console.WriteLine("Client get msg: " + msg);
            }
            else
            {
                Console.WriteLine("The msg is sent to wrong place");
            }

        }
        // 得到server傳來的訊息後做的動作
        protected void ServerToClient_Ack(byte[] bytesPacket)
		{

			MsgPacket receivedPacket = new MsgPacket();
			receivedPacket.UnPack(bytesPacket);

			string msg = receivedPacket.Message;

			Console.WriteLine($"Server send msg: {msg}");

		}

		//得到群發後的動作
		protected void Bulk_Ack(byte[] bytesPacket)
        {
			MsgPacket receivedPacket = new MsgPacket();
			receivedPacket.UnPack(bytesPacket);

			string msg = receivedPacket.Message;
			var sender_id = receivedPacket.SenderID;

			Console.WriteLine($"Client [ {sender_id} ] send msg: {msg}");

		}

		// 得到Login成功與否的結果
		protected void Login_Ack(byte[] bytesPacket)
        {
			MsgPacket receivePacket = new MsgPacket();
			receivePacket.UnPack(bytesPacket);

			string msg = receivePacket.Message;

			if(msg == "Success")
            {
				Console.WriteLine("Login Success");
				isLogin = true;
				// 登入後開始可以傳訊息
				_sendPacket.Start();

			}
            else
            {
				Console.WriteLine(msg);
				Console.WriteLine("Enter the wrong id or password, please enter again!");

				// 輸入 Login: id, password
				Console.Write("ID: ");
				Login_UserID = Console.ReadLine();

				Console.Write("Password: ");
				Login_Password = Console.ReadLine();


				int serviceID = (int)ProtoBuff.Packet.Type.Login;
				byte[] Packet = OnBuildLoginPacket(Login_UserID, Login_Password, serviceID);
				Send(Packet);


			}


		}

		

	}

}

