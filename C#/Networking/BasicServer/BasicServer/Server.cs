﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.IO;
using System.Text.Unicode;
using System.Collections.Concurrent;
using System.Threading;

namespace BasicServer
{
    class Server
    {
        private TcpListener mTcpListener;

        private ConcurrentBag<Client> mClients;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            mTcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            mClients = new ConcurrentBag<Client>();

            mTcpListener.Start();

            while (mClients.Count != 4)
            {
                Console.WriteLine("Awaiting Connection");

                Socket socket = mTcpListener.AcceptSocket();

                Client client = new Client(socket);

                mClients.Add(client);



                Console.WriteLine("Accepted Connection");

                Thread thread = new Thread(() => { ClientMethod(client); });

                thread.Start();
            }

        }

        public void Stop()
        {
            mTcpListener.Stop();
            Console.WriteLine("Closed Connection");
        }

        private void ClientMethod(Client client)
        {
            string recievedMessage;


            //client.Send("Commands 1: Joke. 2: Weather Report. 3: Sarcasm. 4: Exit");

           // client.Send("Boop");


            while ((recievedMessage = client.Read()) != null)
            {
                // string serverMessage = GetReturnMessage(recievedMessage);

                //client.Send()

                //client.Send(serverMessage);
                //Console.WriteLine("Recieved Message");

                foreach(Client cli in mClients)
                {
                    if (cli != client)
                    {
                        cli.Send(recievedMessage);
                    }
                }
            }

            Console.WriteLine("Closing Connection");
            client.Close();

            mClients.TryTake(out client);
        }


        public void BroadcastAll(string message)
        {
            foreach(Client client in mClients)
            {
                client.SendImmediate(message);
            }
        }


    }
}
