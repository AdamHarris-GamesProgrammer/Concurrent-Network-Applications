using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace BasicServer
{
    class Client
    {
        private Socket mSocket;
        private NetworkStream mStream;
        private StreamReader mReader;
        private StreamWriter mWriter;
        private object mReadLock;
        private object mWriteLock;

        public Client(Socket socket)
        {
            mReadLock = new object();
            mWriteLock = new object();

            mSocket = socket;

            mStream = new NetworkStream(mSocket);
            mReader = new StreamReader(mStream, Encoding.UTF8);
            mWriter = new StreamWriter(mStream, Encoding.UTF8);
        }

        public void Close()
        {
            mStream.Close();
            mReader.Close();
            mWriter.Close();
            mSocket.Close();
        }

        public string Read()
        {
            lock (mReadLock)
            {
                string message = mReader.ReadLine();
                return message;
            }
        }

        public void Send(string message)
        {
            mWriter.AutoFlush = false;
            lock (mWriteLock)
            {
                mWriter.WriteLine(message);
                mWriter.Flush();
            }
        }

        public void SendImmediate(string message)
        {
            mWriter.AutoFlush = true;
            lock (mWriteLock)
            {
                mWriter.WriteLine(message);
            }
        }


    }
}
