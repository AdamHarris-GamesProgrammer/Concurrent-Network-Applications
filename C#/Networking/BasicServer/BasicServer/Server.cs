using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.IO;
using System.Text.Unicode;

namespace BasicServer
{
    class Server
    {
        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            tcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            tcpListener.Start();

            Console.WriteLine("Awaiting Connection");

            Socket socket = tcpListener.AcceptSocket();

            Console.WriteLine("Accepted Connection");

            ClientMethod(socket);
        }

        public void Stop()
        {
            tcpListener.Stop();
            Console.WriteLine("Closed Connection");
        }

        private void ClientMethod(Socket socket)
        {
            string recievedMessage;

            NetworkStream stream = new NetworkStream(socket);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);

            writer.WriteLine("Commands 1: Joke. 2: Weather Report. 3: Sarcasm. 4: Exit");
            writer.Flush();

            while ((recievedMessage = reader.ReadLine()) != null) {
                string serverMessage = GetReturnMessage(recievedMessage);

                writer.WriteLine(serverMessage);
                writer.Flush();

                //writer.WriteLine("Commands: 1: Joke 2: Weather Report 3: Sarcasm 4: Exit");

                if (recievedMessage == "4") break;
            }

            Console.WriteLine("Closing Connection");
            socket.Close();
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

        private TcpListener tcpListener;
    }
}
