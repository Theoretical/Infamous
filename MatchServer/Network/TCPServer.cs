using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using MatchServer.Core;
namespace MatchServer.Network
{
    class TCPServer
    {
        private static HashSet<Client> mClients = new HashSet<Client>();
        private static UInt64 mSessions = 0;
        private static Socket mListener;

        public static void Remove(Client pClient)
        {
            lock (mClients)
                mClients.Remove(pClient);
        }
        private static void HandleAccept(IAsyncResult pResult)
        {
            try
            {
                lock (mClients)
                    mClients.Add(new Client(mListener.EndAccept(pResult), ++mSessions));
            }
            catch (Exception e)
            {
                Log.Write("Error: {0}", e.Message);
            }
            mListener.BeginAccept(new AsyncCallback(HandleAccept), null);
        }

        public static bool Initialize()
        {
            try
            {
                mListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mListener.Bind(new IPEndPoint(IPAddress.Any, 6000));
                mListener.Listen(64);
                mSessions = 1;
                mClients = new HashSet<Client>();
                mListener.BeginAccept(new AsyncCallback(HandleAccept), null);
                Log.Write("TCP Server Iniitialized.");
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool IsRunning()
        {
            return mListener.IsBound;
        }
    }
}
