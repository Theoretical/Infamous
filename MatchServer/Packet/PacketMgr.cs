    using System;
using System.Collections.Generic;
using System.Reflection;

using MatchServer.Core;
namespace MatchServer.Packet
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    class PacketHandlerAttribute : Attribute
    {

        public PacketHandlerAttribute(Operation pOpcode, PacketFlags pFlags)
        {
            Opcode = pOpcode;
            flags = pFlags;
        }
        public Operation Opcode
        {
            get { return operation; }
            set { operation = value; }
        }

        public PacketFlags Flag
        {
            get { return flags; }
            set { flags = value; }
        }
        private Operation operation;
        private PacketFlags flags;
    }

    class PacketMgr
    {
        public static Dictionary<Operation, HandlerDelegate> mOpcodes = new Dictionary<Operation, HandlerDelegate>();

        // <summary>
        // Loads the packet handles of class T1.
        // </summary>
        public static void InitializeHandlers<T1>()
        {
            var methods = typeof(T1).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false);

                if (attributes.Length != 1)
                    continue;

                var attribute = (PacketHandlerAttribute)attributes[0];
                if (mOpcodes.ContainsKey(attribute.Opcode))
                    continue;
                mOpcodes.Add(attribute.Opcode, new HandlerDelegate(new HandlerDelegate.PacketProcessor((Action<Client, PacketReader>)Delegate.CreateDelegate(typeof(Action<Client,PacketReader>), method)), attribute.Flag));
                Log.Write("Registered Opcode: {0} to method: {1}", attribute.Opcode, method.Name);
            }
        }
    }
}
