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

        Hangman mHangmanInstance;


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

                Client client = new Client(socket, index);

                mClients.TryAdd(index, client);



                Console.WriteLine("Accepted Connection");

                Thread tcpThread = new Thread(() => { ClientMethod(index); });

                tcpThread.Start();
            }

        }

        //Stops the server
        public void Stop()
        {
            mTcpListener.Stop();
            Console.WriteLine("Closed Connection");
        }
        //sends a packet to all clients except the passed in one
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
        //sends a packet to all clients
        public void TcpSendToAll(Packet packet)
        {
            foreach (Client cli in mClients.Values)
            {
                cli.TcpSend(packet);
            }
        }
        //Sends a private message packet to the reciever
        private void TcpSendPrivatePacket(Packet packet)
        {
            EncryptedPrivateMessagePacket privateMessagePacket = (EncryptedPrivateMessagePacket)packet;

            string reciever  = System.Text.Encoding.UTF8.GetString(privateMessagePacket.mReciever);

            foreach (Client cli in mClients.Values)
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
            try
            {
                while ((recievedPacket = currentClient.TcpRead()) != null)
                {

                    switch (recievedPacket.mPacketType)
                    {
                        case PacketType.Disconnect:
                            //Sends the disconnect message to other clients 
                            DisconnectPacket disconnectPacket = (DisconnectPacket)recievedPacket;
                            TcpSendToOthers(currentClient, disconnectPacket);

                            List<string> clientNames = new List<string>();

                            //Updates the clients old nickname to there new one
                            foreach (Client cli in mClients.Values)
                            {
                                if (cli.Nickname == disconnectPacket.mNickname)
                                {
                                    Client cliendToRemove;

                                    mClients.TryRemove(cli.Index, out cliendToRemove);

                                    continue;
                                }

                                clientNames.Add(cli.Nickname);
                            }

                            //Updates client list
                            UpdateClientList(clientNames);
                            break;
                        case PacketType.NewNickname:
                            SetNicknamePacket setNicknamePacket = (SetNicknamePacket)recievedPacket;

                            List<string> names = new List<string>();

                            //Updates the clients old nickname to there new one
                            foreach (Client cli in mClients.Values)
                            {
                                if (cli.Nickname == setNicknamePacket.mOldNickname)
                                {
                                    cli.Nickname = setNicknamePacket.mNewNickname;
                                }

                                names.Add(cli.Nickname);
                            }

                            //Updates client list
                            UpdateClientList(names);

                            break;
                        case PacketType.EncryptedPrivateMessage:
                            //Sends the private message to its reciever
                            EncryptedPrivateMessagePacket privateMessagePacket = (EncryptedPrivateMessagePacket)recievedPacket;
                            TcpSendPrivatePacket(privateMessagePacket);

                            break;
                        case PacketType.Empty:
                            break;
                        case PacketType.EncryptedMessage:
                            //Sends the chat message to other clients
                            EncryptedChatMessage encryptedChatMessage = (EncryptedChatMessage)recievedPacket;
                            TcpSendToOthers(currentClient, encryptedChatMessage);
                            break;
                        case PacketType.PlayHangman:
                            StartHangmanPacket startHangmanPacket = (StartHangmanPacket)recievedPacket;
                            //Send the packet to other clients
                            TcpSendToOthers(currentClient, startHangmanPacket);

                            //Create a new hangman instance
                            mHangmanInstance = new Hangman(this);

                            break;


                        case PacketType.HangmanLetterGuess:
                            HangmanGuessPacket hangmanGuessPacket = (HangmanGuessPacket)recievedPacket;

                            //Safety check
                            if (mHangmanInstance != null)
                            {
                                //Sends the guess to other clients
                                TcpSendToOthers(currentClient, hangmanGuessPacket);

                                //Passes the guess to the hangman object
                                mHangmanInstance.TakeGuess(hangmanGuessPacket.mGuess);
                                if (mHangmanInstance.GameOver)
                                {
                                    //if the game is now over set hangman instance to null
                                    mHangmanInstance = null;
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
            catch(Exception e)
            {

            }
            finally
            {
                //Closes connection
                Console.WriteLine("Closing Connection");
                mClients[index].Close();

                Client c;

                //Removes the client
                mClients.TryRemove(index, out c);

                if (mClients.Count == 0)
                {
                    Stop();
                }
            }
            


        }

        //This function sorts the names list alphabetically and then sends the new client list to all clients
        private void UpdateClientList(List<string> names)
        {
            names.Sort();
            NicknameWindowPacket nicknameWindowPacket = new NicknameWindowPacket(names);
            TcpSendToAll(nicknameWindowPacket);
        }
    }
}
