using System;
using System.Threading;
using System.Collections.Generic;

using MatchServer.Core;
using MatchServer.Packet;
using MatchServer.Manager;
namespace MatchServer.Packet.Handle
{
    class Battle
    {
        [PacketHandler(Operation.LoadingComplete, PacketFlags.Stage)]
        public static void ProcessLoading (Client pClient, PacketReader pPacket)
        {
            if (pClient.mStage == null) return;
            pClient.mStage.BattleMgr.GameLoadedCallback(pClient);
        }

        [PacketHandler(Operation.StageRequestEnterBattle, PacketFlags.Stage)]
        public static void ProcessEnterBattle(Client pClient, PacketReader pPacket)
        {
            if (pClient.mStage == null) return;
            pClient.mStage.BattleMgr.GameEnterCalback(pClient);
        }

        [PacketHandler(Operation.BattleRequestInfo, PacketFlags.Stage)]
        public static void ProcessBattleInfo (Client pClient, PacketReader pPacket)
        {
            MMatchStage stage = pClient.mStage;
            PacketWriter packet = new PacketWriter(Operation.BattleResponseInfo, CryptFlags.Encrypt);

            packet.Write(stage.uidStage);
            packet.Write(1,6);
            packet.Write(stage.nRedTeamScore);
            packet.Write(stage.nBlueTeamScore);
            packet.Write((Int32)0);
            packet.Write(0, 0);
            packet.Write(stage.Clients.Count, 17);
            foreach (Client c in stage.Clients)
            {
                packet.Write(c.mClientUID);
                packet.Write(c.mGame.Spawned);
                packet.Write(c.mGame.Kills);
                packet.Write(c.mGame.Deaths);
            }
            pClient.Send(packet);
            stage.BattleMgr.GameInfoCallback(pClient);
        }

        [PacketHandler(Operation.GameRequestSpawn, PacketFlags.Stage)]
        public static void ProcessGameSpawn (Client client, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var xpos = pPacket.ReadSingle();
            var ypos = pPacket.ReadSingle();
            var zpos = pPacket.ReadSingle();
            var xdir = pPacket.ReadSingle();
            var ydir = pPacket.ReadSingle();
            var zdir = pPacket.ReadSingle();

            PacketWriter packet = new PacketWriter(Operation.GameResponseSpawn, CryptFlags.Encrypt);
            packet.Write(client.mClientUID);
            packet.Write((UInt16)xpos);
            packet.Write((UInt16)ypos);
            packet.Write((UInt16)zpos);
            packet.Write((UInt16)xdir);
            packet.Write(new byte[] { 0xfc, 0xc7 }, 0, 2);
            packet.Write((UInt16)ydir);

            foreach (Client c in client.mStage.Clients)
                c.Send(packet);
        }

        [PacketHandler(Operation.GameRequestTimeSync, PacketFlags.Stage)]
        public static void ProcessGameTimeSync(Client client, PacketReader pPacket)
        {
            var nTime = pPacket.ReadInt32();

            PacketWriter packet = new PacketWriter(Operation.GameResponseTimeSync, CryptFlags.Encrypt);
            packet.Write(nTime);
            packet.Write(Program.timeGetTime());
            client.Send(packet);
        }
    }
}
