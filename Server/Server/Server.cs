using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
	public class Server : TcpServer_Template
	{
        public int user_Id = 1;

        public string Msg;

        protected new List<User> m_userList = new(10);
        

        public Server(string name) :base(name)
        {
            
            //// 創建執行緒，綁定 OnNewConnection方法，並且設定執行緒非在後台運行(此時此刻並未啟動)
            _awaitClient = new Thread(OnNewConnection) { IsBackground = false };
            //// 創建執行緒，綁定 OnPacketReceived方法，並且設定執行緒非在後台運行(此時此刻並未啟動)
            _packetReceived = new Thread(OnPacketReceived) { IsBackground = false };

            OnInitialize();
        }

        protected override void OnInitialize()
        {

        }

        protected override void OnNewConnection()
        {

            Console.WriteLine("Waiting for Connecting.");
            // 服務器還處於接受連線狀態時，無限迴圈 -> 執行等待客戶
            while (true)
            {
                try
                {
                    var userSocket = m_tcpSocket.Accept(); //程式會卡在此處等待客戶端的連線

                    if (userSocket.Connected)
                    {
                        Console.WriteLine("Connected");



                        // 創建一個user 給連接到的userSocket
                        User user = new User(userSocket, user_Id.ToString());
                        user_Id++;
                        
                        SendTo(userSocket, "Hello Client " + (--user_Id) + " !");

                        //為 user 開啟 一個執行緒，持續等待該用戶的資料
                        _packetReceived.Start(userSocket); // 傳入用戶的 user 作為參數

                        //Console.WriteLine("Send to Client " + (--user_Id) + ": " + Msg.ToUpper());
                        //SendTo(user, Msg.ToUpper());

                        
                        m_userList.Add(user); //保存用戶
                        
                    }
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
        }

        protected override void OnPacketReceived(object? sender)
        {
            
            if (sender == null) return;


            var userSocket = (Socket)sender; //強制將目標轉型成 Socket，因為執行緒啟動的時候傳入了Socket型態的參數

            // 服務器 及 客戶端 還處於接受連線狀態時，無限迴圈 -> 等待接受客戶端資料
            while (userSocket.Connected)
            {
                
                // 連線通道可讀資料 不為 0 時，則讀取資料:否則略過
                if (userSocket.Available != 0)
                {

                    // 讀取資料
                    // Socket.Receive();
                    byte[] date = new byte[userSocket.Available];
                    int count = userSocket.Receive(date);
                    Msg = Encoding.UTF8.GetString(date, 0, count);


                    Console.WriteLine("Server get msg: " + Msg);

                    SendTo(userSocket, Msg.ToUpper());
                }
                else
                {
                    Thread.Sleep(100);
                }

            }

        }
        public override void OnStart()
        {
            m_tcpSocket.Listen(Backlog); // 開始監聽目標ip位址

            _awaitClient.Start(); // 啟動等待客戶端連線執行緒

        }
        //// 監聽目標地址(並未開始監聽，僅作為設定)
        //public void ListenTo(string ipAddress, int port)
        //{
        //    m_host = ipAddress;
        //    m_port = port;

        //    var IPEndPoint = new IPEndPoint(IPAddress.Parse(m_host), m_port);
        //    m_tcpSocket.Bind(IPEndPoint); // 綁定監聽目標

        //}

        //// 關閉服務器
        //public void OnClose()
        //{
        //    m_tcpSocket.Close();
        //}
        public void SendTo(User user, string msg)
        {
            byte[] bytesPacket;
            bytesPacket = Encoding.UTF8.GetBytes(msg);
            Console.WriteLine("Send to User: " + msg);
            user.SendTo(user, bytesPacket);
        }
        public void SendTo(Socket userSocket, string msg)
        {
            byte[] bytesPacket;
            bytesPacket = Encoding.UTF8.GetBytes(msg);
            Console.WriteLine("Send to Client: " + msg);
            userSocket.Send(bytesPacket);
        }

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

