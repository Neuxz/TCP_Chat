using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPchat_BASE
{
    public class Client
    {
        private TcpClient _client;
        private readonly int _packetSize = 64;
        public NetworkStream Stream
        {
            get { return _client.GetStream(); }
        }

        public IPAddress IP
        {
            get { return ((IPEndPoint)_client.Client.RemoteEndPoint).Address; }
        }

        public bool Connected
        {
            get { return _client.Connected; }
        }

        public Client(TcpClient client)
        {
            _client = client;
        }
        public Client(TcpClient client, int name)
        {
            _client = client;
            MessageObj.Changenm(name);
        }

        public void Send(string data)
        {
            MessageObj debug = MessageObj.WriteMessage(data);
            var msg = MessageObj.SerializeMessage.SerializeMSG(debug);
            MessageObj debug2 = MessageObj.SerializeMessage.DeSerializeMSG(msg);
            try
            {
                byte[] debg = BitConverter.GetBytes(msg.Length);
                Stream.Write(debg, 0, 4);
                Stream.Write(msg, 0, msg.Length);
            }
            catch (Exception e)
            {
                CommandLine.Write(e.Message);
            }
        }

        public void Receive()
        {
            var currentMessage = new List<byte>();

            while (true)
            {
                var readMessage = new byte[_packetSize];
                int readMessageSize;

                try
                {
                    var readint = new byte[4];
                    Stream.Read(readint, 0, readint.Length);
                    readMessageSize = Stream.Read(readMessage, 0, BitConverter.ToInt32(readint, 0));
                }
                catch (Exception e)
                {
                    CommandLine.Write(e.Message);
                    break;
                }

                if (readMessageSize <= 0) break;
                for (int i = 0; i < readMessageSize; i++)
                {
                    currentMessage.Add(readMessage[i]);
                }
                MessageObj msg = MessageObj.SerializeMessage.DeSerializeMSG(currentMessage.ToArray());
                CommandLine.Write(msg.From + " Say:" + msg.Msg);
                currentMessage.Clear();
            }
        }

        public void Close()
        {
            try
            {
                _client.Close();
                _client = null;
            }
            catch (Exception e)
            {
                CommandLine.Write(e.Message);
            }
        }
    }
}
