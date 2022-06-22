using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Tcp客戶端
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipendpoint = new IPEndPoint(ipaddress, 8000);

            try
            {
                clientSocket.Connect(ipendpoint);
                Console.WriteLine("Connect successfully");
            }
            catch(Exception e)
            {
                Console.WriteLine("Connect faild");
                Console.WriteLine("Waring with " + e.ToString());
            }
         

            byte[] date = new byte[1024];
            int count = clientSocket.Receive(date);
            string msg = Encoding.UTF8.GetString(date, 0, count);
            Console.WriteLine(msg);

            string s = Console.ReadLine();

            while (s!= "Q" && s != "q")
            {
                clientSocket.Send(Encoding.UTF8.GetBytes(s));
                s = Console.ReadLine();
            }

            clientSocket.Close();
        }
    }
}