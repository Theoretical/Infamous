using System;
using System.Collections.Generic;

using MatchServer.Core;
using MatchServer.Packet;
using MatchServer.Manager;

namespace MatchServer.Packet.Handle
{
    class Agent
    {
        [PacketHandler(Operation.MatchRegisterAgent, PacketFlags.None)]
        public static void ProcessRegisterAgent (Client pClient, PacketReader pPacket)
        {
            pClient.mIsAgent = true;
            Log.Write("[{0}] Agent Registered.", pClient.mClientIP);
            lock (Program.mAgents)
                Program.mAgents.Add(pClient);
        }

        [PacketHandler(Operation.MatchAgentRequestLiveCheck, PacketFlags.None)]
        public static void ProcessLiveCheck (Client pClient, PacketReader pPacket)
        {
            var timeStamp = pPacket.ReadInt32();
            PacketWriter pResponseLiveCheck = new PacketWriter(Operation.MatchAgentResponseLiveCheck, CryptFlags.Encrypt);
            pResponseLiveCheck.Write(timeStamp);
            pClient.Send(pResponseLiveCheck);
        }
    }
}
