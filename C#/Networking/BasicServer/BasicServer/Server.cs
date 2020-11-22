using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.IO;
using System.Text.Unicode;
using System.Collections.Concurrent;
using System.Threading;
using Packets;

namespace BasicServer
{
    class Server
    {
        private TcpListener mTcpListener;

        private ConcurrentBag<Client> mClients;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            mTcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            mClients = new ConcurrentBag<Client>();

            mTcpListener.Start();

            while (mClients.Count != 4)
            {
                Console.WriteLine("Awaiting Connection");

                Socket socket = mTcpListener.AcceptSocket();

                Client client = new Client(socket);

                mClients.Add(client);



                Console.WriteLine("Accepted Connection");

                Thread thread = new Thread(() => { ClientMethod(client); });

                thread.Start();
            }

        }

        public void Stop()
        {
            mTcpListener.Stop();
            Console.WriteLine("Closed Connection");
        }

        private void ClientMethod(Client client)
        {
            Packet recievedMessage;

            while ((recievedMessage = client.Read()) != null)
            {
                switch (recievedMessage.packetType)
                {
                    case PacketType.ChatMessage:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                        foreach (Client cli in mClients)
                        {
                            if (cli != client)
                            {
                                cli.Send(chatPacket);
                            }
                        }
                        break;
                    case PacketType.PrivateMessage:
                        break;
                    case PacketType.ClientName:
                        break;
                    default:
                        break;
                    case PacketType.Empty:
                        break;
                    case PacketType.Nickname:
                        NicknamePacket nicknamePacket = (NicknamePacket)recievedMessage;
                        foreach (Client cli in mClients)
                        {
                            if (cli != client)
                            {
                                cli.Send(nicknamePacket);
                            }
                        }
                        break;

                }


            }

            Console.WriteLine("Closing Connection");
            client.Close();

            mClients.TryTake(out client);

            if(mClients.Count == 0)
            {
                Stop();
            }
        }
    }
}
