using System;
using System.Threading;
using KCPNet;
using NetProtocol;
using Test_KCPNet_NetProtocol;
using UnityEngine;

public class KCPNetTest : MonoBehaviour {
    KCPNetClient<UnitySession> client;
    void Start () {
        Qni.QniLogger.Init( Qni.ELogChannel.Unity);

        KCPNetUtils.Logger.LogFunc = Debug.Log;
        KCPNetUtils.Logger.LogWarningFunc = Debug.LogWarning;
        KCPNetUtils.Logger.LogErrorFunc = Debug.LogError;
        KCPNetUtils.Logger.LogWithColorFunc = (color,msg) => {
            Qni.QniLogger.LogColor((Qni.ELogColor)color,msg);
        };


        string ip = "127.0.0.1";
        client = new KCPNetClient<UnitySession>();
        client.Start(ip, 12100);
        client.ConnectToServer(200, 500);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            client.Close();
        }
        if (Input.GetKeyDown(KeyCode.L)) {

            var _message = new NetProtocol.NetMessage();
            _message.Header = new NetProtocol.NetMessageHeader {
                Cmd = NetProtocol.Cmd.Login
            };
            _message.Body = new NetProtocol.NetMessageBody {
                requestLogin = new NetProtocol.NetMessageRequestLogin {
                    Username = "unity test name",
                    Password = "unity test psd"
                }
            };

            var _data = NetMessageProtocolUtils.Serialize(_message);

            client.SendMessage(_data);

        }
        else if (Input.GetKeyDown(KeyCode.B)) {
            var _message = new NetProtocol.NetMessage();
            _message.Header = new NetProtocol.NetMessageHeader {
                Cmd = NetProtocol.Cmd.BagInfo
            };

            var _data = NetMessageProtocolUtils.Serialize(_message);
            client.SendMessage(_data);

        }
    }
}



public class UnitySession : KCPNetSession {
    protected override void OnConnected () {
        Debug.LogWarning("Thread:" + Thread.CurrentThread.ManagedThreadId + "Client connected. sid: "+ sessionId);
    }


    protected override void OnDisconnected () {
        Debug.LogWarning("Thread:" + Thread.CurrentThread.ManagedThreadId + "Client disconnected. sid: " + sessionId);

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
                SendNetPingMessage(false);
            }
        }
    }


    #region process netmessage


    private void ProcessMessage (NetMessage netMessage) {
        KCPNetUtils.Logger.LogWithColorFormat(KCPNetUtils.Logger.ELogColor.Magenta, "Client receive server message: {0}", netMessage.Header.Message);
    }

    private void ProecssNetPing (NetMessage netMessage) {
        if (netMessage.Body.netPing.isOver) {
            Close();
        }
        else {
            checkCounter = 0;
            int delay = (int)DateTime.UtcNow.Subtract(sendTime).TotalMilliseconds;
            KCPNetUtils.Logger.LogFormat(string.Format("Client NetPing delay:{0} ms", delay));
        }
    }

    private void ProecssBagInfo (NetMessage netMessage) {
        var _bagInfo = netMessage.Body.responseBagInfo;

        if (_bagInfo != null) {
            foreach (var item in _bagInfo.Items) {
                KCPNetUtils.Logger.LogFormat("Client receive server [bagInfo] rsp, Id: {0}, Name: {1} Quantity: {2}", item.Id, item.Name, item.Quantity);
            }
        }
    }

    private void ProecssLogin (NetMessage netMessage) {
        var _loginInfo = netMessage.Body.responseLogin;
        if (_loginInfo != null) {
            KCPNetUtils.Logger.LogFormat("Client receive server [login] rsp, Success: {0}, Message: {1}", _loginInfo.Success, _loginInfo.Message);
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

    #endregion

}