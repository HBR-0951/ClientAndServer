using System;
using System.Net.Sockets;
using System.Net;

using ProtoBuff.Packet;

namespace ProtoBuff
{
    class Program {
        static void Main(string[] args) {

            // 發送者
            var packet = new SamplePacket();
            packet.ID = 123;
            packet.Code = 0;
            packet.TargetID = 1;
            packet.SenderID = 1;
            packet.Function = 0;
            packet.Message = "Hello";
            byte[] bytesPacket = packet.ToPacketup();

            // send(bytesPacket); //網路傳輸

            // 接收者
            // Remote Received Packet
            var receivedPacket = new SamplePacket();
            receivedPacket.UnPack(bytesPacket);
            var sid = receivedPacket.Message;
            var infos = receivedPacket.ToString(); // 顯示封包内容

            Console.WriteLine(infos);

        }
    }
}