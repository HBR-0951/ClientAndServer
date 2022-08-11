using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtoBuff.Packet;


namespace Server
{
    public partial class Server : TcpServer_Template
    {
        private static UInt64 session_id = 0;
        private static int user_id = 0;


        protected static UInt64 SID_Generator => session_id++;
        protected static int UID_Generator => user_id++;

        protected new List<User> m_userList = new();

        protected Thread _checkUserisOffLine;
        protected Thread _processingMsg;
         

        protected bool isCloseServer = false;


        // MQ
        public MQ mq;
        public static Queue<byte[]> MQ_Queue = new Queue<byte[]>();
        protected Thread _queueEvent;

        // MySqlDB
        protected TestDB mySqlDB = new TestDB();



#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        public Server(string name) : base(name)
        {
            _checkUserisOffLine = new Thread(checkUserOffLine);
            _queueEvent = new Thread(OnQueueEventHandler);
            OnInitialize();
        }
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。

        protected override void OnInitialize()
        {
            OnInitializeDispatcher();
        }

        protected override void OnNewConnection()
        {

            Console.WriteLine("Waiting for Connecting.");
            // 服務器還處於接受連線狀態時，無限迴圈 -> 執行等待客戶
            while (!isCloseServer)
            {
                try
                {

                    var userSocket = m_tcpSocket.Accept(); //程式會卡在此處等待客戶端的連線

                    if (userSocket.Connected)
                    {
                        Console.WriteLine("Connected");

                        // 創建一個user 給連接到的userSocket
                        var sid = SID_Generator;
                        var uid = UID_Generator;

                        var user = new User(userSocket, sid.ToString(), uid);
                        //// 加上packet事件處理方法
                        //user.PacketEvent += OnUserPacketEventHandler;
                        // 加上packet MQ 方法
                        //user.PacketMQEvent += OnPacketMQ;

                        DateTime localtime = DateTime.Now;
                        string msg = $"[ Time: {localtime} ]  Connect to [ Target IP:{m_tcpSocket.RemoteEndPoint} ] Successful : [ User_id: {uid} ]";
                        Console.WriteLine(msg);

                        m_userList.Add(user); //保存用戶

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        public override void OnStart()
        {
            m_tcpSocket.Listen(Backlog); // 開始監聽目標ip位址

          

            if (_awaitClient != null)
            {
                _awaitClient.Start(); // 啟動等待客戶端連線執行緒
            }


            _checkUserisOffLine.Start(); // 檢查client 有無斷線
            _queueEvent.Start();
            mySqlDB.OnStart();

            //// 訂閱MQ
            //mq = new MQ(this.Name);
            //mq.PacketEvent += OnUserPacketEventHandler;



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
            if (m_userList != null)
            {

                foreach (var user in m_userList)
                {
                    user.isOffLine = true;
                }
            }

            Thread.Sleep(100);

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

        protected void OnQueueEventHandler()
        {
            while (!isCloseServer)
            {
                // 假設queue裡有packet
                if (MQ_Queue.Count != 0)
                {
                    //pop
                    byte[] bytesPacket = MQ_Queue.Dequeue();
                    Packet_Template packet = new Packet_Template();
                    packet.UnPack(bytesPacket);

                    

                    string service = ((ProtoBuff.Packet.Type)packet.Function).ToString();
              

                    m_cmdDispatcher.Dispatch(service, bytesPacket);
                }
                Thread.Sleep(100);
            }
        }

    }
}
