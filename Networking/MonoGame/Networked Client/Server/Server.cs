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

        private ConcurrentDictionary<int, Client> mClients;

        private UdpClient mUdpListener;

        static int playerCount = 0;

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

                playerCount++;

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

                    MemoryStream stream = new MemoryStream(buffer);

                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    Packet recievedPackage = binaryFormatter.Deserialize(stream) as Packet;

                    foreach (Client c in mClients.Values)
                    {
                        if (endPoint.ToString() == c.mIpEndPoint.ToString())
                        {
                            switch (recievedPackage.mPacketType)
                            {
                                case PacketType.Empty:
                                    break;
                                case PacketType.Connect:
                                    break;
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


        private void ClientMethod(int index)
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
                        currentClient.mIpEndPoint = loginPacket.mEndPoint;
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
