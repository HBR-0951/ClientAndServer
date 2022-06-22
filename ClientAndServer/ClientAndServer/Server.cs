using System;
using System.Net.Sockets;
using System.Net;

namespace ClientAndServer
{
    public class Server
    {
        public Socket serverSocket;
        public Socket clientSocket;

        public Server(){
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Bind(string ipAddress, int port)
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
            serverSocket.Listen(5);//處理連結佇列個數 為0則為不限制
            clientSocket = serverSocket.Accept();//接收一個客戶端連結
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
        //static void Main(string[] args)
        //{


        //    IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
        //    IPEndPoint ipendpoint = new IPEndPoint(ipaddress, 8000);

        //    try
        //    {
        //        serverSocket.Bind(ipendpoint);//繫結完成
        //        Console.WriteLine("Bind successfully");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Bind faild");
        //        Console.WriteLine("Warning with " + e.ToString());
        //    }




        //    ///向客戶端傳送一條訊息
        //    string msg = "Hello client!";
        //    byte[] date = System.Text.Encoding.UTF8.GetBytes(msg);//轉換成為bytes陣列
        //    clientSocket.Send(date);

        //    string msgReceive;

        //    do {
        //        ///接收一條客戶端的訊息
        //        byte[] dateBuffer = new byte[1024];


        //        int count = clientSocket.Receive(dateBuffer);

        //        msgReceive = System.Text.Encoding.UTF8.GetString(dateBuffer, 0, count);
        //        Console.WriteLine(msgReceive);
        //    } while (msgReceive != "Q" && msgReceive != "q");

        //    Console.WriteLine("Connect faild");
        //    Console.ReadKey();
        //    clientSocket.Close();
        //    serverSocket.Close();
        //}
    }
}