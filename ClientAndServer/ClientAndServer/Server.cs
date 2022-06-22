using System;
using System.Net.Sockets;
using System.Net;

namespace ClientAndServer
{
    public class Server
    {
        public Socket serverSocket;
        public Socket clientSocket;
        public string ipAddress;
        public int port;
        public string name;

        public Server(string ipAddress, int port)
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ipAddress = ipAddress;
            this.port = port;

        }

        public void Bind()
        {
            IPAddress ipaddress = IPAddress.Parse(ipAddress);
            IPEndPoint ipendpoint = new IPEndPoint(ipaddress, port);

            try
            {
                serverSocket.Bind(ipendpoint);//繫結完成
                Console.WriteLine("Bind successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine("Bind faild");
                Console.WriteLine("Warning with " + e.ToString());
            }
        }

        public void Start()
        {
            try
            {
                serverSocket.Listen(5);//處理連結佇列個數 為0則為不限制
                clientSocket = serverSocket.Accept();//接收一個客戶端連結
                Console.WriteLine("Client: " + clientSocket.RemoteEndPoint + " have connected.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }
        public string GetMsg()
        {
            byte[] dateBuffer = new byte[1024];


            int count = clientSocket.Receive(dateBuffer);

            string msgReceive = System.Text.Encoding.UTF8.GetString(dateBuffer, 0, count);
            return msgReceive;
        }

        public void SendMsg(string msg)
        {
            byte[] date = System.Text.Encoding.UTF8.GetBytes(msg);//轉換成為bytes陣列
            clientSocket.Send(date);
        }

        public void Close()
        {
            clientSocket.Close();
            serverSocket.Close();
        }

        public void run()
        {
            //Server server = new Server();
            Bind();


            string msg = "";
            string s = "";
            Start();

            do
            {
                
                SendMsg("Hello Client !");
                msg = GetMsg();
                if (msg != "")
                {
                    Console.WriteLine("Server "+ this.name + " get from Client: " + msg);
                    msg = "";
                }

                Console.Write("Server " + this.name + " write to Client: ");
                s = Console.ReadLine();
                SendMsg(s);
                Thread.Sleep(10);
            } while (true);
        }
    }
}