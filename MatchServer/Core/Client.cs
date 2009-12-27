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
        private byte[] mStream = new byte[0];
        private byte[] mBuffer = new byte[4096];

        private void HandleReceive(IAsyncResult pResult)
        {
            var nTotalRecv = 0;

            try
            {
                nTotalRecv = mSocket.EndReceive(pResult);
            }
            catch
            {
                //TODO: Handle Client disconnections / object not initialized here
                Log.Write("[{0}] Client Disconnected.", mClientIP);
                return;
            }

            if (nTotalRecv < 1)
            {
                //TODO: Handle client disconncetions here...
                Log.Write("[{0}] Client Disconnected.", mClientIP);
                return;
            }

            var bTemp = new byte[mStream.Length + nTotalRecv];
            Buffer.BlockCopy(mBuffer, 0, bTemp, mStream.Length, nTotalRecv);
            if (mStream.Length > 0)
                Buffer.BlockCopy(bTemp, 0, mStream, 0, mStream.Length);

            mStream = bTemp;

            try
            {
                Array.Clear(mBuffer, 0, 4096);
                mSocket.BeginReceive(mBuffer, 0, 4096, SocketFlags.None, new AsyncCallback(HandleReceive), null
            }
        }
        public Client(Socket s, UInt64 p)
        {
            string ip = ((System.Net.IPEndPoint)s.RemoteEndPoint).Address.ToString();
            Log.Write("Client connected: {0}", ip);

            mSocket.BeginReceive(mBuffer, 0, 4096, SocketFlags.None, new AsyncCallback(HandleReceive), null
        }
    }
}

