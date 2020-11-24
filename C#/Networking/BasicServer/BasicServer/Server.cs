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

        private void SendPacket(Client currentClient, Packet packet)
        {
            foreach (Client cli in mClients.Values)
            {
                //Sends packet to all people who are not the current client, this is because the current client already has a local copy
                if (cli != currentClient)
                {
                    cli.Send(packet);
                }
            }
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
                        SendPacket(currentClient, chatPacket);
                        break;
                    case PacketType.Disconnect:
                        DisconnectPacket disconnectPacket = (DisconnectPacket)recievedMessage;
                        SendPacket(currentClient, disconnectPacket);
                        break;
                    case PacketType.NewNickname:
                        SetNicknamePacket setNicknamePacket = (SetNicknamePacket)recievedMessage;
                        foreach (Client cli in mClients.Values)
                        {
                            if (cli.GetNickname() == setNicknamePacket.oldNickname)
                            {
                                cli.SetNickname(setNicknamePacket.newNickname);
                            }
                        }
                        break;
                    case PacketType.PrivateMessage:
                        break;
                    case PacketType.Empty:
                        break;
                    default:
                        break;
                }
            }

            Console.WriteLine("Closing Connection");
            mClients[index].Close();

            Client c;

            mClients.TryRemove(index, out c);

            if (mClients.Count == 0)
            {
                Stop();
            }
        }
    }
}
