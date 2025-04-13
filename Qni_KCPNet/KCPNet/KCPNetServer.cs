// ------------------------------------
//
// FileName: KCPNetServer.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/12
// Desc:        
//
// ------------------------------------
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KCPNet
{
    public class KCPNetServer<T_Session> : KCPNet where T_Session : KCPNetSession, new() {

        private Dictionary<uint, T_Session> sessionDict;

        public KCPNetServer () : base() {
            sessionDict = new Dictionary<uint, T_Session>();
        }

        public void Start (string ip, int port) {
            var _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            udp = new UdpClient(_endPoint);


            // on Windows platform, udp present error.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }

            KCPNetUtils.Logger.LogWithColor(KCPNetUtils.Logger.ELogColor.Green, "Server Start...");

            Task.Run(ReceiveAsync, ct);

        }

        public void Close () {
            foreach (var item in sessionDict) {
                item.Value.Close();
            }
            sessionDict = null;

            if (udp != null) {
                udp.Close();
                udp = null;
                cts.Cancel();
            }
        }


        public void BroadcastMessage (byte[] message) {
            if (message == null) {
                KCPNetUtils.Logger.LogWarning("Server send message is null.");
                return;
            }

            foreach (var item in sessionDict) {
                item.Value.SendMessage(message);
            }
        }




        private async void ReceiveAsync () {
            UdpReceiveResult _result;
            while (true) {
                try {
                    if (ct.IsCancellationRequested) {
                        KCPNetUtils.Logger.LogWithColor(KCPNetUtils.Logger.ELogColor.Cyan, "Sever recive task is cancelled.");
                        break;
                    }
                    _result = await udp.ReceiveAsync();
                    uint _sid = BitConverter.ToUInt32(_result.Buffer, 0);

                    //client first connect
                    if (_sid == 0) {
                        _sid = GenerateUniqueSessionID();
                        byte[] _convBytes = new byte[KCPNetConfig.NET_PACKAGE_HEADER_SIZE * 2];
                        byte[] _sidBytes = BitConverter.GetBytes(_sid);
                        Array.Copy(_sidBytes, 0, _convBytes, KCPNetConfig.NET_PACKAGE_HEADER_SIZE, KCPNetConfig.NET_PACKAGE_HEADER_SIZE);

                        SendMessageByUDPAsync(_convBytes, _result.RemoteEndPoint);
                    }
                    else {
                        //client send message
                        if (!sessionDict.TryGetValue(_sid, out T_Session _session)) {
                            _session = new T_Session();
                            _session.Init(_sid, SendMessageByUDPAsync, _result.RemoteEndPoint);
                            _session.OnSessionCloseCallback = OnSessionClose;
                            lock (sessionDict) {
                                sessionDict.Add(_sid, _session);
                            }
                        }
                        else {
                            _session = sessionDict[_sid];
                        }
                        _session.ReceiveMessage(_result.Buffer);
                    }
                }
                catch (Exception e) {
                    KCPNetUtils.Logger.LogWarningFormat("Server UDP recive data exception:{0}", e.ToString());
                }
            }
        }

        private void OnSessionClose (uint sid) {
            if (sessionDict.ContainsKey(sid)) {
                lock (sessionDict) {
                    sessionDict.Remove(sid);
                    KCPNetUtils.Logger.LogWarningFormat("Session:{0} remove from session dict.", sid);
                }
            }
            else {
                KCPNetUtils.Logger.LogErrorFormat("Session:{0} cannot find in session dict", sid);
            }
        }

        private uint sid = 0;
        private uint GenerateUniqueSessionID () {
            lock (sessionDict) {
                while (true) {
                    ++sid;
                    if (sid == uint.MaxValue) {
                        sid = 1;
                    }
                    if (!sessionDict.ContainsKey(sid)) {
                        break;
                    }
                }
            }
            return sid;
        }
    }
}

