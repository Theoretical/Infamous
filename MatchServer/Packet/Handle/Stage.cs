using System;
using System.Threading;
using System.Collections.Generic;

using MatchServer.Core;
using MatchServer.Packet;
using MatchServer.Manager;

namespace MatchServer.Packet.Handle
{
    class Stage
    {
        [PacketHandler(Operation.StageCreate, PacketFlags.Character)]
        public static void ProcessStageCreate (Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var name = pPacket.ReadString();
            var locked = pPacket.ReadBoolean();
            var password = pPacket.ReadString();

            if (uid != pClient.mClientUID)
            {
                pClient.Disconnect();
                return;
            }

            MMatchStage stage = new MMatchStage();
            stage.uidStage = Program.mStageCounter++;
            stage.szName = name;
            stage.stageMaster = pClient;
            stage.bPassword = locked;
            stage.szPassword = password;

            lock (pClient.mChannel.lStages)
                pClient.mChannel.lStages.Add(stage);

            pClient.mState = MMatchObjectStageState.Ready;
            StageMgr.Join(pClient, stage);
        }

        [PacketHandler(Operation.StageLeave, PacketFlags.Stage)]
        public static void ProcessStageLeave (Client pClient, PacketReader pPacket)
        {
            StageMgr.Leave(pClient);
        }

        [PacketHandler(Operation.StageListRequest, PacketFlags.Character)]
        public static void ProcessStageList (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidChan = pPacket.ReadUInt64();
            var page = pPacket.ReadInt32();

            pClient.mStageIndex = Convert.ToByte(page);
            StageMgr.StageList(pClient);
        }

        [PacketHandler(Operation.StageRequestJoin, PacketFlags.Character)]
        public static void ProcessStageJoin (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidStage = pPacket.ReadUInt64();

            MMatchStage stage = pClient.mChannel.lStages.Find(s => s.uidStage == uidStage);
            if (stage == null)
                return;
            StageMgr.Join(pClient, stage);
        }

        [PacketHandler(Operation.StageRequestSettings, PacketFlags.Stage)]
        public static void ProcessStageSettings (Client pClient, PacketReader pPacket)
        {
            StageMgr.UpdateSetting(pClient);
        }

        [PacketHandler(Operation.StageMap, PacketFlags.Stage)]
        public static void ProcessStageMap (Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var map = pPacket.ReadString();
            
            if (map.Length > 127 || pClient.mStage == null || pClient.mStage.stageMaster != pClient)
            {
                pClient.Disconnect();
                return;
            }
            pClient.mStage.szMap = map;
            StageMgr.UpdateSetting(pClient);
        }
        
        [PacketHandler(Operation.StageState, PacketFlags.Stage)]
        public static void ProcessStageState (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidStage = pPacket.ReadUInt64();
            var state = pPacket.ReadInt32();

            if (!Enum.IsDefined(typeof(MMatchObjectStageState), (byte)state) || pClient.mStage == null)
            {
                pClient.Disconnect();
                return;
            }

            if (pClient == pClient.mStage.stageMaster)
                return;
            pClient.mState = (MMatchObjectStageState)state;
            StageMgr.UpdateState(pClient);
        }

        [PacketHandler(Operation.StageTeam, PacketFlags.Stage)]
        public static void ProcessStageTeam (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidStage = pPacket.ReadUInt64();
            var team = pPacket.ReadInt32();

            if (!Enum.IsDefined(typeof(MMatchTeam), team) || pClient.mStage == null)
            {
                pClient.Disconnect();
                return;
            }

            pClient.mTeam = (MMatchTeam)team;
            StageMgr.UpdateTeam(pClient);
        }

        [PacketHandler(Operation.StageChat, PacketFlags.Stage)]
        public static void ResponseStageChat (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidStage = pPacket.ReadUInt64();
            var message = pPacket.ReadString();

            if (pClient.mStage == null || message.Length > 127)
            {
                pClient.Disconnect();
                return;
            }

            StageMgr.Chat(pClient, message);
        }

        [PacketHandler(Operation.StageUpdateSettings, PacketFlags.Stage)]
        public static void ProcessStageSetting (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var uidStage = pPacket.ReadUInt64();
            var total = pPacket.ReadInt32();
            var size = pPacket.ReadInt32();
            var count = pPacket.ReadInt32();
            var uidStage2 = pPacket.ReadUInt64();
            var map = pPacket.ReadString(32);
            var index = pPacket.ReadInt32();
            var type = pPacket.ReadInt32();
            var rounds = pPacket.ReadInt32();
            var time = pPacket.ReadInt32();
            var level = pPacket.ReadInt32();
            var players = pPacket.ReadInt32();
            var teamkill = pPacket.ReadBoolean();
            var join = pPacket.ReadBoolean();
            var balance = pPacket.ReadBoolean();
            var win = pPacket.ReadBoolean();

            if ((MMatchObjectStageGameType)type != pClient.mStage.nGameType)
            {
                if (!Enum.IsDefined(typeof(MMatchObjectStageGameType), (byte)type))
                {
                    pClient.Disconnect();
                    return;
                }
                pClient.mStage.nGameType = (MMatchObjectStageGameType)type;
            }

            pClient.mStage.nRounds = rounds;
            pClient.mStage.nTime = Convert.ToByte(time);
            pClient.mStage.nLevel = Convert.ToByte(level);
            pClient.mStage.nMaxPlayers = Convert.ToByte(players);
            pClient.mStage.bForcedEntry = join;
            pClient.mStage.bTeamBalance = balance;
            pClient.mStage.bTeamKill = teamkill;
            //pClient.mStage.bTeamWinThePoint = win;
            StageMgr.UpdateSetting(pClient);
            StageMgr.StageList(pClient);
        }

        [PacketHandler(Operation.StageStart, PacketFlags.Stage)]
        public static void ProcessStageStart (Client pClient, PacketReader pPacket)
        {
            if (pClient.mStage == null || pClient.mStage.stageMaster != pClient)
                return;

            List<Client> clients = pClient.mStage.Clients.FindAll(c => c.mState != MMatchObjectStageState.Ready && pClient.mStage.stageMaster != c);
            if (clients.Count > 0)
            {
                PacketWriter pError = new PacketWriter(Operation.QuestFail, CryptFlags.Encrypt);
                pError.Write((UInt32)1);
                pError.Write(pClient.mStage.uidStage);
                foreach (Client c in pClient.mStage.Clients)
                {
                    c.Send(pError);
                }
                return;
            }

            pClient.mStage.BattleMgr.GameStartCallback(pClient);
            StageMgr.StageList(pClient);
        }
    }
}
