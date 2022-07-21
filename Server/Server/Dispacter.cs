using System;
using ProtoBuff.Packet;

namespace Server
{
	public delegate void MsgHandler(SamplePacket pack);

	public class Dispacter
	{
		public Dispacter()
		{
		}

		private Dictionary<string, MsgHandler> m_disp = new();

		public bool Register(string key, MsgHandler value) {
			// 假設 key 不包含在 dictionary，把他加進去
            if (! m_disp.ContainsKey(key))
            {
				m_disp.Add(key, value);
				return true;
            }
            else
            {
				return false;
            }
		}

		public bool Dispatch (string key, SamplePacket packet)
        {
			// 假設key存在 dictionary 裡
            if (m_disp.ContainsKey(key))
            {
				MsgHandler value = m_disp[key];

				// 執行value（把packet放進 value 裡）
				if(value != null)
                {
					value.Invoke(packet);
                }

				return true;
				
            }
            else
            {
				return false;
            }
        }
	}
}

