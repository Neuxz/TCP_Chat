using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPchat_BASE
{
    public class Message
    {
        public byte[] Data { get; private set; }

        public Message(byte[] data)
        {
            var wrappedData = new LinkedList<byte>(data);
            wrappedData.AddLast(4);
            Data = wrappedData.ToArray();
        }

        public Message(string data)
        {
            data += (char)4;
            Data = new ASCIIEncoding().GetBytes(data);
        }
    }

    public static class CommandLine
    {
        private static readonly object _token = new object();

        public static void Write(string text)
        {
            lock (_token)
            {
                Console.WriteLine(text);
            }
        }
    }
}
