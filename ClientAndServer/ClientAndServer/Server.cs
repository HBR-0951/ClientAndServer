using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientAndServer
{
    public class Server
    {
        protected Socket? m_tcpSocket;

        protected string m_host = "localhost";
        protected int m_port = 8888;

        // Thread
        protected Thread? _awaitClient;
        protected Thread? _packetReceived;

        // User
        private static UInt64 m_id = 0;
        protected static UInt64 UID_Generator => m_id++;

        public string Name { get; set; } = "N/A";
        public int Backlog { get; set; } = 5;

        
        protected List<User> m_userList = new(5);

        public bool IsConnected
        {
            get => m_tcpSocket != null && m_tcpSocket.Connected; 
        }

        public Server(string name)
        {
            Name = name;
            m_tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            OnInitialize();
        }

        // 服務器基本設定初始化(配置一些必要參數)
        protected void OnInitialize()
        {

        }

        // 當新連線連接時
        protected void OnNewConnection()
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
                        var uid = UID_Generator;
                        var user = new User(userSocket, uid.ToString());
                        DateTime localtime = DateTime.Now;
                        user.Send($"[ Time: {localtime} ]  Connect to [ Target IP:{userSocket.RemoteEndPoint} ] Successful : [ User_id: {uid} ]");

                        //為 user 開啟 一個執行緒，持續等待該用戶的資料
                        _packetReceived.Start(userSocket); // 傳入用戶的 user 作為參數


                        m_userList.Add(user); //保存用戶
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 監聽目標地址(並未開始監聽，僅作為設定)
        public virtual void ListenTo(string ipAddress, int port)
        {
            m_host = ipAddress;
            m_port = port;

            var IPEndPoint = new IPEndPoint(IPAddress.Parse(m_host), m_port);
            m_tcpSocket.Bind(IPEndPoint); // 綁定監聽目標

            // 創建執行緒，綁定 OnNewConnection方法，並且設定執行緒在後台運行(此時此刻並未啟動)
            _awaitClient = new Thread(OnNewConnection) { IsBackground = false };

        }

        public void OnStart()
        {
            m_tcpSocket.Listen(Backlog); // 開始監聽目標ip位址

            _awaitClient.Start(); // 啟動等待客戶端連線執行緒

        }

        // 廣播
        public void OnBroadcast(string msg)
        {
            Encoding.UTF8.GetBytes(msg);

            foreach (var user in m_userList)
            {
                user.Send(msg);
            }
        }


    }
}