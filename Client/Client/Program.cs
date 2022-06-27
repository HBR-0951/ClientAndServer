using System;

namespace Client
{
    public class Program
    {
        static void Main()
        {
            Client client = new Client("client1");
            client.BuildEndPoint("127.0.0.1", 8000);

            client.OnStart();
        }
    }
}