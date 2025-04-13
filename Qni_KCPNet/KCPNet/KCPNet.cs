// ------------------------------------
//
// FileName: KCPNet.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/12
// Desc:        
//
// ------------------------------------
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KCPNet
{
    public class KCPNet {

        protected UdpClient udp;

        //receive or accept task params
        protected CancellationTokenSource cts;
        protected CancellationToken ct;


        public KCPNet () {
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }


        protected void SendMessageByUDPAsync (byte[] bytes, IPEndPoint remotePoint) {
            if (udp != null && bytes != null) {
                udp.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }
    }
}

