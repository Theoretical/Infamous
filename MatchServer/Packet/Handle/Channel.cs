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
    }
}
