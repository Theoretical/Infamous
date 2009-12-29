using System;
using System.Collections.Generic;

using MatchServer.Core;
using MatchServer.Packet;
using MatchServer.Manager;
namespace MatchServer.Packet.Handle
{
    class Channel
    {
        [PacketHandler(Operation.ChannelJoin, PacketFlags.Character)]
        public static void ResponseChannelJoin (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidChan = pPacket.ReadUInt64();

            if (uidChar != pClient.mClientUID)
            {
                pClient.Disconnect();
                return;
            }

            MMatchChannel channel = Program.mChannels.Find(c => c.uidChannel == uidChan);
            if (channel != null)
                ChannelMgr.Join(pClient, channel);
        }

        [PacketHandler(Operation.ChannelRequestPlayerList, PacketFlags.Character)]
        public static void ResponsePlayerList (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidChan = pPacket.ReadUInt64();
            var page = pPacket.ReadInt32();

            if (uidChar != pClient.mClientUID || uidChan != pClient.mChannel.uidChannel)
            {
                pClient.Disconnect();
                return;
            }

            pClient.mChannelPage = page;
            ChannelMgr.PlayerList(pClient);
        }

        [PacketHandler(Operation.ChannelRequestChat, PacketFlags.Character)]
        public static void ResponseChannelChat (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidChan = pPacket.ReadUInt64();
            var message = pPacket.ReadString();

            if (uidChar != pClient.mClientUID || uidChan != pClient.mChannel.uidChannel || message.Length > 127)
            {
                pClient.Disconnect();
                return;
            }

            ChannelMgr.Chat(pClient, message);
        }

        [PacketHandler(Operation.ChannelListStart, PacketFlags.Character)]
        public static void ResponseChannelList (Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var type = pPacket.ReadInt32();

            if (!Enum.IsDefined(typeof(MMatchChannelType), (byte)type))
            {
                pClient.Disconnect();
                return;
            }

            List<MMatchChannel> channels = Program.mChannels.FindAll (c => c.nChannelType == (MMatchChannelType)type);
            if (channels == null || channels.Count == 0)
                return;

            PacketWriter pChannelList = new PacketWriter(Operation.ChannelList, CryptFlags.Encrypt);
            pChannelList.Write(channels.Count, 88);
            Int16 index = 0;
            
            foreach (MMatchChannel c in channels)
            {
                pChannelList.Write(c.uidChannel);
                pChannelList.Write(++index);
                pChannelList.Write((Int16)c.lClients.Count);
                pChannelList.Write((Int16)c.nMaxUsers);
                pChannelList.Write((Int16)c.nMinLevel);
                pChannelList.Write((Int16)c.nMaxLevel);
                pChannelList.Write((byte)c.nChannelType);
                pChannelList.Write(c.szName, 64);
                pChannelList.Write(false);
                pChannelList.Write((Int32)0);
            }
            pClient.Send(pChannelList);
        }

        [PacketHandler(Operation.ChannelRequestJoinFromName, PacketFlags.Character)]
        public static void ResponseChannelJoinFromName (Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var type = pPacket.ReadInt32();
            var name = pPacket.ReadString();

            if (!Enum.IsDefined(typeof(MMatchChannelType), (byte)type))
            {
                pClient.Disconnect();
                return;
            }

            MMatchChannel channel = Program.mChannels.Find(c => c.nChannelType == (MMatchChannelType)type && c.szName.Equals(name));
            if (channel == null)
            {
                channel = new MMatchChannel();
                channel.szName = name;
                channel.lClients = new List<Client>();
                channel.nChannelRule = MMatchChannelRule.Elite;
                channel.nChannelType = (MMatchChannelType)type;
                channel.nMaxLevel = 100;
                channel.nMinLevel = 0;
                channel.nMaxUsers = 200;
                channel.uidChannel = (UInt64)Program.mChannels.Count;
                lock (Program.mChannels)
                    Program.mChannels.Add(channel);
            }
            ChannelMgr.Join(pClient, channel);
        }
    }
}
