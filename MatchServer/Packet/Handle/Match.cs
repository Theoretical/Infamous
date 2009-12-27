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
            /* @ARGS
             * UserID - string
             * Password - string
             * CommandVersion - int
             * Checksum - int
             * MD5 - Blob
             */

            var UserID = pPacket.ReadString();
            var Password = pPacket.ReadString();
            var CommandID = pPacket.ReadInt32();
            var Checksum = pPacket.ReadInt32();
            var totalSize = pPacket.ReadInt32();
            var blobSize = pPacket.ReadInt32();
            var count = pPacket.ReadInt32();
            Results result = Results.Accepted;

            if (blobSize > 32)
            {
                client.Disconnect();
                return;
            }

            if (!Program.mRegex.IsMatch(UserID) || !Program.mRegex.IsMatch(Password))
                result = Results.LoginIncorrectPassword;
            else
            {
                Database.GetAccount(UserID, Password, ref client.mAccount);
                if (client.mAccount.nAID == 0)
                    result = Results.LoginIncorrectPassword;
                else if (client.mAccount.nUGradeID == MMatchUserGradeID.Banned || client.mAccount.nUGradeID == MMatchUserGradeID.Penalty)
                    result = Results.LoginBannedID;
            }
            PacketWriter pLoginResponse = new PacketWriter(Operation.MatchLoginResponse, CryptFlags.Encrypt);
            pLoginResponse.Write((Int32)result);
            pLoginResponse.Write("Lol Emu Test");
            pLoginResponse.Write((byte)2);//Moode
            pLoginResponse.Write(UserID);
            pLoginResponse.Write((byte)client.mAccount.nUGradeID);
            pLoginResponse.Write((byte)client.mAccount.nPGradeID);
            pLoginResponse.Write(client.mClientUID);
            pLoginResponse.Write(false);
            pLoginResponse.Write(1, 20);
            pLoginResponse.WriteSkip(20);
            client.Send(pLoginResponse);

            if (client.mAccount.nUGradeID == MMatchUserGradeID.Banned || client.mAccount.nUGradeID == MMatchUserGradeID.Penalty)
                client.Disconnect();
        }
    }
}
