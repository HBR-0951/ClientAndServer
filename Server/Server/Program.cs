using System;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("server1"); // 建立一個Server

            server.ListenTo("127.0.0.1", 8000); // 監聽目標位址
            
            server.OnStart(); // 啟動服務器(啟動監聽)
            
            //server.OnBroadcast("Hello Client!");
            //server.OnClose();


        }
    }
}