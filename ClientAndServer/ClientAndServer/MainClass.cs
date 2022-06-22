using System;
using System.Threading;

namespace ClientAndServer
{
    public class MainClass
    {
        public static void Main()
        {
            string ipAddress = "127.0.0.1";
            int port = 8000;


            Thread client = new Thread(() =>
            {
                Client p1 = new Client();
                p1.Connect(ipAddress, port);

                string s = "";
                string msg = "";
                do
                {
                    msg = p1.GetMsg();
                    if (msg != "")
                    {
                        Console.WriteLine("Client get from Server: " + msg);
                        msg = "";
                    }

                    Console.Write("Client write to Server: ");
                    s = Console.ReadLine();
                    p1.SendMsg(s);
                    
                    Thread.Sleep(100);

                } while (true);
            });
            Thread server = new Thread(() =>
            {
                Server p2 = new Server();
                p2.Bind(ipAddress, port);

                p2.Start();

                p2.SendMsg("Hello Client");
                string msg = "";
                string s = "";
                do
                {
                    msg = p2.GetMsg();
                    if(msg != "")
                    {
                        Console.WriteLine("Server get from Client: " + msg);
                        msg = "";
                    }

                    Console.Write("Server write to Client: ");
                    s = Console.ReadLine();
                    p2.SendMsg(s);
                    Thread.Sleep(100);
                } while (true);

            });
            server.Start();
            client.Start();
            
        }


    }
}