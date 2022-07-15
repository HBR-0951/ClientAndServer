using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtoBuff.Packet;

namespace Server {
    public class Server : TcpServer_Template {
        private static UInt64 session_id = 0;
        private static int user_id = 0;


        protected static UInt64 SID_Generator => session_id++;
        protected static int UID_Generator => user_id++;

        protected new List<User> m_userList = new(10);

        protected Thread _checkUserisOffLine;
        protected Thread _processingMsg;
        

        protected bool isCloseServer = false;

#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        public Server(string name) : base(name) {
            _checkUserisOffLine = new Thread(checkUserOffLine);
            //_processingMsg = new Thread(ProcessMessage);
            OnInitialize();
        }
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。

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
                        var sid = SID_Generator;
                        var uid = UID_Generator;
                      
                        var user = new User(userSocket, sid.ToString(), uid);
                        user.PacketEvent += OnUserPacketEventHandler;

                        DateTime localtime = DateTime.Now;
                        string msg = $"[ Time: {localtime} ]  Connect to [ Target IP:{m_tcpSocket.RemoteEndPoint} ] Successful : [ User_id: {uid} ]";
                        Console.WriteLine(msg);

                        m_userList.Add(user); //保存用戶
                        
                    }

                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        public override void OnStart() {
            m_tcpSocket.Listen(Backlog); // 開始監聽目標ip位址

#pragma warning disable CS8602 // 可能 null 參考的取值 (dereference)。
            _awaitClient.Start(); // 啟動等待客戶端連線執行緒
#pragma warning restore CS8602 // 可能 null 參考的取值 (dereference)。
            _checkUserisOffLine.Start(); // 檢查client 有無斷線
            //_processingMsg.Start(); // 處理經過的 Message
            

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

        //// 廣播
        //public void OnBroadcast(string msg) {
            
        //    Console.WriteLine("BroadCast: " + msg);

        //    foreach (var user in m_userList) {
        //        Console.WriteLine("1");
        //        user.Send(msg);
        //    }
        //}

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


        //找到前往的user然後傳送封包
        public void OnUserPacketEventHandler(byte[] bytesPacket)
        {
            var receivedPacket = new SamplePacket();
            receivedPacket.UnPack(bytesPacket);

            var function = receivedPacket.Function;

            // 假設是轉發
            if(function == 2)
            {
                var target_id = receivedPacket.TargetID;
                var sender_id = receivedPacket.SenderID;
                //Console.WriteLine("sender_id: " + sender_id);
                //Console.WriteLine("target_id: " + target_id);

                bool hasSend = false;
                foreach (var user in m_userList)
                {
                    if (target_id != sender_id && target_id == user.user_ID)
                    {

                        user.Send(bytesPacket);
                        hasSend = true;
                        break;
                    }
                }
                string msg = "Send Successfully";
                if (!hasSend)
                {
                    msg = "Can't find target user";
                    foreach (var user in m_userList)
                    {
                        if (user.user_ID == sender_id)
                        {
                            byte[] data = user.OnBuildPacket(msg, 4, sender_id);
                            user.Send(data);
                            break;
                        }
                    }
                }
            }
            // 假設是群發
            else if(function == 5)
            {
                var sender_id = receivedPacket.SenderID;
                foreach (var user in m_userList)
                {
                    if (user.user_ID != sender_id)
                    {
                        user.Send(bytesPacket);
                        continue;
                    }
                }
                
            }
            
        }
    }
}

