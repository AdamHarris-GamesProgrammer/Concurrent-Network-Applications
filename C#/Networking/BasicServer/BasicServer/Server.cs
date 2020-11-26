using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using Packets;
using System.Collections.Generic;

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

        private void SendToOthers(Client currentClient, Packet packet)
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

        private void SendToAll(Packet packet)
        {
            foreach (Client cli in mClients.Values)
            {
                cli.Send(packet);
            }
        }

        private void ClientMethod(int index)
        {
            Packet recievedMessage;

            Client currentClient = mClients[index];

            while ((recievedMessage = currentClient.Read()) != null)
            {

                switch (recievedMessage.mPacketType)
                {
                    case PacketType.ChatMessage:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                        SendToOthers(currentClient, chatPacket);
                        break;
                    case PacketType.Disconnect:
                        DisconnectPacket disconnectPacket = (DisconnectPacket)recievedMessage;
                        SendToOthers(currentClient, disconnectPacket);
                        break;
                    case PacketType.NewNickname:
                        SetNicknamePacket setNicknamePacket = (SetNicknamePacket)recievedMessage;

                        List<string> names = new List<string>();

                        foreach (Client cli in mClients.Values)
                        {
                            if (cli.Nickname == setNicknamePacket.mOldNickname)
                            {
                                cli.Nickname = setNicknamePacket.mNewNickname;
                            }

                            names.Add(cli.Nickname);
                        }

                        UpdateClientList(names);

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

        private void UpdateClientList(List<string> names)
        {
            names.Sort();
            NicknameWindowPacket nicknameWindowPacket = new NicknameWindowPacket(names);
            SendToAll(nicknameWindowPacket);
        }

    }
}
