﻿using Packets;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace UserClient
{
    public class Client
    {
        private NetworkStream mStream;
        private BinaryWriter mWriter;
        private BinaryReader mReader;
        private BinaryFormatter mFormatter;
        private ClientForm mClientForm;

        private UdpClient mUdpClient;
        private TcpClient mTcpClient;

        private SolidColorBrush mServerColor;


        private RSACryptoServiceProvider mRSAProvider;
        private RSAParameters mPublicKey;
        private RSAParameters mPrivateKey;
        private RSAParameters mServerKey;

        bool mIsConnected = false;

        private string mNickname;
        public String Nickname
        {
            get
            {
                if (mNickname == null) return "Username";
                return mNickname;
            }
            set
            {
                SetNickname(value);
                mNickname = value;
            }
        }



        private void SetNickname(string name)
        {
                SetNicknamePacket setNicknamePacket = new SetNicknamePacket(Nickname, name);
                SerializePacket(setNicknamePacket);

                if (mClientForm != null) mClientForm.SetWindowTitle(Nickname);
        }

        public Client()
        {
            mTcpClient = new TcpClient();

            mServerColor = Brushes.LightGray;
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                mTcpClient.Connect(ipAddress, port);
                mStream = mTcpClient.GetStream();
                mWriter = new BinaryWriter(mStream, Encoding.UTF8);
                mReader = new BinaryReader(mStream, Encoding.UTF8);
                mFormatter = new BinaryFormatter();

                mUdpClient = new UdpClient();
                mUdpClient.Connect(ipAddress, port);

                mIsConnected = true;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public void Run()
        {
            mClientForm = new ClientForm(this);

            Thread tcpThread = new Thread(TcpProcessServerResponse);
            Thread udpThread = new Thread(UdpProcessServerResponse);


            mClientForm.SetWindowTitle(Nickname);


            Login();

            tcpThread.Start();
            udpThread.Start();


            mClientForm.ShowDialog();


            DisconnectFromServer();
        }

        public void Login()
        {
            LoginPacket loginPacket = new LoginPacket(mUdpClient.Client.LocalEndPoint.ToString(), mPublicKey);
            SerializePacket(loginPacket);

        }

        public void UdpSendMessage(Packet packet)
        {
            MemoryStream msgStream = new MemoryStream();
            mFormatter.Serialize(msgStream, packet);
            byte[] buffer = msgStream.GetBuffer();
            mUdpClient.Send(buffer, buffer.Length);
        }

        public void UdpProcessServerResponse()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] buffer = mUdpClient.Receive(ref endPoint);

                    MemoryStream stream = new MemoryStream(buffer);

                    Packet recievedPackage = mFormatter.Deserialize(stream) as Packet;
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine("Client UDP Read Method Exception: " + e.Message);
            }
        }


        public void DisconnectFromServer()
        {
            if (!mIsConnected) return;
            mIsConnected = false;
            mClientForm.DisconnectMessage(Nickname);

            DisconnectPacket disconnectPacket = new DisconnectPacket(Nickname);
            SerializePacket(disconnectPacket);
            
        }

        public void SendMessage(string message)
        {
            if (message.Contains("-guess"))
            {
                //Updates local chat window
                mClientForm.SendNicknameToWindow("You", System.Windows.HorizontalAlignment.Right);
                mClientForm.SendMessageToWindow(message.ToString(), System.Windows.HorizontalAlignment.Right);

                HangmanGuessPacket hangmanGuessPacket = new HangmanGuessPacket(message[7], Nickname);
                SerializePacket(hangmanGuessPacket);
            }
            else
            {
                //Updates local chat window
                mClientForm.SendNicknameToWindow("You", System.Windows.HorizontalAlignment.Right);
                mClientForm.SendMessageToWindow(message, System.Windows.HorizontalAlignment.Right);

                //Encrypts the nickname and message
                byte[] encryptedNickname = EncryptString(Nickname);
                byte[] encryptedMessage = EncryptString(message);


                //Send message packet to network
                EncryptedChatMessage encryptedChatMessage = new EncryptedChatMessage(encryptedNickname, encryptedMessage);
                SerializePacket(encryptedChatMessage);
            }
        }

        public void SendPrivateMessage(string reciever, string message)
        {
            //Updates local chat window
            mClientForm.SendNicknameToWindow("You -> " + reciever, System.Windows.HorizontalAlignment.Right);
            mClientForm.SendMessageToWindow(message, System.Windows.HorizontalAlignment.Right);

            //Sends private message over network
            EncryptedPrivateMessagePacket privateMessagePacket = new EncryptedPrivateMessagePacket(EncryptString(mNickname), EncryptString(reciever), EncryptString(message));
            SerializePacket(privateMessagePacket);
        }

        public void StartGame()
        {
            mClientForm.SendMessageToWindow("You have started a game of Hangman!", System.Windows.HorizontalAlignment.Center);
            PrintHangmanRules();

            StartHangmanPacket startHangmanPacket = new StartHangmanPacket(Nickname);
            SerializePacket(startHangmanPacket);
        }

        private void SerializePacket(Packet packetToSerialize)
        {
            MemoryStream msgStream = new MemoryStream();
            mFormatter.Serialize(msgStream, packetToSerialize);
            byte[] buffer = msgStream.GetBuffer();
            mWriter.Write(buffer.Length);
            mWriter.Write(buffer);
            mWriter.Flush();
        }

        private void PrintHangmanRules()
        {
            mClientForm.SendMessageToWindow(
                "The rules are simple.\nYou must guess the word that the computer has generated\n" +
                "You have six guesses\n" +
                "To make a guess you must type -guess (followed by your guess)\n" +
                "Only the first character will be counted\n" +
                "Best of luck!", System.Windows.HorizontalAlignment.Center);
        }

        private void TcpProcessServerResponse()
        {
            int numberOfBytes;

            try
            {
                while (mIsConnected)
                {
                    if ((numberOfBytes = mReader.ReadInt32()) != 0)
                    {
                        if (!mIsConnected) break;

                        byte[] buffer = mReader.ReadBytes(numberOfBytes);

                        MemoryStream stream = new MemoryStream(buffer);

                        Packet recievedPackage = mFormatter.Deserialize(stream) as Packet;

                        switch (recievedPackage.mPacketType)
                        {
                            case PacketType.EncryptedMessage:
                                EncryptedChatMessage encryptedChatMessage = (EncryptedChatMessage)recievedPackage;

                                mClientForm.SendNicknameToWindow(DecryptString(encryptedChatMessage.mNickname));
                                mClientForm.SendMessageToWindow(DecryptString(encryptedChatMessage.mMessage), System.Windows.HorizontalAlignment.Left);

                                break;
                            case PacketType.EncryptedPrivateMessage:
                                EncryptedPrivateMessagePacket privateMessagePacket = (EncryptedPrivateMessagePacket)recievedPackage;

                                mClientForm.SendNicknameToWindow("PM From " + DecryptString(privateMessagePacket.mSender), System.Windows.HorizontalAlignment.Left);
                                mClientForm.SendMessageToWindow(DecryptString(privateMessagePacket.mMessage), System.Windows.HorizontalAlignment.Left);

                                break;

                            case PacketType.Disconnect:
                                DisconnectPacket disconnectPacket = (DisconnectPacket)recievedPackage;
                                mClientForm.DisconnectMessage(disconnectPacket.mNickname);
                                break;
                            case PacketType.NicknameWindow:
                                NicknameWindowPacket nicknameWindowPacket = (NicknameWindowPacket)recievedPackage;

                                string[] names = nicknameWindowPacket.mNames.ToArray();
                                mClientForm.UpdateClientListWindow(names);
                                break;
                            case PacketType.Empty:
                                break;
                            case PacketType.PlayHangman:
                                StartHangmanPacket startHangmanPacket = (StartHangmanPacket)recievedPackage;

                                mClientForm.SendMessageToWindow(startHangmanPacket.mStarter + " has started a game of Hangman!", System.Windows.HorizontalAlignment.Center);
                                PrintHangmanRules();
                                break;
                            case PacketType.HangmanInfo:
                                HangmanInformationPacket hangmanInformationPacket = (HangmanInformationPacket)recievedPackage;

                                mClientForm.SendMessageToWindow(hangmanInformationPacket.mState, System.Windows.HorizontalAlignment.Center);
                                break;
                            case PacketType.HangmanLetterGuess:
                                HangmanGuessPacket guessPacket = (HangmanGuessPacket)recievedPackage;
                                mClientForm.SendMessageToWindow(guessPacket.mGuesser + " guessed " + guessPacket.mGuess, System.Windows.HorizontalAlignment.Left);

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }


            mReader.Close();
            mWriter.Close();
            mTcpClient.Close();
        }


        private byte[] Encrypt(byte[] data)
        {
            lock (mRSAProvider)
            {
                mRSAProvider.ImportParameters(mServerKey);
                //mRSAProvider.ImportParameters(mClientKey);

                return mRSAProvider.Encrypt(data, true);
            }
        }

        private byte[] Decrypt(byte[] data)
        {
            lock (mRSAProvider)
            {
                mRSAProvider.ImportParameters(mPrivateKey);
                //mRSAProvider.ImportParameters(mClientKey);

                return mRSAProvider.Decrypt(data, true);
            }
        }

        private byte[] EncryptString(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        private string DecryptString(byte[] message)
        {
            return Encoding.UTF8.GetString(message);
        }
    }
}
