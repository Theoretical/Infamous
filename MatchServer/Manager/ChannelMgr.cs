using System;
using System.Collections.Generic;

using MatchServer.Core;
using MatchServer.Packet;
namespace MatchServer.Manager
{
    class ChannelMgr
    {
        public static void Join(Client pClient, MMatchChannel pChannel)
        {
            Leave(pClient);
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

            foreach (Client c in pChannel.lClients)
            {
                if (c == pClient) continue;
                PlayerList(c);
            }
        }
        public static void Leave(Client pClient)
        {
            if (pClient.mChannel == null)
                return;

            PacketWriter pChannelLeave = new PacketWriter(Operation.ChannelLeave, CryptFlags.Encrypt);
            pChannelLeave.Write(pClient.mClientUID);
            pChannelLeave.Write(pClient.mChannel.uidChannel);
            pClient.Send(pChannelLeave);
        }
        public static void PlayerList (Client pClient)
        {
            PacketWriter pResonsePlayerList = new PacketWriter(Operation.ChannelResponsePlayerList, CryptFlags.Encrypt);
            List<Client> clients;

            var pages = Convert.ToByte(pClient.mChannel.lClients.Count / 6);
            var page = Math.Min(pClient.mChannelPage, pages);
            var start = page * 6;
            var count = Math.Min(pClient.mChannel.lClients.Count - start, 6);

            pResonsePlayerList.Write((byte)pClient.mChannel.lClients.Count);
            pResonsePlayerList.Write((byte)page);
            pResonsePlayerList.Write(count, 71);
            clients = pClient.mChannel.lClients.GetRange(start, count);

            foreach (Client c in clients)
            {
                pResonsePlayerList.Write(c.mClientUID);
                pResonsePlayerList.Write(c.mCharacter.szName, 32);
                pResonsePlayerList.Write(c.mCharacter.szClanName, 16);
                pResonsePlayerList.Write((byte)c.mCharacter.nLevel);
                pResonsePlayerList.Write((Int32)c.mPlace);
                pResonsePlayerList.Write((byte)c.mAccount.nUGradeID);
                pResonsePlayerList.Write((byte)2);
                pResonsePlayerList.Write(c.mCharacter.nCLID);
                pResonsePlayerList.Write((Int32)0);
            }
            pClient.Send(pResonsePlayerList);
        }
    }
}
