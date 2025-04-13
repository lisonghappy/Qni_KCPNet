# Qni_KCPNet
基于 KCP 的可靠 UDP 通信网络库，可应用于 Unity 游戏服务器和客户端开发。并且实现使用 Protobuf 进行网络通信。

[[英文文档]](./README.md)

## Hot to use

### Unity
具体实现请查看测试工程。 `Test_UnityProject`

   ```cs
   // unity client
    public class KCPNetTest : MonoBehaviour {
        KCPNetClient<UnitySession> client;
        void Start () {
            string ip = "127.0.0.1";
            client = new KCPNetClient<UnitySession>();
            client.Start(ip, 12100);
            client.ConnectToServer(200, 500);
        }
        
        private void Update () {
            //do something
        }
         
    }


    //unity session
    public class UnitySession : KCPNetSession {}
   ```



### Client (.net Core)

具体实现请查看测试工程。 `Test_KCPNet_Client`


### Server (.net Core)
具体实现请查看测试工程。 `Test_KCPNet_Server`

```cs
    //server
    public class Server {

        static void Main (string[] args) {
            var _ip = "127.0.0.1";

            KCPNetServer<ServerSession> server = new KCPNetServer<ServerSession>();
            server.Start(_ip, 12100);

            while (true) {
                string _ipt = Console.ReadLine();
                if (_ipt == "quit") {
                    server.Close();
                    break;
                }
                else {
                    var _pinMessage = new NetMessage();
                    _pinMessage.Header = new NetMessageHeader {
                        Cmd = Cmd.None,
                        Message = _ipt
                    };
                    server.BroadcastMessage(NetMessageProtocolUtils.Serialize(_pinMessage));
                }
            }

            Console.ReadKey();
        }

    }

    //server session
     public class ServerSession : KCPNet.KCPNetSession{ }
```