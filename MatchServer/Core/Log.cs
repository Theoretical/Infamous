using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace MatchServer.Core
{
    class Log
    {
        private static TextWriter m_textWriter = Console.Out;
        private static StreamWriter m_streamWriter = new StreamWriter("Envy Log.txt", true);

        public static void Write(string szFormat, params object[] pParams)
        {
            string Final = string.Format("[{0}] - {1} - ", DateTime.Now, new StackTrace().GetFrame(1).GetMethod().Name);
            lock (m_textWriter)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                m_textWriter.Write(Final);
                Console.ForegroundColor = ConsoleColor.Gray;
                m_textWriter.WriteLine(szFormat, pParams);
            }
            lock (m_streamWriter)
            {
                m_streamWriter.Write(Final);
                m_streamWriter.WriteLine(szFormat, pParams);
            }
        }

        public static void PacketLog(byte[] data, int index, int length)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string sDump = (length > 0 ? BitConverter.ToString(data, index, length) : "");
            string[] sDumpHex = sDump.Split('-');
            List<string> lstDump = new List<string>();
            string sHex = "";
            string sAscii = "";
            char cByte = '\0';
            if (sDump.Length > 0)
            {
                for (Int32 iCount = 0; iCount < sDumpHex.Length; iCount++)
                {
                    cByte = Convert.ToChar(data[index + iCount]);
                    sHex += sDumpHex[iCount] + ' ';
                    if (char.IsWhiteSpace(cByte) || char.IsControl(cByte))
                    {
                        cByte = '.';
                    }
                    if (cByte == '{' || cByte == '}')
                        cByte = '.';
                    sAscii += cByte.ToString();
                    if ((iCount + 1) % 16 == 0)
                    {
                        lstDump.Add(sHex + " " + sAscii);
                        sHex = "";
                        sAscii = "";
                    }
                }
                if (sHex.Length > 0)
                {
                    if (sHex.Length < (16 * 3)) sHex += new string(' ', (16 * 3) - sHex.Length);
                    lstDump.Add(sHex + " " + sAscii);
                }
            }
            for (Int32 iCount = 0, j = 0; iCount < lstDump.Count; iCount++, j++)
            {
                Console.WriteLine(lstDump[iCount]);
            }
        }
    }
}
