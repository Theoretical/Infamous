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
            var len = ReadUInt16();
            if (len < 1)
                return String.Empty;
            
            var buffer = new byte[len];
            buffer = this.ReadBytes(len);
            var pString = Encoding.GetEncoding(1252).GetString(buffer);
            return pString.Substring(0, pString.IndexOf('\0'));
        }
    }
}
