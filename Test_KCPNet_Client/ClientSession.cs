// ------------------------------------
//
// FileName: ClientSession.cs
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

namespace Test_KCPNet_Client
{

    public class ClientSession : KCPNet.KCPNetSession {
        protected override void OnConnected () {
            KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Green, "[Client] connect to server. sid:{0}", sessionId);
        }

        protected override void OnDisconnected () {
            KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Green, "[Client] disconnected. sid:{0}", sessionId);
        }

        protected override void OnReceiveMessage (byte[] buffer) {
            var _netMessage = NetMessageProtocolUtils.Deserialize<NetMessage>(buffer);
            if (_netMessage == null) {
                return;
            }

            switch (_netMessage.Header.Cmd) {
                case Cmd.None:
                    ProcessMessage(_netMessage);
                    break;
                case NetProtocol.Cmd.NetPing:
                    ProecssNetPing(_netMessage);
                    break;
                case NetProtocol.Cmd.Login:
                    ProecssLogin(_netMessage);
                    break;
                case NetProtocol.Cmd.BagInfo:
                    ProecssBagInfo(_netMessage);
                    break;
                default:
                    break;
            }

        }

        private DateTime sendTime;
        private int checkCounter;
        private DateTime checkTime = DateTime.UtcNow.AddSeconds(3);
        protected override void OnUpdate (DateTime utcTime) {
            if (utcTime > checkTime) {
                sendTime = utcTime;
                checkTime = utcTime.AddSeconds(3);
                checkCounter++;
               

                if (checkCounter > 3) {
                    Close();
                }
                else {
                    var _pinMessage = new NetMessage {
                        Header = new NetMessageHeader {
                            Cmd = Cmd.NetPing
                        },
                            Body = new NetMessageBody {
                            netPing = new NetMessagePing { isOver = false }
                        }
                    };
                    var _messageData = NetMessageProtocolUtils.Serialize(_pinMessage);
                    SendMessage(_messageData);
                }
            }
        }


        #region process netmessage


        private void ProcessMessage (NetMessage netMessage) {
            KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Magenta, "[Client] receive server message: {0}", netMessage.Header.Message);
        }

        private void ProecssNetPing (NetMessage netMessage) {
            if (netMessage.Body.netPing.isOver) {
                Close();
            }
            else {
                checkCounter = 0;
                int _delay = (int)DateTime.UtcNow.Subtract(sendTime).TotalMilliseconds;
                KCPNetUtils.Logger.LogFormat(string.Format("[Client] NetPing delay:{0} ms", _delay));
            }
        }

        private void ProecssBagInfo (NetMessage netMessage) {
            var _bagInfo = netMessage.Body.responseBagInfo;

            if (_bagInfo != null) {
                foreach (var item in _bagInfo.Items) {
                    KCPNetUtils.Logger.LogFormat("[Client] receive server [bagInfo] rsp, Id: {0}, Name: {1} Quantity: {2}", item.Id, item.Name, item.Quantity);
                }
            }
        }

        private void ProecssLogin (NetMessage netMessage) {
            var _loginInfo = netMessage.Body.responseLogin;
            if (_loginInfo != null) {
                KCPNetUtils.Logger.LogFormat("[Client] receive server [login] rsp, Success: {0}, Message: {1}", _loginInfo.Success, _loginInfo.Message);
            }
        }

        #endregion
    }
}

