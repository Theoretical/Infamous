using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

using MatchServer.Network;
using MatchServer.Packet;
namespace MatchServer.Core
{
    class Client
    {
             
        private Socket mSocket = null;
        public UInt64 mClientUID = 0;
        private string mClientIP = String.Empty;
        private byte[] mStream = new byte[0];
        private byte[] mBuffer = new byte[4096];
        private byte[] mCrypt = new byte[32];
        private byte mCounter = 0;
        private SocketAsyncEventArgs mArgs = new SocketAsyncEventArgs();
        private Queue<PacketReader> mPacketQueue = new Queue<PacketReader>();
        public PacketFlags mClientFlags = PacketFlags.None;
        
        public MMatchAccountInfo mAccount = new MMatchAccountInfo();
        public MTD_CharInfo mCharacter = new MTD_CharInfo();
        public MMatchChannel mChannel = null;
        public MMatchStage mStage = null;
        public MGame mGame = null;
        public MMatchPlace mPlace = MMatchPlace.Outside;


        public void Disconnect()
        {
            if (mSocket.Connected)
                mSocket.Close();

            TCPServer.Remove(this);
        }

        public void UnloadCharacter()
        {
            mClientFlags = PacketFlags.Login;
            mPlace = MMatchPlace.Outside;
            mCharacter = new MTD_CharInfo();
        }
        public void Send(PacketWriter pPacket)
        {
            var packet = pPacket.Process(++mCounter, mCrypt);
            Send(packet);

            PacketCrypt.Decrypt(packet, 6, packet.Length - 6, mCrypt);
            Log.PacketLog(packet, 0, packet.Length);
        }
        private void Send(byte[] pBuffer)
        {
            mArgs.Completed += HandleAsyncSend;
            mArgs.SetBuffer(pBuffer, 0, pBuffer.Length);
            mArgs.UserToken = this;
            mSocket.SendAsync(mArgs);
        }

        private void HandleAsyncSend(object pObject, SocketAsyncEventArgs pArgs)
        {
            pArgs.Completed -= HandleAsyncSend;
            Log.Write("[{0}] Sending Complete.", mClientIP);
        }

        private void HandleReceive(IAsyncResult pResult)
        {
            var nTotalRecv = 0;

            try
            {
                nTotalRecv = mSocket.EndReceive(pResult);
            }
            catch
            {
                Disconnect();
                Log.Write("[{0}] Client Disconnected.", mClientIP);
                return;
            }

            if (nTotalRecv < 1)
            {
                Disconnect();
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
                ProcessStream();
                ThreadPool.QueueUserWorkItem(ProcessQueue);
                Array.Clear(mBuffer, 0, 4096);
                mSocket.BeginReceive(mBuffer, 0, 4096, SocketFlags.None, new AsyncCallback(HandleReceive), null);
            }
            catch 
            {
                Log.Write("Error: Processing pack | Initializing Receieve.");
                Disconnect();
            }
        }

        private void ProcessStream()
        {
            var position = 0;
            var finished = false;

            while (!finished && position < mStream.Length)
            {
                if ((mStream.Length - position) >= 6)
                {
                    byte[] bPacket = new byte[6];
                    Buffer.BlockCopy(mStream, position, bPacket, 0, 6);

                    if ((CryptFlags)bPacket[0] == CryptFlags.Encrypt)
                    {
                        PacketCrypt.Decrypt(bPacket, 2, 2, mCrypt);
                        var size = BitConverter.ToUInt16(bPacket, 2);
                        if ((mStream.Length - position) >= size)
                        {
                            Array.Resize<byte>(ref bPacket, size);
                            Buffer.BlockCopy(mStream, position, bPacket, 0, size);
                            PacketCrypt.Decrypt(bPacket, 6, size - 6, mCrypt);
                            PacketReader pReader = new PacketReader(bPacket, size);
                            lock (mPacketQueue)
                                mPacketQueue.Enqueue(pReader);
                            position += size;
                        }
                        else finished = true;
                    }
                    else if ((CryptFlags)bPacket[0] == CryptFlags.Decrypt)
                    {
                        var size = BitConverter.ToUInt16(bPacket, 2);
                        if ((mStream.Length - position) >= size)
                        {
                            Array.Resize<byte>(ref bPacket, size);
                            Buffer.BlockCopy(mStream, position, bPacket, 0, size);
                            PacketReader pReader = new PacketReader(bPacket, size);
                            lock (mPacketQueue)
                                mPacketQueue.Enqueue(pReader);
                            position += size;
                        }
                        else finished = true;
                           
                    }
                }
                else finished = true;
            }

            var temp = new byte[mStream.Length - position];
            Buffer.BlockCopy(mStream, position, temp, 0, mStream.Length - position);
            mStream = temp;
        }
        private void ProcessQueue(object pObject)
        {
            while (mPacketQueue.Count > 0)
            {
                PacketReader pReader = null;
                HandlerDelegate handler = null;

                lock (mPacketQueue)
                    pReader = mPacketQueue.Dequeue();

                Log.Write("{0} Received: {1}", mClientIP, pReader.getOpcode());
                if (PacketMgr.mOpcodes.TryGetValue(pReader.getOpcode(), out handler))
                    if (mClientFlags >= handler.mFlags) 
                        handler.mProcessor(this, pReader);
            }
        }

        private void Handshake()
        {
            byte[] bHandshake = new byte[26];
            byte[][] bClientKeys = new byte[32][];

            bClientKeys[0] = new byte[] {
                0x37, 0x04, 0x5D, 0x2E, 0x43, 0x3A,
                0x49, 0x53, 0x50, 0x05, 0x13, 0xC9, 
                0x28, 0xA4, 0x4D, 0x05
            };

            bClientKeys[1] = new byte[] {
                0x57, 0x02, 0x5B, 0x04, 0x34, 0x06, 0x01, 
                0x08, 0x37, 0x0A, 0x12, 0x69, 0x41, 0x38,
                0x0F, 0x78
            };

            //Client IP Address
            Buffer.BlockCopy(((System.Net.IPEndPoint)mSocket.RemoteEndPoint).Address.GetAddressBytes(), 0, mCrypt, 0, 4);

            //Client Unknown
            Buffer.BlockCopy(BitConverter.GetBytes((UInt32)2), 0, mCrypt, 4, 4);

            //Client MUID
            Buffer.BlockCopy(BitConverter.GetBytes(mClientUID), 0, mCrypt, 8, 8);

            //Static Client Key Part 1.
            Buffer.BlockCopy(bClientKeys[0], 0, mCrypt, 16, 16);

            //Our actual packet
            Buffer.BlockCopy(BitConverter.GetBytes((Int16)10), 0, bHandshake, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((Int16)26), 0, bHandshake, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((Int16)20), 0, bHandshake, 6, 2);
            Buffer.BlockCopy(mCrypt, 4, bHandshake, 10, 12);
            Buffer.BlockCopy(mCrypt, 0, bHandshake, 22, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketCrypt.CalculateChecksum(bHandshake, 0, 26)), 0, bHandshake, 4, 2);

            for (int i = 0; i < 4; ++i)
            {
                uint a = BitConverter.ToUInt32(bClientKeys[1], i * 4);
                uint b = BitConverter.ToUInt32(mCrypt, i * 4);
                Buffer.BlockCopy(BitConverter.GetBytes(a ^ b), 0, mCrypt, i * 4, 4);
            }

            Send(bHandshake);
        }

        public Client(Socket pSocket, UInt64 pSession)
        {
            mClientIP = ((System.Net.IPEndPoint)pSocket.RemoteEndPoint).Address.ToString();   
            mSocket = pSocket;
            mClientUID = pSession;
            Log.Write("{0} Client connected", mClientIP);

            Handshake();
            mSocket.BeginReceive(mBuffer, 0, 4096, SocketFlags.None, new AsyncCallback(HandleReceive), null);
        }
    }
}

