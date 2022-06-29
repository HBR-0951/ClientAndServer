using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server {
    public class Server : TcpServer_Template {
        private static UInt64 m_id = 0;

        protected static UInt64 UID_Generator => m_id++;

        protected new List<User> m_userList = new(10);

        protected Thread _checkUserisOffLine;
        protected Thread _processingMsg;
        

        protected bool isCloseServer = false;

        public Server(string name) : base(name) {
            _checkUserisOffLine = new Thread(checkUserOffLine);
            _processingMsg = new Thread(ProcessMessage);
            OnInitialize();
        }

        protected override void OnInitialize() {

        }

        protected override void OnNewConnection() {

            Console.WriteLine("Waiting for Connecting.");
            // 服務器還處於接受連線狀態時，無限迴圈 -> 執行等待客戶
            while (!isCloseServer) {
                try {
                    
                    var userSocket = m_tcpSocket.Accept(); //程式會卡在此處等待客戶端的連線

                    if (userSocket.Connected) {
                        Console.WriteLine("Connected");

                        // 創建一個user 給連接到的userSocket
                        var uid = UID_Generator;
                        var user = new User(userSocket, uid.ToString());
                        DateTime localtime = DateTime.Now;
                        user.Send($"[ Time: {localtime} ]  Connect to [ Target IP:{userSocket.RemoteEndPoint} ] Successful : [ User_id: {uid} ]");


                        m_userList.Add(user); //保存用戶
                        
                    }

                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        public override void OnStart() {
            m_tcpSocket.Listen(Backlog); // 開始監聽目標ip位址

            _awaitClient.Start(); // 啟動等待客戶端連線執行緒
            _checkUserisOffLine.Start(); // 檢查client 有無斷線
            _processingMsg.Start(); // 處理經過的 Message
            

        }
        public void OnClose()
        {
            OnClosing();
            OnClosed();
        }
        public override void OnClosed()
        {
            m_tcpSocket.Close();
            Console.WriteLine("Server has closed");
        }

        public void OnClosing()
        {
            isCloseServer = true;
            if(m_userList != null)
            {
                
                foreach (var user in m_userList)
                {
                    user.isOffLine = true;
                }
            }
            
            Thread.Sleep(100);

        }

        // 廣播
        public void OnBroadcast(string msg) {
            
            Console.WriteLine("BroadCast: " + msg);

            foreach (var user in m_userList) {
                Console.WriteLine("1");
                user.Send(msg);
            }
        }

        protected void checkUserOffLine()
        {
            
            while (!isCloseServer)
            {
                if (m_userList != null)
                {
                    foreach (var user in m_userList)
                    {
                        if (user.isOffLine)
                        {
                            Console.WriteLine("Remove Client in UserList");
                            m_userList.Remove(user);
                            break;
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        protected void ProcessMessage()
        {
            while (!isCloseServer)
            {
                if(m_userList != null)
                {
                    string msg;
                    User sendUser;
                    foreach (var user in m_userList)
                    {
                        if (user.hasMsg)
                        {
                            msg = user.receiveMsg;
                            sendUser = user;
                            bool isSend = false;
                            foreach (var receiver in m_userList)
                            {
                                if(receiver != sendUser)
                                {
                                    receiver.Send(msg);
                                    isSend = true;
                                }
                            }
                            if (!isSend)
                            {
                                Console.WriteLine("Can't find Client to receive message.");
                            }
                            else
                            {
                                Console.WriteLine("Send Successfully");
                            }
                            isSend = true;
                            user.hasMsg = false;
                            break;
                         
                        }
                    }
                    
                }
                Thread.Sleep(100);
            }
        }
    }
}

