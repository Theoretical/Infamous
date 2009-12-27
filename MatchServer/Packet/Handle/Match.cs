using System;

using MatchServer.Core;
using MatchServer.Packet;
namespace MatchServer.Packet.Handle
{
    class Match
    {
        [PacketHandler(Operation.MatchLogin, PacketFlags.None)]
        public static void ProcessLogin(Client client, PacketReader pPacket)
        {
        }
    }
}
