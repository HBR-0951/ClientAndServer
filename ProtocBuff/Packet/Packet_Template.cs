using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProtoBuff.Packet
{
    public abstract class Packet_Template {

        // Customer Var >> 自定義封包内容
        // 如果需要更多的參數 或 自定義内容，可繼承此類進行拓展，
        // 如果因拓展而改變了内容結構，則必須自行override 封裝 及 解析

        /// <summary>
        /// 封包ID： 用於識別封包
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 代碼資訊： 表示該封包的狀態(并非一定要使用，可忽略)
        /// </summary>
        /* 例如：當 Code等於 >>
         *  0 ： 通過
         *  1 ： 錯誤A
         *  2 ： 錯誤B
         *  404 : NotFound
         */
        public int Code { get; set; }

        /// <summary>
        /// 封包長度
        /// </summary>
        public int SizeOfPacket => m_data == null ? 0 : IndexOf_Data + m_data.Length;

        /// <summary>
        /// 封包内容
        /// </summary>
        protected byte[]? m_data = null;

        /// <summary>
        /// 封包内容大小
        /// </summary>
        protected int m_dataSize;

        /// <summary>
        /// 變數在封包中的所引位置
        /// </summary>
        public const int IndexOf_ID = 0;
        public const int IndexOf_Code = IndexOf_ID + sizeof(int); // ID的型別是int，int 大小為 4
        public const int IndexOf_dataSize = IndexOf_Code + sizeof(int);
        public const int IndexOf_Data = IndexOf_dataSize + sizeof(int);



        /// <summary>
        /// 封裝封包
        /// </summary>
        public virtual byte[] ToPacketup() {

            //指定封包尺寸
            var bytesPacket = new byte[SizeOfPacket];

            
            System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ID)).CopyTo(bytesPacket, IndexOf_ID);
            System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Code)).CopyTo(bytesPacket, IndexOf_Code);
            
            
            
            if (m_data != null) {
                m_dataSize = m_data.Length;
                System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(m_dataSize)).CopyTo(bytesPacket, IndexOf_dataSize);
                m_data.CopyTo(bytesPacket, IndexOf_Data);
            }
           
            return bytesPacket;
        }

        /// <summary>
        /// 解析封包
        /// </summary>
        public virtual void UnPack(byte[] bytesPacket) {

            ID = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_ID, sizeof(int)), 0));
            Code = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_Code, sizeof(int)), 0));
            m_dataSize = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_dataSize, sizeof(int)), 0));
            m_data = Extract(bytesPacket, IndexOf_Data, m_dataSize);

        }

        /// <summary>
        /// 輸出結果
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return
                 "\n" + base.ToString() + ":\n"
                 + "----------------------------------------------\n"
                 + "ID\t" + ID + "\n"
                 + "Code\t" + Code + "\n"
                 + "m_dataSize\t" + m_dataSize + "\n"
                 //+ "Data\t" + Data + "\n" // 需反序列化成爲自己所需的形態
                 + "End-------------------------------------------\n\n";
        }


        /// <summary>
        /// 提取內容：提取指定位置內容
        /// </summary>
        /// <param name="source_Bytes">被提取的Bytes</param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// /// <param name="InfoStr"></param>
        /// <returns></returns>
        public static byte[] Extract(byte[] source_Bytes, int startIndex, int length) {

            try {
                var dataResult_Bytes = new byte[length];
                Array.Copy(source_Bytes, startIndex, dataResult_Bytes, 0, length);
                return dataResult_Bytes;

            } catch (Exception e) {
                // Print Error
                Console.WriteLine(e.Message);
                return new byte[0];
            }
        }


    }
}
