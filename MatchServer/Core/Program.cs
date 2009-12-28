using System;
using System.Text;

using MatchServer.Network;
using MatchServer.Packet;
using MatchServer.Packet.Handle;
namespace MatchServer.Core
{
    class Program
    {
        public static System.Text.RegularExpressions.Regex mRegex = new System.Text.RegularExpressions.Regex("[a-zA-Z0-9]{3,16}");
        static void Main(string[] args)
        {
            Console.WindowWidth = Console.BufferWidth = 120;
            Console.Title = "Match Server";
            Database.Initialize();
            PacketMgr.InitializeHandlers<Match>();
            TCPServer.Initialize();

            while (TCPServer.IsRunning())
            {
                System.Threading.Thread.Sleep(1);
            }
        }
    }
}
