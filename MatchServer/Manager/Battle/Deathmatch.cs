using System;
using System.Collections.Generic;

using MatchServer.Core;
using MatchServer.Packet;
namespace MatchServer.Manager.Battle
{
    class Deathmatch : IGame
    {
        public void GameStartCallback(Client client)
        {
            MMatchStage stage = client.mStage;
            stage.nStageState = MMatachStageState.Standby;
            stage.nRoundState = MMatchRoundState.Prepare;
            client.mPlace = MMatchPlace.Battle;
            stage.nRedTeamScore = 0;
            stage.nBlueTeamScore = 0;

            PacketWriter pStart = new PacketWriter(Operation.StageLaunch, CryptFlags.Encrypt);
            pStart.Write(stage.uidStage);
            pStart.Write(stage.szMap);

            foreach (Client c in stage.Clients)
                c.Send(pStart);
        }
        public void GameLoadedCallback(Client client) 
        {
            client.mGame.Loaded = true;
            PacketWriter pPacket = new PacketWriter(Operation.LoadingComplete, CryptFlags.Encrypt);
            pPacket.Write(client.mClientUID);
            pPacket.Write((UInt32)100);

            foreach (Client c in client.mStage.Clients)
                c.Send(pPacket);
            ProcessBattleState(client);
        }
        public void GameEnterCalback(Client client) 
        {
            PacketWriter packet = new PacketWriter(Operation.StageEnterBattle, CryptFlags.Encrypt);

            if (client.mStage.nStageState == MMatachStageState.Standby)
                packet.Write((byte)0);
            else
                packet.Write((byte)1);

            packet.Write(1, 166);
            packet.Write(client.mClientUID);

            if (client.mPeerEnd == null)
            {
                packet.Write(new byte[4], 0, 4);
                packet.Write((Int32)0);
            }
            else
            {
                packet.Write(client.mPeerEnd.Address.GetAddressBytes(), 0, 4);
                packet.Write(client.mPeerEnd.Port);
            }

            packet.Write(client.mCharacter.szName, 32);
            packet.Write(client.mCharacter.szClanName, 16);
            packet.Write((Int32)0);//clan rank
            packet.Write((Int16)0);//clan points
            packet.Write((byte)0);//?
            packet.Write((Int16)client.mCharacter.nLevel);
            packet.Write((byte)client.mCharacter.nSex);
            packet.Write((byte)client.mCharacter.nHair);
            packet.Write((byte)client.mCharacter.nFace);
            packet.Write(client.mCharacter.nXP);
            packet.Write(client.mCharacter.nBP);
            packet.Write(client.mCharacter.fBonusRate);
            packet.Write(client.mCharacter.nPrize);
            packet.Write(client.mCharacter.nHP);
            packet.Write(client.mCharacter.nAP);
            packet.Write(client.mCharacter.nMaxWeight);
            packet.Write(client.mCharacter.nSafeFalls);
            packet.Write(client.mCharacter.nFR);
            packet.Write(client.mCharacter.nCR);
            packet.Write(client.mCharacter.nER);
            packet.Write(client.mCharacter.nWR);
            foreach (Item nItem in client.mCharacter.nEquippedItems)
                packet.Write(nItem.nItemID);
            packet.Write((Int32)client.mAccount.nUGradeID);
            packet.Write((Int32)0);//clan id
            packet.Write((byte)client.mTeam);
            packet.Write((byte)0);
            packet.Write((Int16)0);
            foreach (Client c in client.mStage.Clients)
            {
                c.Send(packet);
            }

            client.mGame.InGame = true;
            packet = new PacketWriter(Operation.StageRoundState, CryptFlags.Encrypt);
            packet.Write(client.mStage.uidStage);
            packet.Write((Int32)client.mStage.nRounds);
            packet.Write((Int32)client.mStage.nRoundState);
            packet.Write((Int32)0);
            client.Send(packet);

            ProcessBattleState(client);
        }
        public void GameJoinCallback(Client client) { }
        public void GameInfoCallback(Client client) 
        {
            client.mGame.RequestedInfo = true;
            ProcessBattleState(client);
        }
        public void GameExitCallback(Client client) { }
        public void GameKillCallback(Client client, Client killer) { }
        public void GameSuicideCallback(Client client) { }

        private void ProcessBattleState (Client client)
        {
            MMatchStage stage = client.mStage;

            if (stage.nStageState == MMatachStageState.Standby)
            {
                if (stage.nRoundState == MMatchRoundState.Prepare && stage.Clients.FindAll(c => c.mGame.InGame).TrueForAll(c => (c.mGame.EnteredGame == true && c.mGame.RequestedInfo)))
                {
                    stage.nRoundState = MMatchRoundState.Play;
                    stage.nStageState = MMatachStageState.Battle;
                }
                Log.Write("Wat");
                PacketWriter p = new PacketWriter(Operation.StageRoundState, CryptFlags.Encrypt);
                p.Write(stage.uidStage);
                p.Write((Int32)stage.nRounds);
                p.Write((Int32)stage.nRoundState);
                p.Write((Int32)0);
                foreach (Client c in stage.Clients)
                {
                    c.mGame.RequestedInfo = false;
                    c.mGame.Spawned = true;
                    c.Send(p);
                }
            }
        }

    }
}
