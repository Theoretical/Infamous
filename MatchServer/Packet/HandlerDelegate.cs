using MatchServer.Core;

namespace MatchServer.Packet
{   
    class HandlerDelegate
    {
        public delegate void PacketProcessor(Client pClient, PacketReader pPacket);

        public PacketProcessor mProcessor = null;
        public PacketFlags mFlags = PacketFlags.Login;

        public HandlerDelegate(PacketProcessor pAction, PacketFlags pFlags)
        {
            mProcessor = pAction;
            mFlags = pFlags;
        }
    }
}
