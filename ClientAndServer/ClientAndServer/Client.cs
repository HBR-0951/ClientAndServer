using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ClientAndServer
{
    public class Client
    {
        public Socket clientSocket;
        public string ipAddress;
        public int port;
        public string name;

        public Client(string ipAddress, int port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ipAddress = ipAddress;
            this.port = port;

        }
        public void Connect()
        {
            
            IPAddress ipaddress = IPAddress.Parse(ipAddress);
            IPEndPoint ipendpoint = new IPEndPoint(ipaddress, port);

            try
            {
                clientSocket.Connect(ipendpoint);
                Console.WriteLine("Connect successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine("Connect faild");
                Console.WriteLine("Waring with " + e.ToString());
            }
        }
        public string GetMsg()
        {
            byte[] date = new byte[1024];
            int count = clientSocket.Receive(date);
            string msg = Encoding.UTF8.GetString(date, 0, count);

            return msg;
        }
        public void SendMsg(string msg)
        {
            clientSocket.Send(Encoding.UTF8.GetBytes(msg));
        }
        public void Close()
        {
            clientSocket.Close();
        }
        public void run()
        {
            //Client client = new Client();
            Connect();

            string s = "";
            string msg = "";
            do
            {
                //// Get msg
                //msg = GetMsg();
                //if (msg != "")
                //{
                //    Console.WriteLine("Client " + this.name + " get from Server: " + msg);
                //    msg = "";
                //}
                // Receive msg
                Console.WriteLine("Client");
                Console.Write("Client " + this.name + " write to Server: ");
                s = Console.ReadLine();
                SendMsg(s);
                s = "";

            } while (true);
        }

    }
}