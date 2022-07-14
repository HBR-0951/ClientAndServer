using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProtoBuff.Packet {
    public class SamplePacket : Packet_Template {

        public string Message {
            get {
                return (m_data != null) ? Encoding.UTF8.GetString(m_data, 0, m_data.Length) : "N/A";
            }
            set {
                m_data = Encoding.UTF8.GetBytes(value);
            }
        }

        /// <summary>
        /// 目標user_id: -1: 伺服器
        /// </summary>
        public int TargetID { get; set; }

        /// <summary>
        /// 封包要做的動作: 0: 傳送空封包,
        ///              1: 傳送userID給client
        ///              2: server轉發,
        ///              3: 給server,
        ///              4: server 給 client
        ///              
        ///              
        /// </summary>
        public int Function { get; set; }

        /// <summary>
        /// 自己的user_id
        /// </summary>
        public int SenderID { get; set; }

        public new int SizeOfPacket => m_data == null ? 0 : IndexOf_Data + m_data.Length;

        /// <summary>
        /// 變數在封包中的所引位置
        /// </summary>
        public new const int IndexOf_ID = 0;
        public new const int IndexOf_Code = IndexOf_ID + sizeof(int); // ID的型別是int，int 大小為 4
        public const int IndexOf_Function = IndexOf_Code + sizeof(int); // new
        public const int IndexOf_TargetID = IndexOf_Function + sizeof(int); // new
        public const int IndexOf_SenderID = IndexOf_TargetID + sizeof(int); // new
        public new const int IndexOf_dataSize = IndexOf_SenderID + sizeof(int);
        public new const int IndexOf_Data = IndexOf_dataSize + sizeof(int);




        /// <summary>
        /// 封裝封包
        /// </summary>
        public override byte[] ToPacketup()
        {

            //指定封包尺寸
            var bytesPacket = new byte[SizeOfPacket];
            //Console.WriteLine(IndexOf_ID);
            //Console.WriteLine(IndexOf_Code);

            //Console.WriteLine(IndexOf_TargetID);
            //Console.WriteLine(IndexOf_dataSize);
            //Console.WriteLine(IndexOf_Data);
            //Console.WriteLine(m_data.Length);
            //Console.WriteLine(SizeOfPacket);


            System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ID)).CopyTo(bytesPacket, IndexOf_ID);
            System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Code)).CopyTo(bytesPacket, IndexOf_Code);

            System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Function)).CopyTo(bytesPacket, IndexOf_Function);
            System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(TargetID)).CopyTo(bytesPacket, IndexOf_TargetID);
            System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(SenderID)).CopyTo(bytesPacket, IndexOf_SenderID);

            if (m_data != null)
            {
                m_dataSize = m_data.Length;
                System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(m_dataSize)).CopyTo(bytesPacket, IndexOf_dataSize);
                m_data.CopyTo(bytesPacket, IndexOf_Data);
            }

            return bytesPacket;
        }

        /// <summary>
        /// 解析封包
        /// </summary>
        public override void UnPack(byte[] bytesPacket)
        {

            ID = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_ID, sizeof(int)), 0));
            Code = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_Code, sizeof(int)), 0));
            Function = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_Function, sizeof(int)), 0));
            TargetID = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_TargetID, sizeof(int)), 0));
            SenderID = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_SenderID, sizeof(int)), 0));
            m_dataSize = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_dataSize, sizeof(int)), 0));
            m_data = Extract(bytesPacket, IndexOf_Data, m_dataSize);

        }

        public new string ToString()
        {
            return
                 "\n Packet Content\n"
                 + "----------------------------------------------\n"
                 + "ID\t" + ID + "\n"
                 + "Code\t" + Code + "\n"
                 + "Function\t" + Function + "\n"
                 + "TargetID\t" + TargetID + "\n"
                 + "SenderID\t" + SenderID + "\n"
                 + "m_dataSize\t" + m_dataSize + "\n"
                 //+ "Data\t" + Data + "\n" // 需反序列化成爲自己所需的形態
                 + "End-------------------------------------------\n\n";
        }

    }
}
