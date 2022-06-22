using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ClientAndServer
{
    public class Client
    {
        public Socket clientSocket;
        public Client()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Connect(string ipAddress, int port)
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

    }
}