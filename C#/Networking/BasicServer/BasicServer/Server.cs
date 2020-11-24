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


        struct ClientInformation
        {
            public Client client;
        }

        private ConcurrentDictionary<int, ClientInformation> mClients;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            mTcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            mClients = new ConcurrentDictionary<int, ClientInformation>();

            mTcpListener.Start();

            int clientIndex = 0;

            while (true)
            {
                int index = clientIndex;
                clientIndex++;

                Console.WriteLine("Awaiting Connection");

                Socket socket = mTcpListener.AcceptSocket();

                Client client = new Client(socket);

                ClientInformation clientInformation = new ClientInformation();
                clientInformation.client = client;

                mClients.TryAdd(index, clientInformation);



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

            Client currentClient = mClients[index].client;

            while ((recievedMessage = currentClient.Read()) != null)
            {

                switch (recievedMessage.packetType)
                {
                    case PacketType.ChatMessage:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                        foreach (ClientInformation cli in mClients.Values)
                        {
                            if (cli.client != currentClient)
                            {
                                cli.client.Send(chatPacket);
                            }
                        }
                        break;

                    case PacketType.Nickname:
                        NicknamePacket nicknamePacket = (NicknamePacket)recievedMessage;
                        foreach (ClientInformation cli in mClients.Values)
                        {
                            if (cli.client != currentClient)
                            {
                                cli.client.Send(nicknamePacket);
                            }
                        }
                        break;
                    case PacketType.Disconnect:
                        DisconnectPacket disconnectPacket = (DisconnectPacket)recievedMessage;
                        foreach (ClientInformation cli in mClients.Values)
                        {
                            if (cli.client != currentClient)
                            {
                                cli.client.Send(disconnectPacket);
                            }
                        }
                        break;
                    case PacketType.NewNickname:
                        SetNicknamePacket setNicknamePacket = (SetNicknamePacket)recievedMessage;
                        foreach (ClientInformation cli in mClients.Values)
                        {
                            if (cli.client.GetNickname() == setNicknamePacket.oldNickname)
                            {
                                cli.client.SetNickname(setNicknamePacket.newNickname);
                            }
                        }
                        break;
                    case PacketType.PrivateMessage:
                        break;
                    case PacketType.ClientName:
                        break;
                    case PacketType.Empty:
                        break;
                    default:
                        break;
                }


            }

            Console.WriteLine("Closing Connection");
            mClients[index].client.Close();

            ClientInformation c;

            mClients.TryRemove(index, out c);

            if (mClients.Count == 0)
            {
                Stop();
            }
        }
    }
}
