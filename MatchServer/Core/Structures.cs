using System;
using System.Collections.Generic;

namespace MatchServer.Core
{
    class EXP
    {
        public static int[] EXPTable = new int[] {
        	0,200,1000,2800,6000,11000,18200,28000,40800,57000,77000,101200,130000,
        	163800,203000,248000,299200,357000,421800,494000,574000,671020,777500,
        	893880,1020600,1158100,1306820,1467200,1639680,1824700,2022700,2253340,
        	2499100,2760460,3037900,3331900,3642940,3971500,4318060,4683100,5067100,
        	5537780,6031700,6549420,7091500,7658500,8335620,9042500,9779780,10548100,
        	11348100,12284460,13257900,14269140,15318900,16407900,17662300,18961900,
        	20307500,21699900,23139900,26116700,29191900,3237100,35643900,39023900,
        	45993500,53175900,60574300,68191900,76031900,88130300,100571900,113361500,
        	126503900,140003900,158487100,177459900,196928700,216899900,237379900,263623900,
        	290519900,318075900,346299900,375199900,410700700,447031900,484203100,522223900,
            561103900
        };
    }

    public class MMatchAccountInfo
    {
        public Int32 nAID = 0;
        public string szUserID = "";
        public MMatchUserGradeID nUGradeID = MMatchUserGradeID.Guest;
        public MMatchPremiumGradeID nPGradeID = MMatchPremiumGradeID.Free;
    }

    public class MTD_CharInfo
    {
        public Int32 nCID = 0;
        public string szName = "";
        public string szClanName = "";
        public MMatchClanGrade nClanGrade;
        public Int16 nClanPoint = 0;
        public byte nCharNum = 0;
        public Int16 nLevel = 0;
        public byte nSex = 0;
        public byte nHair = 0;
        public byte nFace = 0;
        public Int32 nXP = 0;
        public Int32 nBP = 0;
        public Single fBonusRate = 0.0f;
        public Int16 nPrize = 0;
        public Int16 nHP = 0;
        public Int16 nAP = 0;
        public Int16 nMaxWeight = 100;
        public Int16 nSafeFalls = 0;
        public Int16 nFR = 0;
        public Int16 nCR = 0;
        public Int16 nER = 0;
        public Int16 nWR = 0;
        public Item[] nEquippedItems = new Item[12];
        public MMatchUserGradeID nUGradeID;
        public Int32 nCLID;
        public List<Item> nItems = new List<Item>();
    }

    class MMatchChannel
    {
        public UInt64 uidChannel = 0;
        public string szName = "";
        public Int32 nMinLevel = 0;
        public Int32 nMaxLevel = 0;
        public Int32 nMaxUsers = 0;
        public MMatchChannelType nChannelType = MMatchChannelType.User;
        public MMatchChannelRule nChannelRule = MMatchChannelRule.Elite;
        public List<Client> lClients = new List<Client>();
        public List<MMatchStage> lStages = new List<MMatchStage>();
    }

    class MMatchStage
    {
        public UInt64 uidStage = 0;
        public Client stageMaster = null;
        public string szName = "";
        public string szPassword = "";
        public string szMap = "Mansion";
        public MMatachStageState nStageState = MMatachStageState.Standby;
        public MMatchRoundState nRoundState = MMatchRoundState.Prepare;
        public MMatchObjectStageGameType nGameType = MMatchObjectStageGameType.Berserker;
        public byte nMaxPlayers = 8;
        public bool bTeamKill = false;
        public bool bTeamWinThePoint = false;
        public bool bForcedEntry = false;
        public bool bTeamBalance = false;
        public bool bInitialStart = false;
        public bool bPassword = false;
        public Int32 nRounds = 50;
        public byte nTime = 30;
        public byte nLevel = 0;
        public byte nRound = 0;
        public byte nRedTeamScore = 0;
        public byte nBlueTeamScore = 0;
        public List<Client> Clients = new List<Client>();
        public List<MMatchWorldItemSpawnInfo> Items = new List<MMatchWorldItemSpawnInfo>();
        public Int32 nItemCount = 0;
    }
    class MMatchFriendNode
    {
        public UInt32 nFriendCID;
        public UInt16 nFavorite;
        public string szName = ""; //Len = 32
        public MMatchPlace nState; //MMatchState
        public string szDescription = ""; //Len = 64
    }

    public class Item
    {
        public Int32 nItemCID = 0;
        public Int32 nItemID = 0;
        public Int32 nRentHour = 0x20050800;
        public Int32 nPrice = 0;
        public byte nSex = 0;
        public byte nLevel = 0;
        public Int32 nWeight = 0;
        public Int32 nMaxWT = 0;
    }
    class MMatchWorldItem   
    {
        public UInt16 nUID;
        public UInt16 nItemID;
        public Int16 nStaticSpawnIndex;
        public Single x;
        public Single y;
        public Single z;
        public Int32 nLifeTime;
        public Int32 nQuestItemID;
    }

    class MMatchWorldItemSpawnInfo
    {
        public UInt16 nItemID = 0;
        public UInt16 nCoolTime = 83;
        public UInt16 nElapsedTime = 1;
        public Single x = 0;
        public Single y = 0;
        public Single z = 0;
        public Int32 ItemUID = 1;
        public bool bExist = false;
        public bool bUsed = false;
    }

    interface IGame
    {
        void GameStartCallback(Client client);
        void GameJoinCallback(Client client);
        void GameInfoCallback(Client client);
        void GameLoadedCallback(Client client);
        void GameEnterCalback(Client client);
        void GameExitCallback(Client client);
        void GameKillCallback(Client client, Client killer);
        void GameSuicideCallback(Client client);
    }

    class MGame
    {
        public Int32 Kills = 0;
        public Int32 Deaths = 0;
        public bool Loaded = false;
        public bool InGame = false;
        public bool EnteredGame = false;
        public bool RequestedInfo = false;
        public bool Spawned = false;
        public bool bBeserker = false;
    }
}
