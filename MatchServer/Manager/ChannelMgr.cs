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
            pPacket.Write(pChannel.nChannelRule.ToString().ToLower());
            pClient.Send(pPacket);

            foreach (Client c in pChannel.lClients)
            {
                PlayerList(c);
            }


        }
        public static void Leave(Client pClient)
        {
            if (pClient.mChannel == null)
                return;

            lock (pClient.mChannel.lClients)
                pClient.mChannel.lClients.Remove(pClient);

            PacketWriter pChannelLeave = new PacketWriter(Operation.ChannelLeave, CryptFlags.Encrypt);
            pChannelLeave.Write(pClient.mClientUID);
            pChannelLeave.Write(pClient.mChannel.uidChannel);
            pClient.Send(pChannelLeave);

            foreach (Client c in pClient.mChannel.lClients)
                PlayerList(c);

            if (pClient.mChannel.lClients.Count == 0 && (pClient.mChannel.nChannelType == MMatchChannelType.Private || pClient.mChannel.nChannelType == MMatchChannelType.Clan))
                lock (Program.mChannels)
                    Program.mChannels.Remove(pClient.mChannel);

            pClient.mChannel = null;
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
        public static void Chat (Client pClient, string pMessage)
        {
            PacketWriter pChannelChat = new PacketWriter(Operation.ChannelChat, CryptFlags.Encrypt);
            
            pChannelChat.Write(pClient.mChannel.uidChannel);
            pChannelChat.Write(pClient.mCharacter.szName);
            pChannelChat.Write(pMessage);
            pChannelChat.Write((Int32)pClient.mAccount.nUGradeID);
            foreach (Client c in pClient.mChannel.lClients)
                c.Send(pChannelChat);
        }
    }
}
