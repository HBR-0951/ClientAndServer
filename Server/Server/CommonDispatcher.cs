using System;
namespace Server {

    // 一般分配器
    public class CommonDispatcher {

        public delegate void PacketHandler(byte[] bytesPacket);

        // Dict
        protected Dictionary<string, PacketHandler> m_disp = new();

        // 註冊器
        public void Register(string type, PacketHandler method) {
            // 判斷是否尚未註冊
            if (m_disp.ContainsKey(type) == false) {
                m_disp.Add(type, method); // 方法註冊

            } else {
                // type已註冊
                Console.WriteLine($"[Warning] Type is Registed : type: {type}");
            }

        }

        // 分配器
        public void Dispatch(string type, byte[] bytesPacket) {

            // 判斷是否已註冊
            if (m_disp.ContainsKey(type) == true) {
                // 嘗試獲取字典內容
                if (m_disp.TryGetValue(type, out var packetHandler)) {
                    packetHandler.Invoke(bytesPacket); // 執行 Callback

                } else {
                    // 字典內容獲取失敗
                    Console.WriteLine($"[Warning] Dispatch Fail : type: {type}");
                }

            } else {
                // type未註冊
                Console.WriteLine($"[Warning] Type is Registed : type: {type}");
            }
        }

        public new void ToString() {
            foreach (var handler in m_disp) {
                Console.WriteLine($"Registed : {handler}");
            }
        }

    }
}

