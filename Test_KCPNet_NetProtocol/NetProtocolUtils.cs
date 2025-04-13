// ------------------------------------
//
// FileName: NetMessageProtocolUtils.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/13
// Desc:        
//
// ------------------------------------
using System;
using System.IO;

namespace Test_KCPNet_NetProtocol {

    public class NetMessageProtocolUtils {

        public static object Deserialize (byte[] bytes) {
            return Deserialize<object>(bytes);
        }

        public static T Deserialize<T> (byte[] bytes) where T: class {
            if (bytes == null) {
                return null;
            }
            using (MemoryStream st = new MemoryStream(bytes)) {
                return ProtoBuf.Serializer.Deserialize<T>(st);
            }
        }

        public static byte[] Serialize (object msg) {
            return Serialize<object>(msg);
        }

        public static byte[] Serialize<T> (T msg) where T : class {
            using (MemoryStream ms = new MemoryStream()) {
                try {
                    ProtoBuf.Serializer.Serialize(ms, msg);

                    byte[] bytes = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, bytes, 0, (int)ms.Length);
                    return bytes;
                }
                catch (Exception ex) {
                    LogError(ex.ToString());
                }

                return null;
            }
        }

        

        private static void LogError (string msg) {
            var _color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(msg);
            Console.ForegroundColor = _color;
        }
    }
}

