using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPchat_BASE
{
    public class Server
    {
        private TcpListener _tcpListener;
        private readonly int _packetSize = MessageObj.MaxBytes;

        private int _clientCount;
        private readonly int _maxClientCount;
        private Dictionary<IPAddress, Client> _clients;
        private readonly object _token = new object();


        public bool Running { get; set; }

        public int Port
        {
            get { return ((IPEndPoint)_tcpListener.Server.LocalEndPoint).Port; }
        }

        public Dictionary<IPAddress, Client> Clients
        {
            get { return _clients; }
        }

        public Server(int port, int maxClientCount)
        {
            _clientCount = 0;
            _maxClientCount = maxClientCount;
            _clients = new Dictionary<IPAddress, Client>(_maxClientCount);

            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, port);
            }
            catch (Exception e)
            {
                CommandLine.Write(e.Message);
            }
        }

        public void StartListen()
        {
            try
            {
                Running = true;
                _tcpListener.Start();
                CommandLine.Write("Started listening at port " + Port + ".");
            }
            catch (Exception e)
            {
                CommandLine.Write(e.Message);
            }

            while (Running)
            {
                lock (_token)
                {
                    if (_clientCount >= _maxClientCount) { continue; }
                }

                var newClient = _tcpListener.AcceptTcpClient();
                AddClient(newClient);
            }
        }

        public void StopListen()
        {
            try
            {
                Running = false;
                _tcpListener.Stop();
                CommandLine.Write("Stopped listening at port " + Port + ".");
            }
            catch (Exception e)
            {
                CommandLine.Write(e.Message);
            }
        }

        public void Send(Client client, string data)
        {
            if (client == null || !client.Connected) return;

            var msg = MessageObj.SerializeMessage.SerializeMSG(MessageObj.WriteMessage(data));

            try
            {
                byte[] debg = BitConverter.GetBytes(msg.Length);
                client.Stream.Write(debg, 0, 4);
                client.Stream.Write(msg, 0, msg.Length);
            }
            catch (Exception e)
            {
                CommandLine.Write(e.Message);
            }
        }

        public void SendAll(string data)
        {
            foreach (var entry in _clients) { Send(entry.Value, data); }
        }

        private void ShareMsg(MessageObj data, Client client)
        {
            data.Ip = client.IP;
            foreach (var entry in _clients) 
            { 
                if(!entry.Key.Equals(client.IP))
                Send(entry.Value, data.Msg); 
            }
        }

        private void AddClient(TcpClient newClient)
        {
            if (newClient == null) return;

            var client = new Client(newClient);
            _clients.Add(client.IP, client);
            IncreaseClientCount();

            var clientThread = new Thread(HandleClient) { IsBackground = true };
            clientThread.Start(client);

            CommandLine.Write("A new client connected. Client count is " + _clientCount + ".");
        }

        private void RemoveClient(Client client)
        {
            if (client == null) return;

            _clients.Remove(client.IP);
            DecreaseClientCount();

            client.Close();
        }

        private void HandleClient(object newClient)
        {
            var client = (Client)newClient;
            var currentMessage = new List<byte>();

            while (true)
            {
                var readMessage = new byte[_packetSize];
                int readMessageSize;

                try
                {
                    var readint = new byte[4];
                    client.Stream.Read(readint, 0, readint.Length);
                    readMessageSize = client.Stream.Read(readMessage, 0, BitConverter.ToInt32(readint, 0));
                }
                catch (Exception e)
                {
                    CommandLine.Write(e.Message);
                    break;
                }

                if (readMessageSize <= 0)
                {
                    CommandLine.Write("The client [" + client.IP + "] has closed the connection.");
                    break;
                }
                for (int i = 0; i < readMessageSize; i++)
                {
                    currentMessage.Add(readMessage[i]);
                }
                OnDataReceive(currentMessage.ToArray(), client);
            }

            CommandLine.Write("Communication ended with client [" + client.IP + "].");
            RemoveClient(client);
        }

        private void IncreaseClientCount()
        {
            lock (_token) { _clientCount++; }
        }

        private void DecreaseClientCount()
        {
            lock (_token) { _clientCount--; }
        }
        public void OnDataReceive(byte[] data, Client client)
        {
            //String message = new ASCIIEncoding().GetString(data);
            MessageObj msg = MessageObj.SerializeMessage.DeSerializeMSG(data);
            CommandLine.Write(msg.From + " Say:" + msg.Msg);
            ShareMsg(msg, client);
        }
    }
}
