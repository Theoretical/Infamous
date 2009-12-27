using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using MatchServer.Network;
using MatchServer.Packet;
using MatchServer.Packet.Handle;
namespace MatchServer.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = Console.BufferWidth = 120;
            Console.Title = "MatchServer";
            PacketMgr.InitializeHandlers<Match>();
            TCPServer.Initialize();

            while (TCPServer.IsRunning())
            {
                System.Threading.Thread.Sleep(1);
            }
        }
    }
}
