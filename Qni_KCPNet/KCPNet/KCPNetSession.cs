// ------------------------------------
//
// FileName: KCPSession.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/13
// Desc:        
//
// ------------------------------------
using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;

namespace KCPNet
{
    public enum ESessionState {
        None,
        Connected,
        Disconnected
    }


    public abstract class KCPNetSession : IKcpCallback {
        protected uint sessionId;
        protected ESessionState sessionState = ESessionState.None;

        private PoolSegManager.Kcp kcp;
        private Action<byte[], IPEndPoint> messageSender;
        private IPEndPoint remoteEndPoint;

        public Action<uint> OnSessionCloseCallback = null;


        //session update task params
        private CancellationTokenSource cts;
        private CancellationToken ct;


        public uint SessionId {
            get {
                return sessionId;
            }
        }

        public bool IsConnected {
            get {
                return sessionState == ESessionState.Connected;
            }
        }



        protected abstract void OnConnected ();
        protected abstract void OnDisconnected ();
        protected abstract void OnUpdate (DateTime utcTime);
        protected abstract void OnReceiveMessage (byte[] buffer);





        /// <summary>
        /// 初始化 Session / init session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="sender"></param>
        /// <param name="endPoint"></param>
        public void Init (uint sessionId, Action<byte[], IPEndPoint> sender, IPEndPoint endPoint) {
            this.sessionId = sessionId;
            this.messageSender = sender;
            this.remoteEndPoint = endPoint;

            sessionState = ESessionState.Connected;

            kcp = new PoolSegManager.Kcp(sessionId, this);

            kcp.NoDelay(1, 10, 2, 1);
            kcp.WndSize(128, 128);
            //kcp.SetMtu(512);

            OnConnected();

            cts = new CancellationTokenSource();
            ct = cts.Token;
            Task.Run(Update, ct);
        }



        /// <summary>
        /// update KCP receive.
        /// </summary>
        private async void Update () {
            try {
                while (true) {
                    DateTime _time = DateTime.UtcNow;
                    OnUpdate(_time);
                    if (ct.IsCancellationRequested) {
                        KCPNetUtils.Logger.LogWithColor(KCPNetUtils.Logger.ELogColor.Cyan, "Session Update Task is Cancelled.");
                        break;
                    }
                    else {
                        kcp.Update(_time);
                        int _len = 0;
                        while ((_len = kcp.PeekSize()) > 0) {
                            var _buffer = new byte[_len];
                            if (kcp.Recv(_buffer) >= 0) {
                                ReceiveDataFormKCP(_buffer);
                            }
                        }
                        await Task.Delay(5);
                    }
                }
            }
            catch (Exception e) {
                KCPNetUtils.Logger.LogWarningFormat("Session Update Exception :{0}", e.ToString());
            }
        }

        #region  receive

        /// <summary>
        /// <para> [接收] 接收 KCP 处理后的数据。</para>
        /// <para> [Receive] receive data form KCP.</para>
        /// </summary>
        /// <param name="buffer"></param>
        private void ReceiveDataFormKCP (byte[] buffer) {
            OnReceiveMessage(buffer);
        }


        /// <summary>
        /// <para> [接收] 接收 UDP 发送的数据，然后传递给 KCP 处理。</para>
        /// <para> [Receive] Pick up the data sent by UDP and pass it to KCP for processing.</para>
        /// </summary>
        /// <param name="bytes"></param>
        public void ReceiveMessage (byte[] bytes) {
            if (bytes == null) {
                KCPNetUtils.Logger.LogWarning("Session receive message is null.");
                return;
            }
            kcp.Input(bytes.AsSpan());
        }
        #endregion


        #region Send

        /// <summary>
        /// <para> [发送] 发送 UDP 的数据到 KCP 中，处理后再发送到目标。</para>
        /// <para> [Send] Send UDP data to KCP, process it, and then send it to the destination.</para>
        /// </summary>
        /// <param name="bytes"></param>
        public void SendMessage (byte[] bytes) {
            if (bytes == null) {
                KCPNetUtils.Logger.LogWarning("Session send message is null.");
                return;
            }

            if (IsConnected) {
                kcp.Send(bytes.AsSpan());
            }
            else {
                //Session未连接到目标。
                //Session is not connected to the target.
                KCPNetUtils.Logger.LogWarning("Session is not connected to the target.");
            }
        }

        /// <summary>
        /// <para>[发送] 发送消息到 KCP 处理后，输出结果的回调处理。(不要手动调用它)</para>
        /// <para>[Send] Send message to KCP for processing,
        /// and handle the callback for the output result. (Do not manually call it.)</para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="avalidLength"></param>
        public void Output (IMemoryOwner<byte> buffer, int avalidLength) {
            var _buffer = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            messageSender(_buffer, remoteEndPoint);
            buffer.Dispose();
        }
        #endregion



        /// <summary>
        /// 关闭 session。
        /// / clsoe current session.
        /// </summary>
        public void Close () {
            cts.Cancel();

            OnDisconnected();

            OnSessionCloseCallback?.Invoke(sessionId);
            OnSessionCloseCallback = null;

            sessionState = ESessionState.Disconnected;
            remoteEndPoint = null;
            messageSender = null;
            kcp = null;
            cts = null;
            sessionId = 0;
        }

        public override bool Equals (object obj) {
            if (obj != null && obj is KCPNetSession) {
                return (obj as KCPNetSession).sessionId == sessionId;
            }
            return false;
        }

        public override int GetHashCode () {
            return sessionId.GetHashCode();
        }

    }
}

