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

            while (true)
            {
                Console.WriteLine("Awaiting Connection");

                Socket socket = mTcpListener.AcceptSocket();

                Client client = new Client(socket);

                latestPlayer = GenerateUID();

                mClients.TryAdd(latestPlayer, client);



                Console.WriteLine("Accepted Connection");

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

                    foreach (Client c in mClients.Values)
                    {
                        if(c.mIpEndPoint != null )
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

        private void ClientMethod(string index)
        {
            Packet recievedPacket;

            Client currentClient = mClients[index];

            while ((recievedPacket = currentClient.TcpRead()) != null)
            {

                switch (recievedPacket.mPacketType)
                {
                    case PacketType.Connect:
                        GUID guidPacket = new GUID(latestPlayer);
                        currentClient.TcpSend(guidPacket);
                        break;
                    case PacketType.Login:
                        LoginPacket loginPacket = (LoginPacket)recievedPacket;
                        currentClient.mIpEndPoint = IPEndPoint.Parse(loginPacket.mEndPoint);

                        Players playersPacket = new Players(mClients.Keys);
                        TcpSendToAll(playersPacket);

                        NewPlayer newPlayer = new NewPlayer(latestPlayer);
                        TcpSendToOthers(currentClient, newPlayer);

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
