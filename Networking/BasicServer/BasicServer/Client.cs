using Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace BasicServer
{
    class Client
    {
        private Socket mSocket;
        private NetworkStream mStream;
        private BinaryReader mReader;
        private BinaryWriter mWriter;
        private BinaryFormatter mFormatter;

        private RSACryptoServiceProvider mRSAProvider;
        private RSAParameters mPublicKey;
        private RSAParameters mPrivateKey;
        private RSAParameters mClientKey;
        private RSAParameters mServerKey;

        private object mReadLock;
        private object mWriteLock;

        private string mNickname;
        public String Nickname
        {
            get { return mNickname; }
            set { mNickname = value; }
        }

        public IPEndPoint mIpEndPoint;

        public Client(Socket socket)
        {
            mReadLock = new object();
            mWriteLock = new object();
            mSocket = socket;

            mStream = new NetworkStream(mSocket);
            mReader = new BinaryReader(mStream, Encoding.UTF8);
            mWriter = new BinaryWriter(mStream, Encoding.UTF8);
            mFormatter = new BinaryFormatter();

            Nickname = "Username";

            mRSAProvider = new RSACryptoServiceProvider(1024);
            mPublicKey = mRSAProvider.ExportParameters(false);
            mPrivateKey = mRSAProvider.ExportParameters(true);

        }

        public void Close()
        {
            mSocket.Shutdown(SocketShutdown.Both);
            mStream.Close();
            mReader.Close();
            mWriter.Close();
            mSocket.Close();
        }

        public void Login(RSAParameters clientKey)
        {
            //set name end point and client key

            mPublicKey = clientKey;

            //Tcp Send the server key packet
        }

        public Packet TcpRead()
        {
            lock (mReadLock)
            {
                int result = mReader.ReadInt32();
                if (result != -1)
                {
                    byte[] buffer = mReader.ReadBytes(result);

                    MemoryStream stream = new MemoryStream(buffer);

                    return mFormatter.Deserialize(stream) as Packet;
                }
                else
                {
                    return null;
                }
            }
        }

        public void TcpSend(Packet message)
        {
            lock (mWriteLock)
            {
                MemoryStream stream = new MemoryStream();
                mFormatter.Serialize(stream, message);

                byte[] bufffer = stream.GetBuffer();

                mWriter.Write(bufffer.Length);
                mWriter.Write(bufffer);
                mWriter.Flush();
            }
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
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            EncryptedMessage messagePacket = new EncryptedMessage(bytes);
            TcpSend(messagePacket);

            return bytes;
        }

        private string DecryptString(byte[] message)
        {
            return Encoding.UTF8.GetString(message);
        }
    }
}
