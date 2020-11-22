using Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BasicServer
{
    class Client
    {
        private Socket mSocket;
        private NetworkStream mStream;
        private BinaryReader mReader;
        private BinaryWriter mWriter;
        private BinaryFormatter mFormatter;


        private object mReadLock;
        private object mWriteLock;

        public Client(Socket socket)
        {
            mReadLock = new object();
            mWriteLock = new object();
            mSocket = socket;

            mStream = new NetworkStream(mSocket);
            mReader = new BinaryReader(mStream, Encoding.UTF8);
            mWriter = new BinaryWriter(mStream, Encoding.UTF8);
            mFormatter = new BinaryFormatter();
        }

        public void Close()
        {
            mSocket.Shutdown(SocketShutdown.Both);
            mStream.Close();
            mReader.Close();
            mWriter.Close();
            mSocket.Close();
        }

        public Packet Read()
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

        public void Send(Packet message)
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
    }
}
