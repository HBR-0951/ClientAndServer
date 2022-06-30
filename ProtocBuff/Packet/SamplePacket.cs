using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packet {
    public class SamplePacket : Packet_Template {

        public string Message {
            get {
                return (m_data != null) ? Encoding.UTF8.GetString(m_data, 0, m_data.Length) : "N/A";
            }
            set {
                m_data = Encoding.UTF8.GetBytes(value);
            }
        }

    }
}
