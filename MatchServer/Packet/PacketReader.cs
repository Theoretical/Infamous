using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using MatchServer.Core;
namespace MatchServer.Packet
{
    sealed class PacketReader : BinaryReader
    {
        private Operation Opcode;
        public Operation getOpcode() { return Opcode; }

        public PacketReader(byte[] pBuffer, int nSize) :
            base(new MemoryStream(pBuffer, 0, nSize, false, true))
        {
            base.BaseStream.Position = 8;
            Opcode = (Operation)base.ReadUInt16();
            base.ReadByte();
        }

        public override string ReadString()
        {
            ReadUInt16();
            return base.ReadString();
        }
    }
}
