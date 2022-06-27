using System;

namespace Client
{
    public class Program
    {
        static void Main()
        {
            Client client1 = new Client("client1");
            client1.BuildEndPoint("127.0.0.1", 8000);

            client1.OnStart();
            
        }
    }
}