using System;

using MatchServer.Core;
using MatchServer.Packet;
namespace MatchServer.Manager
{
    class ChannelMgr
    {
        public static void Join (Client pClient, MMatchChannel pChannel)
        {
            lock (pChannel.lClients)
                pChannel.lClients.Add(pClient);

            pClient.mPlace = MMatchPlace.Lobby;
            pClient.mChannel = pChannel;

            PacketWriter pPacket = new PacketWriter(Operation.ChannelResponseJoin, CryptFlags.Encrypt);
            pPacket.Write(pChannel.uidChannel);
            pPacket.Write((Int32)pChannel.nChannelType);
            pPacket.Write(pChannel.szName);
            pPacket.Write(true);
            pClient.Send(pPacket);

            pPacket = new PacketWriter(Operation.MatchResponseRuleset, CryptFlags.Encrypt);
            pPacket.Write(pChannel.uidChannel);
            pPacket.Write(pChannel.nChannelRule.ToString());
            pClient.Send(pPacket);
        }
    }
}
