using System;   

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
    }
}
