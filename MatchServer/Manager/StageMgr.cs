using System;
using System.Collections.Generic;

using MatchServer.Core;
using MatchServer.Packet;
namespace MatchServer.Manager
{
    class StageMgr
    {
        public static void Join (Client pClient, MMatchStage pStage)
        {
            if (pStage.Clients.Count < pStage.nMaxPlayers)
            {
                pClient.mPlace = MMatchPlace.Stage;
                pClient.mStage = pStage;
                pClient.mClientFlags = PacketFlags.Stage;

                lock (pStage.Clients)
                    pStage.Clients.Add(pClient);
    
                PacketWriter pPacket = new PacketWriter(Operation.StageJoin, CryptFlags.Encrypt);
                pPacket.Write(pClient.mClientUID);
                pPacket.Write(pStage.uidStage);
                pPacket.Write(Convert.ToInt32(pClient.mChannel.lStages.IndexOf(pStage) + 1));
                pPacket.Write(pStage.szName);

                foreach (Client c in pStage.Clients)
                    c.Send(pPacket);
            
                pPacket = new PacketWriter(Operation.MatchObjectCache, CryptFlags.Encrypt);
                pPacket.Write((byte)ObjectCache.Expire);
                pPacket.Write(pStage.Clients.Count, 152);
                foreach (Client c in pStage.Clients)
                {
                    pPacket.Write((Int32)0);
                    pPacket.Write(c.mClientUID);
                    pPacket.Write(c.mCharacter.szName, 32);
                    pPacket.Write(c.mCharacter.szClanName, 16);
                    pPacket.Write((Int32)c.mCharacter.nLevel);
                    pPacket.Write((Int32)c.mAccount.nUGradeID);
                    pPacket.Write((Int32)0);
                    pPacket.Write((Int32)0);
                    pPacket.Write((Int32)0);//CLID
                    pPacket.Write((Int32)0);//Emblem
                    pPacket.Write((Int32)c.mCharacter.nSex);
                    pPacket.Write((byte)c.mCharacter.nHair);
                    pPacket.Write((byte)c.mCharacter.nFace);
                    pPacket.Write((Int16)0);
                    foreach (Item i in c.mCharacter.nEquippedItems)
                        pPacket.Write(i.nItemID);
                    pPacket.Write((Int32)1);
                    pPacket.Write((Int32)5);
                    pPacket.Write((Int32)25);
                }
                pClient.Send(pPacket);
                UpdateStageCache(pStage, pClient, ObjectCache.Keep);
                UpdateMaster(pStage);
            }

            StageList(pClient);
        }
        public static void Leave (Client pClient)
        {
            if (pClient.mStage == null)
                return;
            
            MMatchStage stage = pClient.mStage;
            pClient.mStage = null;
            pClient.mClientFlags = PacketFlags.Character;
            pClient.mPlace = MMatchPlace.Lobby;

            PacketWriter pPacket = new PacketWriter(Operation.StageLeave, CryptFlags.Encrypt);
            pPacket.Write(pClient.mClientUID);
            foreach (Client c in stage.Clients)
                c.Send(pPacket);

            lock (stage)
            {
                stage.Clients.Remove(pClient);

                if (stage.Clients.Count == 0)
                {
                    lock (pClient.mChannel.lStages)
                        pClient.mChannel.lStages.Remove(stage);
                    StageList(pClient);
                    return;
                }
            }
            UpdateStageCache(stage, pClient, ObjectCache.New);
            stage.stageMaster = stage.Clients[0];
            UpdateMaster(stage);


        }
        public static void ForceLeave(Client pClient)
        {
            if (pClient.mStage == null)
                return;

            MMatchStage stage = pClient.mStage;
            pClient.mStage = null;
            pClient.mClientFlags = PacketFlags.Character;
            pClient.mPlace = MMatchPlace.Lobby;

            PacketWriter pPacket = new PacketWriter(Operation.StageLeave, CryptFlags.Encrypt);
            pPacket.Write(pClient.mClientUID);
            foreach (Client c in stage.Clients)
            {
                if (pClient == c) continue;
                c.Send(pPacket);
            }
            lock (stage)
            {
                stage.Clients.Remove(pClient);

                if (stage.Clients.Count == 0)
                {
                    lock (pClient.mChannel.lStages)
                        pClient.mChannel.lStages.Remove(stage);
                    StageList(pClient);
                    return;
                }
            }
            UpdateStageCache(stage, pClient, ObjectCache.New);
            stage.stageMaster = stage.Clients[0];
            UpdateMaster(stage);


        }
        public static void StageList (Client pClient)
        {
            List<MMatchStage> stages = null;
            MMatchChannel pChannel = pClient.mChannel;
            PacketWriter pPacket = new PacketWriter(Operation.StageList, CryptFlags.Encrypt);

            byte curr = Convert.ToByte(Math.Min(8, Math.Max(0, pChannel.lStages.Count - pClient.mStageIndex)));
            byte prev = Convert.ToByte(Math.Min(8, Math.Max(0, pChannel.lStages.Count - Math.Max(0, pClient.mStageIndex - 8))));
            byte next = Convert.ToByte(Math.Min(8, Math.Max(0, pChannel.lStages.Count - (pClient.mStageIndex + 8))));

            pPacket.Write(prev);
            pPacket.Write(next);
            pPacket.Write(curr, 0x5A);
            if (curr > 0) stages = pChannel.lStages.GetRange(pClient.mStageIndex, curr);

            if (stages != null)
            {
                byte index = pClient.mStageIndex;
                foreach (MMatchStage stage in stages)
                {
                    pPacket.Write(stage.uidStage);
                    pPacket.Write(++index);
                    pPacket.Write(stage.szName, 64);
                    pPacket.Write(Convert.ToByte(stage.Clients.Count));
                    pPacket.Write(stage.nMaxPlayers);
                    pPacket.Write((Int32)stage.nStageState);
                    pPacket.Write((Int32)stage.nGameType);
                    pPacket.Write((byte)0);
                    Int32 nSettings = 0;
                    if (stage.bForcedEntry) nSettings = 1;
                    if (stage.szPassword.Length > 0) nSettings += 2;
                    pPacket.Write(nSettings);
                    pPacket.Write(Convert.ToByte(stage.stageMaster.mCharacter.nLevel));
                    pPacket.Write(stage.nLevel);
                }
            }

            foreach (Client c in pChannel.lClients)
                c.Send(pPacket);
        }
        public static void Chat (Client pClient, string pMessage)
        {
            PacketWriter pStageChat = new PacketWriter(Operation.StageChat, CryptFlags.Encrypt);
            pStageChat.Write(pClient.mClientUID);
            pStageChat.Write(pClient.mStage.uidStage);
            pStageChat.Write(pMessage);

            foreach (Client c in pClient.mStage.Clients)
            {
                c.Send(pStageChat);
            }
        }
        public static void UpdateStageCache(MMatchStage stage, Client client, ObjectCache cache)
        {
            PacketWriter pPacket = new PacketWriter(Operation.MatchObjectCache, CryptFlags.Encrypt);
            pPacket.Write((byte)cache);
            pPacket.Write(1, 140);
            pPacket.Write((Int32)0);
            pPacket.Write(client.mClientUID);
            pPacket.Write(client.mCharacter.szName, 32);
            pPacket.Write(client.mCharacter.szClanName, 16);
            pPacket.Write(Convert.ToInt32(client.mCharacter.nLevel));
            pPacket.Write((Int32)client.mAccount.nUGradeID);
            pPacket.Write((Int32)0);
            pPacket.Write((Int32)0);
            pPacket.Write(client.mCharacter.nCLID);
            pPacket.Write((Int32)0);//Emblem
            pPacket.Write((Int32)client.mCharacter.nSex);
            pPacket.Write((byte)client.mCharacter.nHair);
            pPacket.Write((byte)client.mCharacter.nFace);
            pPacket.Write((Int16)0);
            foreach (Item i in client.mCharacter.nEquippedItems)
                pPacket.Write(i.nItemID);

            lock (stage.Clients)
            {
                foreach (Client c in stage.Clients)
                {
                    if (c == client) continue;
                    c.Send(pPacket);
                }
            }
        }
        public static void UpdateMaster (MMatchStage pStage)
        {
            PacketWriter pPacket = new PacketWriter(Operation.StageMaster, CryptFlags.Encrypt);
            pPacket.Write(pStage.uidStage);
            pPacket.Write(pStage.stageMaster.mClientUID);
            foreach (Client c in pStage.Clients)
            {
                c.Send(pPacket);
            }
        }
        public static void UpdateSetting (Client client)
        {
            if (client.mStage == null)
                return;
            MMatchStage stage = client.mStage;
            PacketWriter packet = new PacketWriter(Operation.StageResponseSettings, CryptFlags.Encrypt);
            packet.Write(stage.uidStage);
            packet.Write(1, 68);
            packet.Write(stage.uidStage);
            packet.Write(stage.szMap, 32);
            packet.Write((Int32)0);
            packet.Write((Int32)stage.nGameType);
            packet.Write((Int32)stage.nRounds);
            packet.Write((Int32)stage.nTime);
            packet.Write((Int32)stage.nLevel);
            packet.Write((Int32)stage.nMaxPlayers);
            packet.Write(stage.bTeamKill);
            packet.Write(stage.bTeamWinThePoint);
            packet.Write(stage.bForcedEntry);
            packet.Write(stage.bTeamBalance);

            packet.Write(stage.Clients.Count, 16);
            foreach (Client c in stage.Clients)
            {
                packet.Write(c.mClientUID);
                packet.Write((Int32)c.mTeam);
                packet.Write((Int32)c.mState);
            }
            packet.Write((Int32)stage.nStageState);
            packet.Write(client.mClientUID);
            foreach (Client c in stage.Clients)
            {
                c.Send(packet);
            }

            UpdateMaster(stage);
        }
        public static void UpdateState (Client client)
        {
            PacketWriter pPacket = new PacketWriter(Operation.StageState, CryptFlags.Encrypt);
            pPacket.Write(client.mClientUID);
            pPacket.Write(client.mStage.uidStage);
            pPacket.Write((Int32)client.mState);

            foreach (Client c in client.mStage.Clients)
                c.Send(pPacket);
        }
        public static void UpdateTeam (Client client)
        {
            PacketWriter pPacket = new PacketWriter(Operation.StageTeam, CryptFlags.Encrypt);
            pPacket.Write(client.mClientUID);
            pPacket.Write(client.mStage.uidStage);
            pPacket.Write((Int32)client.mTeam);

            foreach (Client c in client.mStage.Clients)
                c.Send(pPacket);
        }
    }
}
