// ------------------------------------
//
// FileName: KCPNetClient.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/12
// Desc:        
//
// ------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KCPNet
{
    public class KCPNetClient<T_Session> : KCPNet where T_Session : KCPNetSession, new() {

        private T_Session session;
        private IPEndPoint remoteEndPoint;


        

        public void Start (string ip, int port) {
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            udp = new UdpClient(0);

            // on Windows platform, udp present error.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }

            KCPNetUtils.Logger.LogWithColor(KCPNetUtils.Logger.ELogColor.Green, "Client Start...");

            Task.Run(ReceiveAsync, ct);
        }



        private async void ReceiveAsync () {
            UdpReceiveResult _result;
            while (true) {
                try {

                    if (ct == null || ct.IsCancellationRequested) {
                        //client 的 session 关闭，结束接收任务。
                        //The client's session is closed, and the receiving task is terminated.
                        break;
                    }
                    
                    _result = await udp.ReceiveAsync();
                    
                    if (Equals(_result.RemoteEndPoint, remoteEndPoint)) {
                        uint _sid = BitConverter.ToUInt32(_result.Buffer, 0);
                        if (_sid == 0) {
                            if (session != null && session.IsConnected) {
                                //接收到多余的 session ，不做处理
                                //Received an extra session, no action taken.
                                KCPNetUtils.Logger.LogWarning("Client received an extra session, no action taken.");
                                return;
                            }
                            else {
                                //首次接收到 服务器发送的 session id，需要初始化 session
                                //first time receiving the session ID sent by the server,
                                //it is necessary to initialize the session.
                                _sid = BitConverter.ToUInt32(_result.Buffer, KCPNetConfig.NET_PACKAGE_HEADER_SIZE);
                                session = new T_Session();
                                session.Init(_sid, SendMessageByUDPAsync, remoteEndPoint);
                                session.OnSessionCloseCallback += OnSessionClose;
                            }
                        }
                        else {
                            //处理数据
                            //process data
                            if (session != null && session.IsConnected) {
                                session.ReceiveMessage(_result.Buffer);
                            }
                            else {
                                //等待完成 session 的初始化，不做处理
                                //Waiting for the completion of session initialization, no action taken.
                                KCPNetUtils.Logger.LogWarning("Client waiting for the completion of session initialization.");
                            }
                        }
                    }
                    else {
                        KCPNetUtils.Logger.LogWarning("Client receive illegal target data.endPoint:" + _result.RemoteEndPoint.ToString());
                    }

                }
                catch (Exception ex) {
                    KCPNetUtils.Logger.LogWarning("Client receive data error :" + ex.ToString());
                }
            }
        }


        /// <summary>
        /// <para> 首次连接到服务器，发送一个空消息，
        /// 服务器检测到后，返回一个 session id，
        /// 客户端接收到这个 session id 后，建立于连接。</para>
        ///
        /// <para> Connect to the server for the first time, send an empty message.
        /// After detecting the message, the server returns a session ID.
        /// client receive the session ID, and establishes a connection.</para>
        /// 
        /// </summary>
        /// <param name="waitInterval">
        /// <para>单次检测连接状态的等待时间。</para>
        /// <para>Waiting time for a single detection of connection status.</para>
        /// </param>
        /// <param name="maxWaitInterval">
        /// <para>检测连接服务器状态的最大等待时间。</para>
        /// <para>Maximum waiting time for detecting server connection status.</para>
        /// </param>
        public Task<bool> ConnectToServer (int waitInterval, int maxWaitInterval = 5000) {
            //连接请求
            //Connection request
            SendMessageByUDPAsync(new byte[KCPNetConfig.NET_PACKAGE_HEADER_SIZE], remoteEndPoint);

            // 连接状态检测
            //Connection state detection
            int _checkTimes = 0;
            Task<bool> _task = Task.Run(async () => {
                while (true) {
                    await Task.Delay(waitInterval);
                    _checkTimes += waitInterval;

                    if (session != null && session.IsConnected) {
                        return true;
                    }
                    else {
                        if (_checkTimes >= maxWaitInterval) {
                            return false;
                        }
                    }
                }
            });
            return _task;
        }



        public void SendMessage (byte[] message) {
            if (session != null) {
                session.SendMessage(message);
            }
            else {
                KCPNetUtils.Logger.LogWarning("Client session is null.");
            }
        }

        /// <summary>
        /// 关闭 客户端
        /// Close client.
        /// </summary>
        public void Close () {
            if (session != null) {
                session.Close();
            }
        }


        private void OnSessionClose (uint sessionId) {
            cts.Cancel();
            if (udp != null) {
                udp.Close();
                udp = null;
            }

            KCPNetUtils.Logger.LogWarning("Client session closed.sid:" + sessionId);
        }


    }
}