using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using Packets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
    class Server
    {
        private TcpListener mTcpListener;

        private ConcurrentDictionary<string, Client> mClients;

        private UdpClient mUdpListener;

        static int playerCount = 0;

        string latestPlayer;



        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            mTcpListener = new TcpListener(ip, port);

            mUdpListener = new UdpClient(port);
        }

        public void Start()
        {
            mClients = new ConcurrentDictionary<string, Client>();

            mTcpListener.Start();

            int clientIndex = 0;

            while (true)
            {
                int index = clientIndex;
                clientIndex++;

                Console.WriteLine("Awaiting Connection");

                Socket socket = mTcpListener.AcceptSocket();

                Client client = new Client(socket);

                latestPlayer = GenerateUID();

                mClients.TryAdd(latestPlayer, client);



                Console.WriteLine("Accepted Connection");

                playerCount++;

                Thread tcpThread = new Thread(() => { ClientMethod(latestPlayer); });
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

                    MemoryStream stream = new MemoryStream(buffer);

                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    Packet recievedPackage = binaryFormatter.Deserialize(stream) as Packet;

                    foreach (Client c in mClients.Values)
                    {
                        if (endPoint.ToString() == c.mIpEndPoint.ToString())
                        {
                            switch (recievedPackage.mPacketType)
                            {
                                case PacketType.Position:
                                    
                                    break;
                                
                                default:
                                    break;

                            }
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
        }


        private void ClientMethod(string index)
        {
            Packet recievedPacket;

            Client currentClient = mClients[index];

            while ((recievedPacket = currentClient.TcpRead()) != null)
            {

                switch (recievedPacket.mPacketType)
                {
                    case PacketType.Connect:

                        break;
                    case PacketType.Login:
                        LoginPacket loginPacket = (LoginPacket)recievedPacket;
                        currentClient.mIpEndPoint = IPEndPoint.Parse(loginPacket.mEndPoint);

                        GUID guidPacket = new GUID(latestPlayer);
                        currentClient.TcpSend(guidPacket);
                        Players playersPacket = new Players(mClients.Keys);
                        TcpSendToAll(playersPacket);

                        NewPlayer newPlayer = new NewPlayer(latestPlayer);
                        TcpSendToOthers(currentClient, newPlayer);

                        break;

                    case PacketType.Position:
                        PositionPacket positionPacket = (PositionPacket)recievedPacket;
                        TcpSendToAll(positionPacket);

                        break;
                    case PacketType.Disconnect:
                        DisconnectPacket disconnectPacket = (DisconnectPacket)recievedPacket;
                        TcpSendToAll(disconnectPacket);
                        mClients.TryRemove(disconnectPacket.mId, out currentClient);

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

        private string GenerateUID()
        {
            Guid g = Guid.NewGuid();

            return g.ToString();
        }
    }
}
