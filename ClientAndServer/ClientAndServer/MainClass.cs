using System;
using System.Threading;

namespace ClientAndServer
{
    public class MainClass
    {
        public static void Main()
        {
            Server S1 = new Server("127.0.0.1",8000);
            Client C1 = new Client("127.0.0.1", 8000);
            //Client C2 = new Client("127.0.0.1", 8000);
            S1.name = "S1";
            C1.name = "C1";
            //C2.name = "C2";

            
            Thread pcServer = new Thread(S1.run);
            Thread person1 = new Thread(C1.run);
            //Thread person2 = new Thread(C2.run);
            pcServer.Start();
            person1.Start();
            //person2.Start();
            
        }
    }
}