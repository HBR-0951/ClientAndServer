using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProtoBuff.Packet
{
	public class LoginPacket : Packet_Template
	{

		/// <summary>
		/// Login 的id 、 password 長度
		/// </summary>
		protected int m_SizeOfID { get; set; }
		protected int m_SizeOfPsd { get; set; }


		///<summary>
		/// Login 的 id 、 password 宣告
		/// </summary>
		public string LoginID
		{
			get
			{
				return (m_dataID != null) ? Encoding.UTF8.GetString(m_dataID, 0, m_dataID.Length) : "N/A";
			}
			set
			{
				m_dataID = Encoding.UTF8.GetBytes(value);
			}
		}
		public string LoginPsd
		{
			get
			{
				return (m_dataPsd != null) ? Encoding.UTF8.GetString(m_dataPsd, 0, m_dataPsd.Length) : "N/A";
			}
			set
			{
				m_dataPsd = Encoding.UTF8.GetBytes(value);
			}
		}

		/// <summary>
		/// id, password 封包内容
		/// </summary>
		protected byte[]? m_dataID = null;
		protected byte[]? m_dataPsd = null;

		/// <summary>
		/// id, password 封包内容大小
		/// </summary>
		protected int m_dataIDSize;
		protected int m_dataPsdSize;

		/// <summary>
		/// 目標user_id: -1: 伺服器
		/// </summary>
		public int TargetID { get; set; }


		/// <summary>
		/// 自己的user_id
		/// </summary>
		public int SenderID { get; set; }

		/// <summary>
		/// 封包大小
		/// </summary>
		public new int SizeOfPacket
		{
			get
			{
				if (m_dataID != null && m_dataPsd != null)
				{
					return IndexOf_DataID + m_dataID.Length + m_dataPsd.Length;
				}
				return 0;

			}
		}

		/// <summary>
		/// 變數在封包中的所引位置
		/// </summary>
		public new const int IndexOf_ID = 0;
		public new const int IndexOf_Code = IndexOf_ID + sizeof(int); // ID的型別是int，int 大小為 4
		public new const int IndexOf_Function = IndexOf_Code + sizeof(int); // new
		public const int IndexOf_TargetID = IndexOf_Function + sizeof(int);
		public const int IndexOf_SenderID = IndexOf_TargetID + sizeof(int);

		public const int IndexOf_DataIDSize = IndexOf_SenderID + sizeof(int);
		public const int IndexOf_DataPsdSize = IndexOf_DataIDSize + sizeof(int);
		public const int IndexOf_DataID = IndexOf_DataPsdSize + sizeof(int);
		public int IndexOf_DataPsd
		{
			get
			{
				if (m_dataID != null)
				{
					return IndexOf_DataID + m_dataID.Length;

				}
				return 0;
			}
		}


		/// <summary>
		/// 封裝封包
		/// </summary>
		public override byte[] ToPacketup()
		{
			//指定封包尺寸
			var bytesPacket = new byte[SizeOfPacket];

			System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ID)).CopyTo(bytesPacket, IndexOf_ID);
			System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Code)).CopyTo(bytesPacket, IndexOf_Code);

			System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Function)).CopyTo(bytesPacket, IndexOf_Function);
			System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(TargetID)).CopyTo(bytesPacket, IndexOf_TargetID);
			System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(SenderID)).CopyTo(bytesPacket, IndexOf_SenderID);

			if (m_dataID != null)
			{
				m_dataIDSize = m_dataID.Length;
				System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(m_dataIDSize)).CopyTo(bytesPacket, IndexOf_DataIDSize);
				
				m_dataID.CopyTo(bytesPacket, IndexOf_DataID);
			}
			if (m_dataPsd != null)
			{
				m_dataPsdSize = m_dataPsd.Length;
				System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(m_dataPsdSize)).CopyTo(bytesPacket, IndexOf_DataPsdSize);
				
				m_dataPsd.CopyTo(bytesPacket, IndexOf_DataPsd);
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

			m_dataIDSize = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_DataIDSize, sizeof(int)), 0));
			m_dataPsdSize = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(Extract(bytesPacket, IndexOf_DataPsdSize, sizeof(int)), 0));
			
			m_dataID = Extract(bytesPacket, IndexOf_DataID, m_dataIDSize);

			m_dataPsd = Extract(bytesPacket, IndexOf_DataPsd, m_dataPsdSize);
		

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
				 + "LoginID\t" + LoginID + "\n"
				 + "LoginPsd\t" + LoginPsd + "\n"
				 //+ "Data\t" + Data + "\n" // 需反序列化成爲自己所需的形態
				 + "End-------------------------------------------\n\n";
		}

	}
}

