// ------------------------------------
//
// FileName: ServerSession.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/13
// Desc:        
//
// ------------------------------------
using KCPNet;
using NetProtocol;
using Test_KCPNet_NetProtocol;

namespace Test_KCPNet_Server
{
    public class ServerSession : KCPNet.KCPNetSession {
        protected override void OnConnected () {
            KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Green, "[Server] Client online, sid:{0}", sessionId);
        }

        protected override void OnDisconnected () {
            KCPNetUtils.Logger.LogWarningFormat("[Server] Client offline, sid:{0}", sessionId);

        }

        protected override void OnReceiveMessage (byte[] buffer) {
            var _netMessage = NetMessageProtocolUtils.Deserialize<NetMessage>(buffer);

            if (_netMessage == null) {
                return;
            }

            if (_netMessage.Header.Cmd == Cmd.NetPing) {
                ProcessNetPing(_netMessage);
            }
            else {
                switch (_netMessage.Header.Cmd) {
                    case Cmd.Login:
                        ProecssLogin(_netMessage);
                        break;
                    case Cmd.BagInfo:
                        ProecssBagInfo(_netMessage);
                        break;
                    default:
                        break;
                }
            }

            
        }

        private int checkCounter;
        DateTime checkTime = DateTime.UtcNow.AddSeconds(3);

        protected override void OnUpdate (DateTime utcTime) {
            //检测心跳状态
            //check netping state
            if (utcTime > checkTime) {
                checkTime = utcTime.AddSeconds(3);
                checkCounter++;
                if (checkCounter > 3) {
                    // 3 次心跳未完成后，结束 client 的连接
                    //If the 3 netping are not completed, the client connection is terminated.

                    KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Magenta, "[Server] Client connection timed out, close the current session. sid: {0}", sessionId);

                    SendNetPingMessage(true);
                    Close();
                }
            }
        }

        #region process client request
        private void ProcessNetPing (NetMessage netMessage) {
            KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Gray, "[Server] Receive client netping, sid:{0}, is_over:{1}", sessionId, netMessage.Body.netPing.isOver);
            if (netMessage.Body.netPing.isOver) {
                Close();
            }
            else {
                //接收到 netping 请求，重置检查计数，并回复 netping 消息到客户端
                //Received netping request, reset check counter, and reply netping message to the client.
                checkCounter = 0;
                SendNetPingMessage(false);
            }
        }

        private void SendNetPingMessage (bool isOver) {
            var _pinMessage = new NetMessage();
            _pinMessage.Header = new NetMessageHeader {
                Cmd = Cmd.NetPing
            };
            _pinMessage.Body = new NetMessageBody {
                netPing = new NetMessagePing { isOver = isOver }
            };
            var _messageData = NetMessageProtocolUtils.Serialize(_pinMessage);
            SendMessage(_messageData);
        }


        private void ProecssLogin (NetMessage netMessage) {
            var _message = netMessage.Body.requestLogin;
            KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Gray, "Server receive, sid:{0}, req login, name: {1}, psd: {2}", sessionId, _message.Username, _message.Password);

            var _loginMessage = new NetMessage();

            _loginMessage.Header = new NetMessageHeader {
                Cmd = Cmd.Login
            };
            _loginMessage.Body = new NetMessageBody {
                responseLogin = new NetMessageResponseLogin {
                    Success = true,
                    Message = "login ok"
                }
            };
            var _messageData = NetMessageProtocolUtils.Serialize(_loginMessage);
            SendMessage(_messageData);

        }

        private void ProecssBagInfo (NetMessage netMessage) {
            var _bagInfoMessage = new NetMessage();
            _bagInfoMessage.Header = new NetMessageHeader {
                Cmd = Cmd.BagInfo
            };
            _bagInfoMessage.Body = new NetMessageBody {
                responseBagInfo = new NetMessageResponseBagInfo {
                    Items = new List<NetMessageBagInfoItem> {
                         new NetMessageBagInfoItem{ Id = 1, Name = "name_1", Quantity = 1 },
                         new NetMessageBagInfoItem{ Id = 2, Name = "name_2", Quantity = 2 },
                         new NetMessageBagInfoItem{ Id = 3, Name = "name_3", Quantity = 3 },
                         new NetMessageBagInfoItem{ Id = 4, Name = "name_4", Quantity = 4 },
                     }
                }
            };
            var _messageData = NetMessageProtocolUtils.Serialize(_bagInfoMessage);
            SendMessage(_messageData);
        }
        #endregion

    }
}

