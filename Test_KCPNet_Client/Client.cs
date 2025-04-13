// ------------------------------------
//
// FileName: Client.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/13
// Desc:        
//
// ------------------------------------
using KCPNet;
using Test_KCPNet_NetProtocol;

namespace Test_KCPNet_Client
{
    public class Client {

        private static KCPNetClient<ClientSession> client;

        private static Task<bool> checkTask = null;


        static void Main (string[] args) {
            var _ip = "127.0.0.1";
            client = new KCPNetClient<ClientSession>();
            client.Start(_ip, 12100);
            checkTask = client.ConnectToServer(200, 5000);
            Task.Run(CheckConnectState);


            while (true) {
                string _ipt = Console.ReadLine();
                if (_ipt == "quit") {
                    client.Close();
                    break;
                }
                else if (_ipt == "login") {
                    var _message = new NetProtocol.NetMessage();
                    _message.Header = new NetProtocol.NetMessageHeader {
                        Cmd = NetProtocol.Cmd.Login
                    };
                    _message.Body = new NetProtocol.NetMessageBody {
                        requestLogin = new NetProtocol.NetMessageRequestLogin {
                            Username = "test name",
                            Password = "test psd"
                        }
                    };

                    var _data = NetMessageProtocolUtils.Serialize(_message);

                    client.SendMessage(_data);
                }
                else if (_ipt == "bag") {
                    var _message = new NetProtocol.NetMessage();
                    _message.Header = new NetProtocol.NetMessageHeader {
                        Cmd = NetProtocol.Cmd.BagInfo
                    };

                    var _data = NetMessageProtocolUtils.Serialize(_message);
                    client.SendMessage(_data);
                }
            }

            Console.ReadKey();
        }


        private static int counter = 0;
        private static int ReconnectCount = 5;
        private static async void CheckConnectState () {
            while (true) {
                await Task.Delay(3000);
                if (checkTask == null || !checkTask.IsCompleted) {
                    continue;
                }

                if (checkTask.Result) {
                    KCPNetUtils.Logger.LogWithColor(KCPNetUtils.Logger.ELogColor.Green, "Connect server success.");
                    checkTask = null;
                }
                else {
                    ++counter;
                    if (counter >= ReconnectCount) {
                        KCPNetUtils.Logger.LogError(string.Format("Connect failed {0} times, Please check your Network State.", counter));
                        checkTask = null;
                        break;
                    }
                    else {
                        KCPNetUtils.Logger.LogWarning(string.Format("Connect faild {0} times. Retry...", counter));
                        checkTask = client.ConnectToServer(200, 5000);
                    }
                }
            }
        }

    }


}

