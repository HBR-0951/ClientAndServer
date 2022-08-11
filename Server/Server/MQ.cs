using System;
using ProtoBuff.Packet;
namespace Server
{
	public class MQ
	{
#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        public MQ(string serverName)
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        {
			Node temp = new Node();
			temp.Name = serverName;
			temp.Type = "123";
			temp.packetQueue = new Queue<MsgPacket>();

			Queues.Add(temp);

			_msgObserver = new Thread(Run);
			_msgObserver.Start();

		}

		protected Thread? _msgObserver;
		public List<Node> Queues = new();

		public delegate void PacketHandler(MsgPacket packet);
		public event PacketHandler PacketEvent;

		public bool isClose = false;

		public struct Node
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public Queue<MsgPacket> packetQueue { get; set; }
		}

		

		public void push(string serverName, MsgPacket packet) {
			foreach(var node in Queues)
            {
				if(node.Name == serverName)
                {
					node.packetQueue.Enqueue(packet);
					break;
                }
            }
		}
		public MsgPacket pop(Node node) {
			MsgPacket packet = node.packetQueue.Dequeue();

			return packet;
		}
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
						var packet = pop(node);
						//var target_id = packet.TargetID;
						//int sender_id = packet.SenderID;
						//string msg = packet.Message;
						//int function = packet.Function;
						//var packetLength = packet.SizeOfPacket;

      //                  Console.WriteLine("\ntargetid: " + target_id
      //                                  + "\nsenderid: " + sender_id
      //                                  + "\nfunction: " + function
						//				+ "\npacketLength: " + packetLength
						//				+ "\nmsg: " + msg);
                        this.OnPacket(packet);
						continue;
                    }
                }
				Thread.Sleep(100);

            }
		}

		public void OnPacket(MsgPacket packet)
        {
			if(PacketEvent != null)
            {
				PacketEvent.Invoke(packet);
            }
        }


		
	}
}

