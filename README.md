# Qni_KCPNet
A reliable UDP communication network library based on KCP, which can be applied to Unity game server and client development. And implement network communication using Protobuf.

[English] | [[中文]](./README_CN.md)

## Hot to use

### Unity
Please refer to the test project for the specific implementation. [Test_UnityProject](./Test_UnityProject)

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

Please refer to the test project for the specific implementation. [Test_KCPNet_Client](./Test_KCPNet_Client)


### Server (.net Core)
Please refer to the test project for the specific implementation. [Test_KCPNet_Server](./Test_KCPNet_Server)

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