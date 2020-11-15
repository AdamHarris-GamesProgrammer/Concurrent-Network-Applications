using System;
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

                //ClientMethod(socket);
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


            client.Send("Commands 1: Joke. 2: Weather Report. 3: Sarcasm. 4: Exit");

            BroadcastAll("Hello All");

            while ((recievedMessage = client.Read()) != null) {
                string serverMessage = GetReturnMessage(recievedMessage);

                client.Send(serverMessage);

                if (recievedMessage == "4") break;
            }

            Console.WriteLine("Closing Connection");
            client.Close();

            mClients.TryTake(out client);
        }

        private string GetReturnMessage(string code)
        {
            if(code == "1")
            {
                return GetJoke();
            }
            else if(code == "2")
            {
                return GetWeather();
            }
            else if(code == "3")
            {
                return GetSarcastic();
            }else if(code == "4")
            {
                return "Goodbye";
            }
            else
            {
                return "Invalid Message.";
            }

        }

        public void BroadcastAll(string message)
        {
            foreach(Client client in mClients)
            {
                client.SendImmediate(message);
            }
        }

        public string GetJoke()
        {
            string[] jokes = { "A man walks into a bar... He says \"Ow that bloody hurt\"", "You.", "How much money does a pirate pay for corn...? A buccaneer.", "Barista: How do you take your coffee...? Me: Very, very seriously." };

            Random rand = new Random();

            return jokes[rand.Next(0, jokes.Length)];
        }

        public string GetWeather()
        {
            string[] jokes = { "I'm not smart enough for that", "Try looking outside genius", "100& Chance of rain, accurate 0.001% percent of the time" };

            Random rand = new Random();

            return jokes[rand.Next(0, jokes.Length)];
        }

        public string GetSarcastic()
        {
            string[] jokes = { "It's lovely to meet you, is what I would say if I cared", "Honestly I have no clue what to put here", "Making responses is hard", "Boom." };

            Random rand = new Random();

            return jokes[rand.Next(0, jokes.Length)];
        }


    }
}
