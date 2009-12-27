using System;

namespace MatchServer.Packet
{
    class PacketCrypt
    {
        public static UInt16 CalculateChecksum(byte[] buf, int index, int length)
        {
            UInt32 sum1 = (UInt32)buf[index] + buf[index + 1] + buf[index + 2] + buf[index + 3];
            UInt32 sum2 = 0;
            for (int x = 6; x < length; ++x) sum2 += buf[index + x];
            UInt32 sum3 = sum2 - sum1;
            UInt32 sum4 = sum3 >> 0x10;
            sum3 += sum4;
            return (UInt16)sum3;
        }

        public static void Decrypt(byte[] buf, int index, int length, byte[] Key)
        {
            for (int i = 0; i < length; ++i)
            {
                byte a = (byte)(buf[index + i]);
                a ^= 0x0F0;
                byte b = (byte)(a & 0x1f);
                a >>= 5;
                b <<= 3;
                b = (byte)(a | b);
                buf[index + i] = (byte)(b ^ Key[i % 32]);

            }
        }

        public static void Encrypt(byte[] buf, int index, int length, byte[] Key)
        {
            for (int i = 0; i < length; ++i)
            {
                ushort a = buf[index + i];
                a ^= Key[i % 32];
                a <<= 5;

                byte b = (byte)(a >> 8);
                b |= (byte)(a & 0xFF);
                b ^= 0xF0;
                buf[index + i] = (byte)b;

            }
        }
    }
}
