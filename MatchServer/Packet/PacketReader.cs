using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MatchServer.Packet
{
    sealed class PacketReader : BinaryReader
    {
        public PacketReader(byte[] pBuffer, int nIndex, int nSize) :
            base(new MemoryStream(pBuffer, nIndex, nSize, false, true))
        {
        }

        public override string ReadString()
        {
            ReadUInt16();
            return base.ReadString();
        }
    }
}
