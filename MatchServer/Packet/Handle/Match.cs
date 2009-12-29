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
            pClient.UnloadCharacter();

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

        [PacketHandler(Operation.MatchRequestCreateChar , PacketFlags.Login)]
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

        [PacketHandler(Operation.MatchRequestDeleteChar, PacketFlags.Login)]
        public static void ProcessDeleteChar (Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var index = pPacket.ReadInt32();
            var name = pPacket.ReadString();
            var result = Results.Accepted;

            if (uid != pClient.mClientUID || !Program.mRegex.IsMatch(name) || index < 0 || index > 4)
            {
                pClient.Disconnect();
                return;
            }

            var cid = Database.GetQuery(string.Format("SELECT CID FROM Character WHERE CharNum={0} AND AID={1}", index, pClient.mAccount.nAID));
            if (cid == 0) result = Results.CharacterDeleteDisabled;
            else
            {
                Database.Execute("DELETE FROM CharacterItem WHERE CID=" + cid);
                Database.Execute("DELETE FROM Character WHERE CID=" + cid);
                Database.UpdateIndexes(pClient.mAccount.nAID);
            }

            PacketWriter pResponseDeleteChar = new PacketWriter(Operation.MatchResponseDeleteChar, CryptFlags.Encrypt);
            pResponseDeleteChar.Write((Int32)result);
            pClient.Send(pResponseDeleteChar);
        }

        [PacketHandler(Operation.MatchRequestSelectChar, PacketFlags.Login)]
        public static void ProcessSelectChar (Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var index = pPacket.ReadInt32();

            if (uid != pClient.mClientUID || index < 0 || index > 4)
            {
                pClient.Disconnect();
                return;
            }

            PacketWriter pResponseSelectChar = new PacketWriter(Operation.MatchResponseSelectChar, CryptFlags.Encrypt);
            pResponseSelectChar.Write((Int32)0);
            pResponseSelectChar.Write(pClient.mCharacter);
            pResponseSelectChar.Write(1, 1);
            pResponseSelectChar.Write((byte)0x3E);
            pClient.Send(pResponseSelectChar);

            pClient.mClientFlags = PacketFlags.Character;
        }

        [PacketHandler(Operation.MatchRequestRecommendedChannel, PacketFlags.Character)]
        public static void ProcessRequestRecommendChannel (Client pClient, PacketReader pPacket)
        {
            MMatchChannel channel = null;
            channel = Program.mChannels.Find(c => c.nMaxUsers > c.lClients.Count && c.nMinLevel < pClient.mCharacter.nLevel && c.nMaxLevel > pClient.mCharacter.nLevel);
            if (channel == null)
                channel = Program.mChannels[0];
            
            PacketWriter pResponseRecommendChannel = new PacketWriter(Operation.MatchResponseRecommendedChannel, CryptFlags.Encrypt);
            pResponseRecommendChannel.Write(channel.uidChannel);
            pClient.Send(pResponseRecommendChannel);
        }

        [PacketHandler(Operation.MatchRequestShopItemList, PacketFlags.Character)]
        public static void ProcessShopItemList (Client pClient, PacketReader pPacket)
        {
            PacketWriter pResponseShop = new PacketWriter(Operation.MatchResponseShopItemList, CryptFlags.Decrypt);
            pResponseShop.Write(1, 12);
            pResponseShop.WriteSkip(12);

            pResponseShop.Write(Program.mShop.Count, 4);
            foreach (uint item in Program.mShop)
                pResponseShop.Write(item);

            pClient.Send(pResponseShop);

        }

        [PacketHandler(Operation.MatchRequestCharacterItemList, PacketFlags.Character)]
        public static void ProcessCharItemList (Client pClient, PacketReader pPacket)
        {
            PacketWriter pResponseCharItems = new PacketWriter(Operation.MatchResponseCharacterItemList, CryptFlags.Decrypt);
            pResponseCharItems.Write(pClient.mCharacter.nBP);

            pResponseCharItems.Write(12, 8);
            for (int i = 0; i < 12; ++i)
            {
                pResponseCharItems.Write((Int32)0);
                pResponseCharItems.Write(pClient.mCharacter.nEquippedItems[i].nItemCID);
            }

            pResponseCharItems.Write(pClient.mCharacter.nItems.Count, 16);
            foreach (Item i in pClient.mCharacter.nItems)
            {
                pResponseCharItems.Write((Int32)0);
                pResponseCharItems.Write(i.nItemCID);
                pResponseCharItems.Write(i.nItemID);
                pResponseCharItems.Write(i.nRentHour);
            }
            pResponseCharItems.Write(0, 12);
            pClient.Send(pResponseCharItems);
        }

        [PacketHandler(Operation.MatchRequestBuyItem, PacketFlags.Character)]
        public static void ProcessBuyItem(Client pClient, PacketReader pPacket)
        {
            var uid = pPacket.ReadUInt64();
            var itemid = pPacket.ReadInt32();
            Item item = null;
            var result = Results.Accepted;

            item = Program.mItems.Find(i => i.nItemID == itemid);
            if (item == null)
                result = Results.ShopItemNonExistant;
            else if (item.nPrice > pClient.mCharacter.nBP)
                result = Results.ShopInsufficientBounty;
            else
            {
                Item temp = new Item();
                temp.nItemID = item.nItemID;
                temp.nLevel = item.nLevel;
                temp.nMaxWT = item.nMaxWT;
                temp.nWeight = item.nWeight;
                temp.nPrice = item.nPrice;
                Database.Execute(string.Format("INSERT INTO CharacterItem (CID,ItemID,RegDate) VALUES ({0},{1},GetDate())", pClient.mCharacter.nCID, item.nItemID));
                temp.nItemCID = Database.GetIdentity(string.Format("select @@identity"));
                pClient.mCharacter.nItems.Add(temp);

                pClient.mCharacter.nBP -= item.nPrice;
                Database.Execute(string.Format("UPDATE Character SET BP={0} WHERE CID={1}", pClient.mCharacter.nBP, pClient.mCharacter.nCID));
            }

            PacketWriter pResponseBuyItem = new PacketWriter(Operation.MatchResponseBuyItem, CryptFlags.Decrypt);
            pResponseBuyItem.Write((Int32)result);
            pClient.Send(pResponseBuyItem);
        }

        [PacketHandler(Operation.MatchRequestSellItem, PacketFlags.Character)]
        public static void ProcessSellItem (Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var low = pPacket.ReadInt32();
            var high = pPacket.ReadInt32();
            var result = Results.Accepted;

            var item = pClient.mCharacter.nItems.Find (i => i.nItemCID == high);
            if (item == null)
                result = Results.ShopItemNonExistant;
            else
            {
                Database.Execute(string.Format("DELETE FROM CharacterItem WHERE CIID={0}", item.nItemCID));
                pClient.mCharacter.nBP += item.nPrice;
                Database.Execute(string.Format("UPDATE Character SET BP={0} WHERE CID={1}", pClient.mCharacter.nBP, pClient.mCharacter.nCID));
                pClient.mCharacter.nItems.Remove(item);
            }

            PacketWriter pResponseSellItem = new PacketWriter(Operation.MatchResponseSellItem, CryptFlags.Decrypt);
            pResponseSellItem.Write((Int32)result);
            pClient.Send(pResponseSellItem);
        }

        [PacketHandler(Operation.MatchRequestEquipItem, PacketFlags.Character)]
        public static void ProcessEquipItem(Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var nItemLow = pPacket.ReadInt32();
            var nItemHigh = pPacket.ReadInt32();
            var nItemSlot = pPacket.ReadInt32();
            Results result = Results.Accepted;

            if (!Enum.IsDefined(typeof(MMatchItemSlotType), nItemSlot))
            {
                pClient.Disconnect();
                return;
            }

            Item nItem = pClient.mCharacter.nItems.Find(i => i.nItemCID == nItemHigh);
            if (nItem == null)
                result = Results.ShopItemNonExistant;
            else
            {
                pClient.mCharacter.nEquippedItems[nItemSlot].nItemCID = nItemHigh;
                pClient.mCharacter.nEquippedItems[nItemSlot].nItemID = nItem.nItemID;
                Database.Execute(string.Format("UPDATE Character SET {0}={1} WHERE CID={2}", (MMatchItemSlotType)nItemSlot, nItemHigh, pClient.mCharacter.nCID));
            }
            PacketWriter pResponseEquipItem = new PacketWriter(Operation.MatchResponseEquipItem, CryptFlags.Decrypt);
            pResponseEquipItem.Write((Int32)result);
            pClient.Send(pResponseEquipItem);

            ProcessCharItemList(pClient, pPacket);
        }

        [PacketHandler(Operation.MatchRequestTakeOffItem, PacketFlags.Character)]
        public static void ProcessTakeOffItem(Client pClient, PacketReader pPacket)
        {
            var uidChar = pPacket.ReadUInt64();
            var nItemSlot = pPacket.ReadInt32();

            if (!Enum.IsDefined(typeof(MMatchItemSlotType), nItemSlot))
            {
                pClient.Disconnect();
                return;
            }
            pClient.mCharacter.nEquippedItems[nItemSlot].nItemCID = 0;
            pClient.mCharacter.nEquippedItems[nItemSlot].nItemID = 0;
            Database.Execute(string.Format("UPDATE Character SET {0}={1} WHERE CID={2}", (MMatchItemSlotType)nItemSlot, 0, pClient.mCharacter.nCID));

            PacketWriter pResponseTakeOffItem = new PacketWriter(Operation.MatchResponseTakeOffItem, CryptFlags.Decrypt);
            pResponseTakeOffItem.Write((Int32)0);
            pClient.Send(pResponseTakeOffItem);

            ProcessCharItemList(pClient, null);
        }

    }
}
