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

            Console.ReadKey();
            server.OnClose();

            //bool isQuit = false;
            //while (!isQuit)
            //{
            //    Console.WriteLine("q or Q: quit\n" +
            //                 "b or B: Broadcast");
            //    string function = Console.ReadLine();
            //    switch (function)
            //        {
            //            case "Q":
            //            case "q":
            //                Console.WriteLine("Quit");
            //    server.OnClose();
            //    isQuit = true;
            //    break;
            //            case "B":
            //            case "b":
            //                Console.WriteLine("Broadcast");
            //    Console.Write("msg: ");
            //    string msg = Console.ReadLine();
            //    server.OnBroadcast(msg);
            //    break;
            //    default:
            //                Console.WriteLine("Write again!");
            //    break;

            //}

        //}




}
    }
}