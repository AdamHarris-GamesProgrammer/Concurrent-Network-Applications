﻿using Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Server
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


        public IPEndPoint mIpEndPoint;

        public Client(Socket socket)
        {
            //Initializes the read and write lock objects
            mReadLock = new object();
            mWriteLock = new object();
            mSocket = socket;

            //Initializes the networking objects
            mStream = new NetworkStream(mSocket);
            mReader = new BinaryReader(mStream, Encoding.UTF8);
            mWriter = new BinaryWriter(mStream, Encoding.UTF8);
            mFormatter = new BinaryFormatter();

        }

        /// <summary>
        /// Closes the clients connection to the server
        /// </summary>
        public void Close()
        {
            mSocket.Shutdown(SocketShutdown.Both);
            mStream.Close();
            mReader.Close();
            mWriter.Close();
            mSocket.Close();
        }

        /// <summary>
        /// This method reads TCP data 
        /// </summary>
        /// <returns></returns>
        public Packet TcpRead()
        {
            lock (mReadLock)
            {
                //Checks the result is valid
                int result = mReader.ReadInt32();
                if (result != -1)
                {
                    //Reads the bytes buffer from the reader
                    byte[] buffer = mReader.ReadBytes(result);

                    MemoryStream stream = new MemoryStream(buffer);

                    //Deserializes the stream as a packet for processing
                    return mFormatter.Deserialize(stream) as Packet;
                }
                else
                {
                    return null;
                }
            }
        }

        
        /// <summary>
        /// Sends a Packet to the client via TCP
        /// </summary>
        /// <param name="message"></param>
        public void TcpSend(Packet message)
        {
            lock (mWriteLock)
            {
                MemoryStream stream = new MemoryStream();

                //Serializes the message
                mFormatter.Serialize(stream, message);

                //Gets the bytes buffer
                byte[] bufffer = stream.GetBuffer();

                //Writes and sends the buffer onto the network
                mWriter.Write(bufffer.Length);
                mWriter.Write(bufffer);
                mWriter.Flush();
            }
        }
    }
}
