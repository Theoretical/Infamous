using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace MatchServer.Core
{
    class Client
    {
        private Socket mSocket = null;
        private UInt64 mClientUID = 0;
        private string mClientIP = String.Empty;

        public Client(Socket s, UInt64 p)
        {
            string ip = ((System.Net.IPEndPoint)s.RemoteEndPoint).Address.ToString();
            Log.Write("Client connected: {0}", ip);
        }
    }
}

