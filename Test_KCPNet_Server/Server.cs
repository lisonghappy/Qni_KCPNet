// ------------------------------------
//
// FileName: Server.cs
//
// Author:   lisonghappy
// Email:    lisonghappy@gmail.com
// Date:     2025/4/13
// Desc:        
//
// ------------------------------------
using System;
using KCPNet;
using NetProtocol;
using Test_KCPNet_NetProtocol;

namespace Test_KCPNet_Server {

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
}

