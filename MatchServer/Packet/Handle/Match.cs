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
                Log.Write("Login: {0} {1}", UserID, Password);
                Database.GetAccount(UserID, Password, ref client.mAccount);
                if (client.mAccount.nAID == 0)
                    result = Results.LoginIncorrectPassword;
                else if (client.mAccount.nUGradeID == MMatchUserGradeID.Banned || client.mAccount.nUGradeID == MMatchUserGradeID.Penalty)
                    result = Results.LoginBannedID;
                else if (client.mAccount.nAID > 0)
                    client.mClientFlags = PacketFlags.Login;
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
                client. Disconnect();
        }

        [PacketHandler(Operation.MatchRequestAccountCharList, PacketFlags.Login)]
        public static void ProcessCharacters(Client pClient, PacketReader pPacket)
        {
            PacketWriter pResponseChars = new PacketWriter(Operation.MatchResponseAccountCharList, CryptFlags.Encrypt);
            var charCount = Database.GetQuery(string.Format("SELECT COUNT(*) FROM Character WHERE AID={0}", pClient.mAccount.nAID));
            pResponseChars.Write(charCount, 34);
            Database.GetCharacterList(pClient.mAccount.nAID, pResponseChars);
            pClient.Send(pResponseChars);
        }

        [PacketHandler(Operation.MatchRequestAccountCharInfo, PacketFlags.Login)]
        public static void ProcessCharInfo(Client pClient, PacketReader pReader)
        {
            var index = pReader.ReadByte();

            if (index < 0 || index > 4)
            {
                pClient.Disconnect();
                return;
            }

            pClient.mCharacter.nCharNum = index;
            Database.GetCharacter(pClient.mAccount.nAID, index, pClient.mCharacter);
            pClient.mCharacter.nUGradeID = pClient.mAccount.nUGradeID;

            PacketWriter pCharInfoResponse = new PacketWriter(Operation.MatchResponseAccountCharInfo, CryptFlags.Encrypt);
            pCharInfoResponse.Write(index);
            pCharInfoResponse.Write(pClient.mCharacter);
            pClient.Send(pCharInfoResponse);
        }

        [PacketHandler(Operation.MatchRequestCreateChar, PacketFlags.Login)]
        public static void ProcessCreateChar (Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var index = pPacket.ReadInt32();
            var name = pPacket.ReadString();
            var sex = pPacket.ReadInt32();
            var hair = pPacket.ReadInt32();
            var face = pPacket.ReadInt32();
            var costume = pPacket.ReadInt32();
            var result = Results.Accepted;

            if (uid != pClient.mClientUID || index < 0 || index > 4 || sex < 0 || sex > 1)
            {
                pClient.Disconnect();
                return;
            }

            if (!Program.mRegex.IsMatch(name))
                result = Results.CharacterEnterName;
            else if (Database.GetQuery("SELECT COUNT(AID) FROM Character WHERE AID=" + pClient.mAccount.nAID) >= 4)
                result = Results.CharacterInvalidName;
            else if (Database.GetQuery("SELECT COUNT(Name) FROM Character WHERE Name='" + name + "'") > 0)
                result = Results.CharacterNameInUse;
            else if (!Database.CreateCharacter(pClient.mAccount.nAID, (byte)index, name, sex, hair, face, costume))
                result = Results.CharacterInvalidName;

            PacketWriter pResponseCreateChar = new PacketWriter(Operation.MatchResponseCreateChar, CryptFlags.Encrypt);
            pResponseCreateChar.Write((Int32)result);
            pResponseCreateChar.Write(name);
            pClient.Send(pResponseCreateChar);
        }
    }
}
