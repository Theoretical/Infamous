using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using MatchServer.Core;
using MatchServer.Packet;
namespace MatchServer.Network
{
    sealed class Pair<T1, T2>
    {
        public readonly T1 First;
        public readonly T2 Second;
        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }

    class UDPServer
    {
        private const int SIO_UDP_CONNRESET = -1744830452;
        private static Queue<Pair<IPEndPoint, PacketReader>> UDPReceiveQueue = new Queue<Pair<IPEndPoint, PacketReader>>();
        private static Socket mSocket = null;
        private static byte[] mUDPBuffer = new byte[4096];

        public static void Initialize ()
        {
            try
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                mSocket.Bind(new IPEndPoint(IPAddress.Any, 7777));
                mSocket.IOControl(SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                mSocket.BeginReceiveFrom(mUDPBuffer, 0, mUDPBuffer.Length, SocketFlags.None, ref ep, OnUDPRecv, null);
            }
            catch (Exception e)
            {
            }
        }


        private static void OnUDPRecv(IAsyncResult iResult)
        {
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            int nRecv = mSocket.EndReceiveFrom(iResult, ref ep);
            if (nRecv > 11)
            {
                UInt16 nTotal = BitConverter.ToUInt16(mUDPBuffer, 2);
                Operation operation = (Operation)BitConverter.ToUInt16(mUDPBuffer, 8);
                if (nRecv >= nTotal)
                {
                    lock (UDPReceiveQueue)
                    {
                        UDPReceiveQueue.Enqueue(new Pair<IPEndPoint, PacketReader>((IPEndPoint)ep, new PacketReader(mUDPBuffer, nTotal)));
                        ThreadPool.QueueUserWorkItem(ProcessUDP);
                    }
                }
            }
            ep = new IPEndPoint(IPAddress.Any, 0);
            mSocket.BeginReceiveFrom(mUDPBuffer, 0, mUDPBuffer.Length, SocketFlags.None, ref ep, OnUDPRecv, null);

        }
        private static void ProcessUDP(object o)
        {
            try
            {
                while (UDPReceiveQueue.Count > 0)
                {
                    IPEndPoint ep = null;
                    PacketReader packet = null;

                    lock (UDPReceiveQueue)
                    {
                        Pair<IPEndPoint, PacketReader> next = UDPReceiveQueue.Dequeue();
                        ep = next.First;
                        packet = next.Second;
                    }
                    if (packet.getOpcode() == Operation.BridgeRequest)
                    {
                        var uid = packet.ReadUInt64();
                        var ip = packet.ReadBytes(4);
                        var port = packet.ReadInt32();
                        Client client = TCPServer.mClients.Find(c => c.mClientUID == uid && (c.mPeerEnd == null || c.mPeerEnd.Port != port));
                        if (client == null) Log.Write("Invalid Client");
                        else
                        {

                            client.mPeerEnd = ep;

                            PacketWriter pPacket = new PacketWriter(Operation.BridgeResponse, CryptFlags.Decrypt);
                            pPacket.Write(client.mClientUID);
                            pPacket.Write((UInt32)Results.Accepted);
                            client.Send(pPacket);

                            Log.Write("[{0}] Briged client.", client.mClientIP);
                        }
                    }
                    else
                    {
                        //Log.Write("Recieved: {0}", packet.getOperation());
                    }
                }
            } catch {}
        }

    }


}
