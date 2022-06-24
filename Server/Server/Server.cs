using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
	public class Server : TcpServer_Template
	{
        
        protected new List<User> m_userList = new(10);

        public Server(string name) : base(name)
        {
            OnInitialize();
        }

        protected override void OnInitialize()
        {

        }

        protected override void OnNewConnection()
        {
            int user_Id = 1;

            // 服務器還處於接受連線狀態時，無限迴圈 -> 執行等待客戶
            while (m_tcpSocket.Connected)
            {

                var userSocket = m_tcpSocket.Accept(); //程式會卡在此處等待客戶端的連線

                // 創建一個user 給連接到的userSocket
                User user = new User(userSocket, user_Id.ToString());
                user_Id++;

                //為 user 開啟 一個執行緒，持續等待該用戶的資料
                _packetReceived.Start(userSocket); // 傳入用戶的 user 作為參數


                m_userList.Add(user); //保存用戶
            }
        }

        protected override void OnPacketReceived(object? sender)
        {
            if (sender == null) return;


            var userSocket = (Socket)sender; //強制將目標轉型成 Socket，因為執行緒啟動的時候傳入了Socket型態的參數

            // 服務器 及 客戶端 還處於接受連線狀態時，無限迴圈 -> 等待接受客戶端資料
            while (m_tcpSocket.Connected && userSocket.Connected)
            {

                // 連線通道可讀資料 不為 0 時，則讀取資料:否則略過
                if (userSocket.Available != 0)
                {

                    // 讀取資料
                    // Socket.Receive();
                    byte[] date = new byte[userSocket.Available];
                    int count = userSocket.Receive(date);
                    string msg = Encoding.UTF8.GetString(date, 0, count);
                    Console.WriteLine("Server get msg: " + msg);


                }
                else
                {
                    Thread.Sleep(100);
                }

            }

        }

        //public void SendTo(User user, string msg)
        //{
        //    byte[] bytesPacket;
        //    bytesPacket = Encoding.UTF8.GetBytes(msg);

        //    user.SendTo(user, bytesPacket);
        //}

        // 廣播
        public void OnBroadcast(string msg)
        {
            byte[] bytesPacket;
            bytesPacket = Encoding.UTF8.GetBytes(msg);

            foreach (var user in m_userList)
            {
                user.SendTo(user, bytesPacket);
            }
        }




    }
}

