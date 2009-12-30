using System;

namespace MatchServer.Core
{
    [Flags]
    enum CryptFlags
    {
        Exchange = 10,
        Decrypt = 100,
        Encrypt
    }

    [Flags]
    enum PacketFlags
    {
        None,
        Login,
        Character,
        Stage,
        Battle
    }
    public enum MMatchUserGradeID : byte
    {
        Guest = 0,
        Registered = 1,
        Event = 2,
        Criminal = 100,
        Warning1 = 101,
        Warning2 = 102,
        Warning3 = 103,
        ChatDenied = 104,
        Penalty = 105,
        EventMaster = 252,
        Banned = 253,
        Developer = 254,
        Administrator = 255
    }
    public enum MMatchPremiumGradeID : byte
    {
        Free,
        PremiumIP
    }
    public enum MMatchClanGrade : int
    {
        None,
        Master,
        Admin,
        User = 9
    }
    public enum MMatchItemType : int
    {
        Melee,
        Range,
        Equipment,
        Custom,
        None
    }
    public enum MMatchItemSlotType : int
    {
        head_slot,
        chest_slot,
        hands_slot,
        legs_slot,
        feet_slot,
        fingerl_slot,
        fingerr_slot,
        melee_slot,
        primary_slot,
        secondary_slot,
        custom1_slot,
        custom2_slot
    }
    public enum MMatchChannelType : byte
    {
        General,
        Private,
        User,
        Clan,
        None = 255

    }
    public enum MMatchChannelRule : byte
    {
        Novice,
        Newbie,
        Rookie,
        Mastery,
        Elite
    }
    public enum MMatchPlace : byte
    {
        Outside,
        Lobby,
        Stage,
        Battle,
        End
    }
    public enum MMatchObjectStageState : byte
    {
        NonReady,
        Ready,
        Shop,
        Equipment,
        End
    }
    public enum MMatachStageState : byte
    {
        Standby,
        Coutdown,
        Battle,
        End
    }
    public enum MMatchRoundState : byte
    {
        Prepare,
        Countdown,
        Play,
        Finish,
        Exit,
        Free,
        Failed
    }
    public enum MMatchObjectStageGameType : byte
    {
        DeathMatch,
        TeamDeathMatch,
        Gladiator,
        TeamGladiator,
        Assassination,
        Training,
        Survival,
        Quest,
        Berserker,
        TeamDeathMatchExtreme,
        Duel
    }
    public enum MMatchTeam : int
    {
        All,
        Spectator,
        Red,
        Blue,
        End
    }

    public enum ObjectCache : byte
    {
        Keep,
        New,
        Expire
    }
    public enum Results : int
    {
        Accepted = 0,
        LoginIncorrectPassword = 10000,
        LoginIDInUse = 10001,
        LoginInvalidVersion = 10002,
        LoginServerFull = 10003,
        LoginBannedID = 10004,
        LoginAuthenticationFailed = 10005,
        CharacterNameInUse = 10100,
        CharacterInvalidName = 10101,
        CharacterNameTooShort = 10102,
        CharacterNameTooLong = 10103,
        CharacterEnterName = 10104,
        CharacterNameNonExistant = 10110,
        CharacterDeleteDisabled = 10111,
        ShopInsufficientBounty = 20001,
        ShopInvalidItem = 20007,
        ShopInventoryFull = 20008,
        ShopItemNonExistant = 20009,
        ShopLevelTooLow = 20011,
        StageRoomFull = 30001,
        StageIncorrectPassword = 30002,
        StageInvalidLevel = 30003,
        StageInvalid = 30006,
        StageCreateDisabled = 30008,
        ChannelJoinDenied = 30020,
        ChannelFull = 300021,
        ChannelInvalidLevel = 300022,
    }

    public enum Operation : ushort
    {
        QuestFail = 0x17AC,
        MatchNotify = 401,
        MatchAnnounce = 402,
        MatchResponseResult = 403,
        //MatchLogin = 1000,
        MatchLogin = 1001,
        MatchLoginResponse = 1002,
        MatchLoginNetmarble = 1003,
        MatchLoginNetmarbleJP = 1004,
        MatchBridgePeer = 1006,
        MatchBridgePeerACK = 1007,
        MatchDisconnectMsg = 1010,
        MatchLoginNHN = 1011,
        MatchObjectCache = 1101,
        MatchRequestRecommendedChannel = 1201,
        MatchResponseRecommendedChannel = 1202,
        MatchRequestPeerRelay = 1471,
        MatchResponsePeerRelay = 1472,
        MatchRequestSuicide = 1531,
        MatchResponseSuicide = 1532,
        MatchRequestObtainWorldItem = 1541,
        MatchWorldItemObtain = 1542,
        MatchWorldItemSpawn = 1543,
        MatchRequestSpawnWorldItem = 1544,
        MatchRequestSpawnWorldItem2 = 1545,
        MatchAssignCommander = 1551,
        MatchResetTeamMembers = 1552,
        MatchSetObserver = 1553,
        MatchRequestProposal = 1561,
        MatchResponseProposal = 1562,
        MatchAskAgreement = 1563,
        MatchReplyAgreement = 1564,
        MatchLadderRequestChallenge = 1571,
        MatchLadderResponseChallenge = 1572,
        MatchLadderSearchRival = 1574,
        MatchLadderRequestCancelChallenge = 1575,
        MatchLadderCancelChallenge = 1576,
        MatchWhisper = 1601,
        MatchWhere = 1602,
        MatchUserOption = 1605,
        MatchRequestAccountCharList = 1701,
        MatchResponseAccountCharList = 1702,
        MatchRequestSelectChar = 1703,
        MatchResponseSelectChar = 1704,
        MatchRequestCharInfo = 1705,
        MatchResponseCharInfo = 1706,
        MatchRequestCreateChar = 1711,
        MatchResponseCreateChar = 1712,
        MatchRequestDeleteChar = 1713,
        MatchResponseDeleteChar = 1714,
        MatchRequestCopyToTestServer = 1715,
        MatchResponseCopyToTestServer = 1716,
        MatchRequestCharInfoDetail = 1717,
        MatchResponseCharInfoDetail = 1718,
        MatchRequestAccountCharInfo = 1719,
        MatchResponseAccountCharInfo = 1720,
        MatchRequestSimpleCharInfo = 1801,
        MatchResponseSimpleCharInfo = 1802,
        MatchRequestMySimpleCharInfo = 1803,
        MatchResponseMySimpleCharInfo = 1804,
        MatchRequestBuyItem = 1811,
        MatchResponseBuyItem = 1812,
        MatchRequestSellItem = 1813,
        MatchResponseSellItem = 1814,
        MatchRequestShopItemList = 1815,
        MatchResponseShopItemList = 1816,
        MatchRequestCharacterItemList = 1821,
        MatchResponseCharacterItemList = 1822,
        MatchRequestEquipItem = 1823,
        MatchResponseEquipItem = 1824,
        MatchRequestTakeOffItem = 1825,
        MatchResponseTakeOffItem = 1826,
        MatchRequestAccountItemList = 1831,
 
        MatchResponseAccountItemList = 1832,
        MatchRequestBringAccountItem = 1833,
        MatchResponseBringAccountItem = 1834,
        MatchRequestBringBackAccountItem = 1835,
        MatchResponseBringBackAccountItem = 1836,
        MatchExpiredRentItem = 1837,
        MatchFriendAdd = 1901,
        MatchFriendRemove = 1902,
        MatchFriendList = 1903,
        MatchResponseFriendList = 1904,
        MatchFriendMsg = 1905,
        MatchClanRequestCreateClan = 2000,
        MatchClanResponseCreateclan = 2001,
        MatchClanAskSponsorAgreement = 2002,
        MatchClanAnswerSponsorAgreement = 2003,
        MatchRequestAgreedCreateClan = 2004,
        MatchResponseAgreedCreateClan = 2005,
        MatchClanRequestCloseClan = 2006,
        MatchClanResponseCloseClan = 2007,
        MatchClanRequestJoinClan = 2008,
        MatchClanResponseJoinClan = 2009,
        MatchClanAskJoinAgreement = 2010,
        MatchClanAnswerJoinAgreement = 2011,
        MatchClanRequestAgreedJoinClan = 2012,
        MatchClanResponseAgreedJoinClan = 2013,
        MatchClanRequestLeaveClan = 2014,
        MatchClanResponseLeaveClan = 2015,
        MatchClanUpdateClanInfo = 2016,
        MatchClanMasterRequestChangeGrade = 2017,
        MatchClanMasterResponseChangeGrade = 2018,
        MatchClanAdminRequestExpelMember = 2019,
        MatchClanAdminResponseLeaveMember = 2020,
        MatchClanRequestMsg = 2021,
        MatchClanMsg = 2022,
        MatchClanRequestClanMemberList = 2023,
        MatchClanResponseClanMemberList = 2024,
        MatchClanRequestClanInfo = 2025,
        MatchClanResponseClanInfo = 2026,
        MatchClanStandbyClanList = 2027,
        MatchClanMemberConnected = 2028,
        MatchClanRequestEmblemInfo = 2051,
        MatchClanResponseEmblemInfo = 2052,
        MatchClanLocalEmblemReady = 2053,
        MatchClanAnnounceDelete = 2054,
        MatchCallVote = 2100,
        MatchNotifyCallVote = 2101,
        MatchNotifyVoteResult = 2102,
        MatchVoteYes = 2105,
        MatchVoteNo = 2106,
        MatchVoteStop = 2108,
        MatchBroadcastClanRenewVictories = 2200,
        MatchBroadcastClanInterruptVictories = 2201,
        MatchBroadcastDuelRenewVictories = 2202,
        MatchBroadcastDuelInterruptVictories = 2203,
        MatchAssignBerserker = 3001,
        MatchDuelQueueInfo = 3100,
        MatchQuestPing = 6012,
        MatchQuestPong = 6013,
        MatchResponseRuleset = 0x4CF,

        ChannelJoin = 1205,
        ChannelRequestJoinFromName = 1206,
        ChannelResponseJoin = 1207,
        ChannelLeave = 1208,
        ChannelListStart = 1211,
        ChannelListStop = 1212,
        ChannelList = 1213,
        ChannelRequestPlayerList = 1221,
        ChannelResponsePlayerList = 1222,
        ChannelRequestChat = 1225,
        ChannelChat = 1226,

        StageCreate = 1301,
        StageJoin = 1303,
        StageRequestJoin = 1304,
        StageLeave = 1307,
        StageRequestPritaveJoin = 1305,
        StageResponseJoin = 1306,
        StageListRequest = 1311,
        StageList = 1314,
        StageChat = 1321,
        StageRequestSettings = 1411,
        StageResponseSettings = 1412,
        StageUpdateSettings = 1413,
        StageTeam = 0x58F,
        StageMaster = 0x58D,
        StageState = 0x58E,
        StageMap = 0x586,
        StageStart = 0x597,
        StageLaunch = 0x598,
        StageRequestEnterBattle = 0x579,
        StageEnterBattle = 0x57A,
        StageLeaveBattle = 0x57B,
        StageRoundState = 0x5DD,
        StageRequestForcedEntry = 0x587,
        StageResponseForcedEntry = 0x588,
        StageRequestPeerList = 0x5B5,
        StageResponsePeerList = 0x5B6,
        BattleRequestInfo = 1451,
        BattleResponseInfo = 1452,

        LoadingComplete = 0x5A1,
        GameRequestTimeSync = 0x5F1,
        GameResponseTimeSync = 0x5F2,
        GameRequestInfo = 0x5AB,
        GameResponseInfo = 0x5AC,
        GameRequestSpawn = 0x5EB,
        GameResponseSpawn = 0x5EC,
        GameKill = 0x5E7,
        GameDie = 0x5E8,
        GameLevelUp = 0x5E9,
        GameTeamBonus = 0x5ED,
        BridgeRequest = 1006,
        BridgeResponse = 1007,
        CRCRequest = 0x2af9,
        AgentError = 0x1395,
        AgentRelayPeer = 0x13C6,

        MatchRegisterAgent = 0x1389,
        MatchUnregisterAgent = 0x138A,
        MatchAgentRequestLiveCheck = 0x1393,
        MatchAgentResponseLiveCheck = 0x1394

    }

}