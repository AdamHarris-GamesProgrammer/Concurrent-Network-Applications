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

            writer.WriteLine("You have connected to the server how may I help you?");
            writer.Flush();


            while((recievedMessage = reader.ReadLine()) != null) {
                string serverMessage = GetReturnMessage(recievedMessage);

                writer.WriteLine(serverMessage);
                writer.Flush();

                if (recievedMessage == "end") break;
            }

            Console.WriteLine("Closing Connection");
            socket.Close();
        }

        private string GetReturnMessage(string code)
        {
            if (code == "hi") return "Hello.";
            else if (code == "end") return "Goodbye.";
            else return "Invalid message.";
        }

        private TcpListener tcpListener;
    }
}
