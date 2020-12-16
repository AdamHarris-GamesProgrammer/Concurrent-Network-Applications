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

        //Holds the latest players name for use in login and uid packets
        string mLatestPlayer;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            mTcpListener = new TcpListener(ip, port);

            mUdpListener = new UdpClient(port);
        }

        /// <summary>
        /// Server start method. 
        /// </summary>
        public void Start()
        {
            //Initializes the clients dictionary
            mClients = new ConcurrentDictionary<string, Client>();

            //Starts the TCP listener
            mTcpListener.Start();

            while (true)
            {
                Console.WriteLine("Awaiting Connection");

                //Blocking call, this method does not continue until a socket has been accepted
                Socket socket = mTcpListener.AcceptSocket();

                //Creates a new client object when a client instance connects to the server
                Client client = new Client(socket);

                //Generates a unique ID for each new player
                mLatestPlayer = GenerateUID();

                //Adds the new client to the dictionary using there new UID
                mClients.TryAdd(mLatestPlayer, client);


                Console.WriteLine("Accepted Connection");


                //Starts the tcp and udp thread for each new client
                Thread tcpThread = new Thread(() => { TCPClientMethod(mLatestPlayer); });
                Thread udpThread = new Thread(UdpListen);

                tcpThread.Start();
                udpThread.Start();
            }

        }

        /// <summary>
        /// Listens for any packets from the clients that are sent by UDP
        /// </summary>
        private void UdpListen()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    //Gets the byte buffer from the received packet
                    byte[] buffer = mUdpListener.Receive(ref endPoint);

                    MemoryStream stream = new MemoryStream(buffer);

                    //Deserializes the stream as a packet for processing
                    BinaryFormatter formatter = new BinaryFormatter();
                    Packet recievedPackage = formatter.Deserialize(stream) as Packet;

                    switch (recievedPackage.mPacketType)
                    {
                        case PacketType.Velocity:
                            VelocityPacket vp = (VelocityPacket)recievedPackage;

                            Console.WriteLine("Recieved Velocity Package from {0}: X: {1}, Y: {2}", vp.mId, vp.xVel, vp.yVal);

                            break;
                        case PacketType.Position:
                            PositionPacket pp = (PositionPacket)recievedPackage;

                            Console.WriteLine("Recieved Position Package from {0}: X: {1}, Y: {2}", pp.mId, pp.xPos, pp.yPos);
                            break;
                    }

                    //Loops through each client and sends the packet
                    foreach (Client c in mClients.Values)
                    {
                        //Safety check that the clients ip end point has been set
                        if (c.mIpEndPoint != null && endPoint != c.mIpEndPoint)
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

        /// <summary>
        /// This function stops the server from listening for any further packets
        /// </summary>
        public void Stop()
        {
            mTcpListener.Stop();
            Console.WriteLine("Closed Connection");
        }

        /// <summary>
        /// Method for handling any TCP packets the server received
        /// </summary>
        private void TCPClientMethod(string index)
        {
            Packet receivedPacket;

            Client currentClient = mClients[index];

            //Continues until the client can no longer read data
            while ((receivedPacket = currentClient.TcpRead()) != null)
            {
                //Decides what to do based on the packet type
                switch (receivedPacket.mPacketType)
                {
                    case PacketType.Connect:
                        //Sends the players Unique ID to the new client
                        GUID guidPacket = new GUID(mLatestPlayer);
                        currentClient.TcpSend(guidPacket);
                        break;
                    case PacketType.Login:
                        //Sets the clients ip end point for processing the packets via UDP
                        LoginPacket loginPacket = (LoginPacket)receivedPacket;
                        currentClient.mIpEndPoint = IPEndPoint.Parse(loginPacket.mEndPoint);

                        //Sends the client list's keys to all clients so they can update it
                        Players playersPacket = new Players(mClients.Keys);
                        TcpSendToAll(playersPacket);

                        //Sends the new players information to all other clients 
                        NewPlayer newPlayer = new NewPlayer(mLatestPlayer);
                        TcpSendToOthers(currentClient, newPlayer);

                        break;
                    case PacketType.Disconnect:
                        //Sends a disconnect packet to all clients, this will cause each client to remove the disconnected player
                        DisconnectPacket disconnectPacket = (DisconnectPacket)receivedPacket;
                        TcpSendToAll(disconnectPacket);
                        mClients.TryRemove(disconnectPacket.mId, out currentClient);

                        break;
                    default:
                        break;
                }
            }

            Console.WriteLine("Closing Connection");
            mClients[index].Close();

            //Temporary client object
            Client c;

            //Removes the client from the clients list
            mClients.TryRemove(index, out c);

            //Stops the server if the client count is now 0
            if (mClients.Count == 0)
            {
                Stop();
            }
        }

        /// <summary>
        /// Sends a packet over TCP to all clients except the passed in client
        /// </summary>
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

        /// <summary>
        /// Sends a packet via TCP to all clients
        /// </summary>
        private void TcpSendToAll(Packet packet)
        {
            foreach (Client cli in mClients.Values)
            {
                cli.TcpSend(packet);
            }
        }

        /// <summary>
        /// Generates a Unique ID for each ball
        /// </summary>
        private string GenerateUID()
        {
            Guid g = Guid.NewGuid();

            return g.ToString();
        }
    }
}
