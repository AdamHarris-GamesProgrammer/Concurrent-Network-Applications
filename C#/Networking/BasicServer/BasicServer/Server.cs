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

        private ConcurrentDictionary<int, Client> mClients;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            mTcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            mClients = new ConcurrentDictionary<int, Client>();

            mTcpListener.Start();

            int clientIndex = 0;

            while (true)
            {
                int index = clientIndex;
                clientIndex++;

                Console.WriteLine("Awaiting Connection");

                Socket socket = mTcpListener.AcceptSocket();

                Client client = new Client(socket);

                mClients.TryAdd(index, client);



                Console.WriteLine("Accepted Connection");

                Thread thread = new Thread(() => { ClientMethod(index); });

                thread.Start();
            }

        }

        public void Stop()
        {
            mTcpListener.Stop();
            Console.WriteLine("Closed Connection");
        }

        private void ClientMethod(int index)
        {
            Packet recievedMessage;

            Client currentClient = mClients[index];

            while ((recievedMessage = currentClient.Read()) != null)
            {
                
                switch (recievedMessage.packetType)
                {

                    case PacketType.ChatMessage:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                        foreach (Client cli in mClients.Values) { 
                            if (cli != currentClient)
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
                        foreach (Client cli in mClients.Values)
                        {
                            if (cli != currentClient)
                            {
                                cli.Send(nicknamePacket);
                            }
                        }
                        break;
                    case PacketType.Disconnect:
                        DisconnectPacket disconnectPacket = (DisconnectPacket)recievedMessage;
                        foreach (Client cli in mClients.Values)
                        {
                            if (cli != currentClient)
                            {
                                cli.Send(disconnectPacket);
                            }
                        }
                        break;

                }


            }

            Console.WriteLine("Closing Connection");
            mClients[index].Close();

            Client c;
            mClients.TryRemove(index, out c);

            if(mClients.Count == 0)
            {
                Stop();
            }
        }
    }
}
