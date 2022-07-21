using System;
using ProtoBuff.Packet;
namespace Server
{
	public class MQ
	{
		public MQ(string user_id)
		{
			Node temp = new Node();
			temp.Name = user_id;
			temp.Type = "123";

			Queues.Add(temp);

			_msgObserver = new Thread(Run);
			_msgObserver.Start();

		}

		protected Thread? _msgObserver;
		public List<Node> Queues = new();

		public bool isClose = false;


		public struct Node
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public Queue<SamplePacket> packetQueue { get; set; }
		}

		

		public void push(SamplePacket msg) { }
		public void pop() { }
		public void Run() {
			// 假設還跟server連接
            while (!isClose)
            {
				// 檢查 List<Node> 中每個 Node
				foreach (var node in Queues)
                {
					// 假設 node 中的queue裡有資料，就pop出來
					if(node.packetQueue.Count > 0)
                    {
						pop();
						continue;
                    }
                }
				Thread.Sleep(100);

            }
		}


		
	}
}

