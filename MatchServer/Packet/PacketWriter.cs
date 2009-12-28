using System;
using System.IO;
using System.Text;

using MatchServer.Core;
namespace MatchServer.Packet
{
    sealed class PacketWriter : BinaryWriter
    {
        public CryptFlags Flags { get; internal set; }
        public UInt16 Operation { get; internal set; }
        
        public PacketWriter(Operation operation, CryptFlags flag)
            : base(new MemoryStream(4096))
        {
            Flags = flag;
            Operation = (UInt16)operation;

            this.Write((UInt16)operation);
            this.Write((byte)0);
        }
        public override void Write(string value)
        {
            if (value == null) value = "";
            Write((UInt16)(value.Length+2));
            var buf = new byte[value.Length+2];
            Encoding.GetEncoding(1252).GetBytes(value, 0, value.Length, buf, 0);
            base.Write(buf);
        }

        public void Write(string pString, int pLength)
        {
            if (pString == null) pString = "";
            if (pString.Length > pLength)
                throw new Exception("Could not write string.");

            byte[] buf = new byte[pLength];
            var used = Encoding.GetEncoding(1252).GetBytes(pString, 0, pString.Length, buf, 0);
            var unused = Math.Min(pLength - 1, used);
            Array.Clear(buf, unused, Math.Max(1, pLength - unused));
            this.Write(buf);
        }

        public void Write(int pCount, int pSize)
        {
            var total = (pCount * pSize) + 8;
            this.Write(total);
            this.Write(pSize);
            this.Write(pCount);
        }

        public void Write(MTD_CharInfo CharInfo)
        {
            Write(1, 146);
            Write(CharInfo.szName, 32);
            Write(CharInfo.szClanName, 16);
            Write((Int32)CharInfo.nClanGrade);
            Write(CharInfo.nClanPoint);
            Write(CharInfo.nCharNum);
            Write(CharInfo.nLevel);
            Write(CharInfo.nSex);
            Write(CharInfo.nHair);
            Write(CharInfo.nFace);
            Write(CharInfo.nXP);
            Write(CharInfo.nBP);
            Write(CharInfo.fBonusRate);
            Write(CharInfo.nPrize);
            Write(CharInfo.nHP);
            Write(CharInfo.nAP);
            Write(CharInfo.nMaxWeight);
            Write(CharInfo.nSafeFalls);
            Write(CharInfo.nFR);
            Write(CharInfo.nCR);
            Write(CharInfo.nER);
            Write(CharInfo.nWR);
            for (int i = 0; i < 12; i++)
                Write(CharInfo.nEquippedItems[i].nItemID);
            Write((Int32)CharInfo.nUGradeID);
            Write(CharInfo.nCLID);
        }


        public void WriteSkip(int pCount)
        {
            this.Write(new byte[pCount]);
        }

        public byte[] Process(byte pCount, byte[] bEncrypt)
        {
            var totalSize = (int)(this.BaseStream.Length + 8);
            var buffer = new byte[totalSize];
            this.BaseStream.Position = 0;
            this.BaseStream.Read(buffer, 8, (int)(totalSize-8));
           
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)Flags), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)totalSize), 0, buffer, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, buffer, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)(totalSize - 6)), 0, buffer, 6, 2);
            buffer[10] = pCount;

            if (Flags == CryptFlags.Encrypt)
            {
                PacketCrypt.Encrypt(buffer, 2, 2, bEncrypt);
                PacketCrypt.Encrypt(buffer, 6, (totalSize - 6), bEncrypt);
            }

            Buffer.BlockCopy(BitConverter.GetBytes(PacketCrypt.CalculateChecksum(buffer, 0, totalSize)), 0, buffer, 4, 2);
            return buffer;
        }   
    }
}
