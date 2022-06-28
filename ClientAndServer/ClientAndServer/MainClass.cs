using System;
using System.Threading;

namespace ClientAndServer
{
    public class MainClass
    {
        public static void Main()
        {
            Server server = new Server("server");
            Client client1 = new Client("client1");
            Client client2 = new Client("client2");

            server.ListenTo("127.0.0.1", 8000);
            client1.BuildEndPoint("127.0.0.1", 8000);
            client2.BuildEndPoint("127.0.0.1", 8000);

            server.OnStart();
            client1.OnStart();
            client2.OnStart();



        }
    }
}