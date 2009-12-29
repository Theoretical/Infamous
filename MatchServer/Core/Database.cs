using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

using MatchServer.Packet;
namespace MatchServer.Core
{
    class Database
    {
        private static string m_szConnectionString = "User ID=sa;Password=asd;Server=TRINITYLAPTOP\\SQLEXPRESS;Trusted_Connection=true;Database=GunzDB;Connection Timeout = 1; Pooling=True;MultipleActiveResultSets=True";
        private static SqlConnection m_sqlConnection = null;

        public static bool Initialize()
        {
            try
            {
                m_sqlConnection = new SqlConnection(m_szConnectionString);
                m_sqlConnection.Open();
                return true;
            }
            catch (Exception e)
            {
                Log.Write("Error Initializing DB: {0}", e.Message);
                return false;
            }
        }
        public static void Execute(string szQuery)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand(szQuery, m_sqlConnection))
                    sqlCMD.ExecuteNonQuery();
            }
        }

        public static void Execute(string szQuery, ArrayList pArray)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand(szQuery, m_sqlConnection))
                {
                    using (SqlDataReader sqlDR = sqlCMD.ExecuteReader())
                    {
                        while (sqlDR.Read())
                            for (int i = 0; i < sqlDR.FieldCount; ++i)
                                if (!sqlDR.IsDBNull(i))
                                    pArray.Add(sqlDR[i]);
                                else
                                    pArray.Add(0);
                    }   
                }
            }
        }

        public static int GetQuery(string szQuery)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand(szQuery, m_sqlConnection))
                    return (Int32)sqlCMD.ExecuteScalar();
            }
        }

        public static int GetIdentity(string szQuery)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand(szQuery, m_sqlConnection))
                    using (SqlDataReader sqlDR = sqlCMD.ExecuteReader())
                    {
                        sqlDR.Read();
                        return Convert.ToInt32(sqlDR[0]);
                    }
            }
        }

        public static void GetAccount(string szUser, string szPassword, ref MMatchAccountInfo accountInfo)
        {
            if (accountInfo == null)
                accountInfo = new MMatchAccountInfo();
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand(string.Format("SELECT Account.AID, Account.UGradeID, Account.PGradeID FROM Account,Login WHERE Login.UserID='{0}' AND Login.Password='{1}' AND Login.AID = Account.AID", szUser, szPassword), m_sqlConnection))
                {
                    using (SqlDataReader sqlDR = sqlCMD.ExecuteReader())
                    {
                        if (sqlDR.Read())
                        {
                            accountInfo.nAID = Convert.ToInt32(sqlDR["AID"]);
                            accountInfo.nUGradeID = (MMatchUserGradeID)Convert.ToByte(sqlDR["UGradeID"]);
                            accountInfo.nPGradeID = (MMatchPremiumGradeID)Convert.ToByte(sqlDR["PGradeID"]);
                            accountInfo.szUserID = szUser;
                        }
                        else
                        {
                            accountInfo.szUserID = "INVALID";
                            accountInfo.nUGradeID = MMatchUserGradeID.Guest;
                            accountInfo.nPGradeID = 0;
                        }
                    }
                }
            }
        }

        public static void GetCharacterList(Int32 nAID, PacketWriter pPacket)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand(string.Format("SELECT * FROM Character WHERE DeleteFlag=0 AND AID={0} ORDER BY CharNum ASC", nAID), m_sqlConnection))
                {
                    using (SqlDataReader sqlDR = sqlCMD.ExecuteReader())
                    {
                        for (byte b = 0; sqlDR.Read(); b++)
                        {
                            pPacket.Write(Convert.ToString(sqlDR["Name"]), 32);
                            pPacket.Write(b);
                            pPacket.Write(Convert.ToByte(sqlDR["Level"]));
                        }
                    }
                }
            }
        }

        public static bool CreateCharacter(Int32 nAID, byte nCharNumber, string szName, Int32 nSex, Int32 nHair, Int32 nFace, Int32 nCostume)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand("dbo.spInsertChar", m_sqlConnection))
                {
                    sqlCMD.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCMD.Parameters.AddWithValue("@AID", nAID);
                    sqlCMD.Parameters.AddWithValue("@CharNum", nCharNumber);
                    sqlCMD.Parameters.AddWithValue("@Name", szName);
                    sqlCMD.Parameters.AddWithValue("@Sex", nSex);
                    sqlCMD.Parameters.AddWithValue("@Hair", nHair);
                    sqlCMD.Parameters.AddWithValue("@Face", nFace);
                    sqlCMD.Parameters.AddWithValue("@Costume", nCostume);
                    SqlParameter returnValue = new SqlParameter("@Return_Value", DbType.Int32);
                    returnValue.Direction = System.Data.ParameterDirection.ReturnValue;

                    sqlCMD.Parameters.Add(returnValue);
                    sqlCMD.ExecuteNonQuery();
                    return Int32.Parse(sqlCMD.Parameters["@Return_Value"].Value.ToString()) != -1;
                }
            }
        }

        public static void UpdateIndexes(Int32 nAID)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand("SELECT TOP 4 CID FROM Character WHERE AID=" + nAID, m_sqlConnection))
                {
                    using (SqlDataReader sqlDR = sqlCMD.ExecuteReader())
                    {
                        for (int i = 0; sqlDR.Read(); ++i)
                            Execute(string.Format("update character set CharNum={0} where cid={1}", i, sqlDR[0]));
                    }
                }
            }
        }

        public static bool GetCharacter(Int32 nAID, byte nIndex, MTD_CharInfo charInfo)
        {
            lock (m_sqlConnection)
            {
                using (SqlCommand sqlCMD = new SqlCommand(string.Format("SELECT * FROM Character WHERE AID={0} AND CharNum={1}", nAID, nIndex), m_sqlConnection))
                {
                    using (SqlDataReader sqlDR = sqlCMD.ExecuteReader())
                    {
                        if (!sqlDR.Read())
                            return false;
                        charInfo.nCID = Convert.ToInt32(sqlDR["CID"]);
                        charInfo.nCLID = 0;//Convert.ToInt32(sqlDR["CLID"]);
                        charInfo.szName = Convert.ToString(sqlDR["Name"]);
                        charInfo.nLevel = Convert.ToByte(sqlDR["Level"]);
                        charInfo.nSex = Convert.ToByte(sqlDR["Sex"]);
                        charInfo.nHair = Convert.ToByte(sqlDR["Hair"]);
                        charInfo.nFace = Convert.ToByte(sqlDR["Face"]);
                        charInfo.nXP = Convert.ToInt32(sqlDR["XP"]);
                        charInfo.nBP = Convert.ToInt32(sqlDR["BP"]);
                        charInfo.fBonusRate = 0.0f;
                        charInfo.nPrize = 0;
                        charInfo.nFR = 0;
                        charInfo.nER = 0;
                        charInfo.nCR = 0;
                        charInfo.nWR = 0;
                        charInfo.nSafeFalls = 0;

                        ArrayList items = new ArrayList();
                        Execute("SELECT head_slot,chest_slot,hands_slot,legs_slot,Feet_slot,fingerl_slot,fingerr_slot,melee_slot,primary_slot,secondary_slot,custom1_slot,custom2_slot FROM Character WHERE CID=" + charInfo.nCID, items);
                        for (int i = 0; i < 12; i++)
                        {
                            charInfo.nEquippedItems[i] = new Item();
                            charInfo.nEquippedItems[i].nItemCID = Convert.ToInt32(items[i]);
                        }
                        sqlCMD.CommandText = "SELECT ItemID,CIID,RentHourPeriod FROM CharacterItem WHERE CID=" + charInfo.nCID;
                        sqlDR.Close();
                        using (SqlDataReader sDR = sqlCMD.ExecuteReader())
                        {
                            while (sDR.Read())
                            {
                                Item nItem = new Item();
                                nItem.nItemID = Convert.ToInt32(sDR["itemid"]);
                                nItem.nItemCID = Convert.ToInt32(sDR["CIID"]);
                                nItem.nRentHour = Convert.ToInt32(sDR.IsDBNull(2) ? 0 : sDR["RentHourPeriod"]);
                                charInfo.nItems.Add(nItem);
                            }
                        }
                        for (int i = 0; i < 12; i++)
                        {
                            Item item = charInfo.nItems.Find(ii => ii.nItemCID == charInfo.nEquippedItems[i].nItemCID);
                            if (item == null)
                                charInfo.nEquippedItems[i].nItemID = 0;
                            else
                                charInfo.nEquippedItems[i].nItemID = item.nItemID;
                        }
                        return true;
                    }
                }
            }
        }
    }
}
