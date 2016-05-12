using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPchat_BASE;

namespace TCP_ChatConsole
{
    public class Program
    {
        private static Server _server;
        private static Client _client;

        public static void Main()
        {
            while (true)
            {
                string[] input = Console.ReadLine().Split(" ".ToArray<char>());

                switch (input[0])
                {
                    case "-srv":
                        {
                            if (_client != null) return;

                            Console.Title = "Server";

                            _server = new Server(15150, 3);
                            new Thread(_server.StartListen).Start();
                        }
                        break;
                    case "-clients":
                        {
                            if (_server == null) return;

                            int counter = 0;

                            foreach (var entry in _server.Clients)
                            {
                                if (entry.Value == null) return;
                                CommandLine.Write(++counter + "- " + entry.Key + "\n");
                            }
                        }
                        break;
                    case "-connect":
                        {
                            //if (_server != null) return;
                            Console.Title = "Client";

                            var client = new TcpClient();
                            //ip adress below will be taken by user input after tests.
                            var serverEndPoint = new IPEndPoint(IPAddress.Parse(input[1]), 15150);
                            client.Connect(serverEndPoint);
                            CommandLine.Write("Whats your number?");
                            _client = new Client(client, Int32.Parse(Console.ReadLine()));
                            new Thread(_client.Receive).Start();
                        }
                        break;
                    default:
                        {
                            if (_client != null) // if user is a client.
                            {
                                _client.Send(input[0]);
                            }
                            else if (_server != null) // if user is the server.
                            {
                                _server.SendAll(input[0]);
                            }
                        }
                        break;
                }
            }
        }
    }
}
