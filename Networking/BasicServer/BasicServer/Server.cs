using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using Packets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace BasicServer
{
    class Server
    {
        private TcpListener mTcpListener;

        private ConcurrentDictionary<int, Client> mClients;

        private UdpClient mUdpListener;




        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            mTcpListener = new TcpListener(ip, port);

            mUdpListener = new UdpClient(port);
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

                Thread tcpThread = new Thread(() => { ClientMethod(index); });
                Thread udpThread = new Thread(UdpListen);

                tcpThread.Start();
                udpThread.Start();
            }

        }

        private void UdpListen()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] buffer = mUdpListener.Receive(ref endPoint);

                    foreach(Client c in mClients.Values)
                    {
                        if(endPoint.ToString() != c.mIpEndPoint.ToString())
                        {
                            mUdpListener.Send(buffer, buffer.Length, c.mIpEndPoint);
                        }
                        
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client UDP Read Method Exception: " + e.Message);
            }
        }


        public void Stop()
        {
            mTcpListener.Stop();
            Console.WriteLine("Closed Connection");
        }

        private void TcpSendToOthers(Client currentClient, Packet packet)
        {
            foreach (Client cli in mClients.Values)
            {
                //Sends packet to all people who are not the current client, this is because the current client already has a local copy
                if (cli != currentClient)
                {
                    cli.TcpSend(packet);
                }
            }
        }

        private void TcpSendToAll(Packet packet)
        {
            foreach (Client cli in mClients.Values)
            {
                cli.TcpSend(packet);
            }
        }

        private void TcpSendToSelected(Packet packet)
        {
            PrivateMessagePacket privateMessagePacket = (PrivateMessagePacket)packet;

            string reciever = privateMessagePacket.mReciever;

            foreach(Client cli in mClients.Values)
            {
                if(cli.Nickname == reciever)
                {
                    cli.TcpSend(packet);
                    break;
                }
            }
        }

        private void ClientMethod(int index)
        {
            Packet recievedPacket;

            Client currentClient = mClients[index];

            while ((recievedPacket = currentClient.TcpRead()) != null)
            {

                switch (recievedPacket.mPacketType)
                {
                    case PacketType.ChatMessage:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)recievedPacket;
                        TcpSendToOthers(currentClient, chatPacket);
                        break;
                    case PacketType.Disconnect:
                        DisconnectPacket disconnectPacket = (DisconnectPacket)recievedPacket;
                        TcpSendToOthers(currentClient, disconnectPacket);
                        break;
                    case PacketType.NewNickname:
                        SetNicknamePacket setNicknamePacket = (SetNicknamePacket)recievedPacket;

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
                        PrivateMessagePacket privateMessagePacket = (PrivateMessagePacket)recievedPacket;
                        TcpSendToSelected(privateMessagePacket);

                        break;
                    case PacketType.Empty:
                        break;
                    case PacketType.Encrypted:
                        EncryptedMessage encryptedMessage = (EncryptedMessage)recievedPacket;
                        TcpSendToOthers(currentClient, encryptedMessage);
                        break;

                    case PacketType.Login:
                        LoginPacket loginPacket = (LoginPacket)recievedPacket;
                        currentClient.mIpEndPoint = IPEndPoint.Parse(loginPacket.mEndPoint);
                        currentClient.Login(loginPacket.mPublicKey);
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
            TcpSendToAll(nicknameWindowPacket);
        }




    }
}
