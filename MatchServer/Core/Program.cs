using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MatchServer.Network;
using MatchServer.Packet;
using MatchServer.Packet.Handle;
namespace MatchServer.Core
{
    class Program
    {
        [DllImport("WINMM")]
        public static extern Int32 timeGetTime();

        public static List<Item> mItems;
        public static List<UInt32> mShop;
        public static List<MMatchChannel> mChannels;

        public static System.Text.RegularExpressions.Regex mRegex = new System.Text.RegularExpressions.Regex("[a-zA-Z0-9]{3,16}");
        static void Main(string[] args)
        {
            mItems = new List<Item>();
            mShop = new List<uint>();
            mChannels = new List<MMatchChannel>();
                
            Console.WindowWidth = Console.BufferWidth = 120;
            Console.Title = "Match Server";
            Database.Initialize();
            PacketMgr.InitializeHandlers<Match>();
            PacketMgr.InitializeHandlers<Channel>();
            LoadItems();
            LoadChannels();
            TCPServer.Initialize();

            Log.Write("Loaded: {0} channels.", mChannels.Count);
            Log.Write("Loaded: {0} items.", mItems.Count);

            while (TCPServer.IsRunning())
            {
                System.Threading.Thread.Sleep(1);
            }
        } 

        private static void LoadItems()
        {
            XmlReader reader = new XmlTextReader("zitem.xml");
            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "ITEM":
                        Item item = new Item();
                        item.nItemID = Int32.Parse(reader.GetAttribute("id"));
                        item.nLevel = (byte)Int32.Parse(reader.GetAttribute("res_level"));
                        item.nWeight = Int32.Parse(reader.GetAttribute("weight"));
                        item.nMaxWT = reader.GetAttribute("maxwt") == null ? 0 : Int32.Parse(reader.GetAttribute("maxwt"));
                        item.nPrice = reader.GetAttribute("bt_price") == null ? 0 : Int32.Parse(reader.GetAttribute("bt_price"));
                        mItems.Add(item);
                        break;
                }
            }

            reader = new XmlTextReader("shop.xml");
            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "SELL":
                        mShop.Add(UInt32.Parse(reader.GetAttribute("itemid")));
                        break;
                }
            }
        }

        private static void LoadChannels()
        {
            XmlReader reader = new XmlTextReader("channel.xml");
            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "CHANNEL":
                        MMatchChannel channel = new MMatchChannel();
                        channel.szName = reader.GetAttribute("name");
                        if (!Int32.TryParse(reader.GetAttribute("levelmin"), out channel.nMinLevel))
                            channel.nMinLevel = 0;
                        channel.nMaxUsers = Int32.Parse(reader.GetAttribute("maxplayers"));
                        channel.uidChannel = Convert.ToUInt64(mChannels.Count);
                        channel.nChannelType = MMatchChannelType.General;
                        switch (reader.GetAttribute("rule"))
                        {
                            case "elite":
                                channel.nChannelRule = MMatchChannelRule.Elite;
                                break;
                        }
                        mChannels.Add(channel);
                        break;
                }
            }
        }
    }
}
