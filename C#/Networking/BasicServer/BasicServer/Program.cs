using System;

namespace BasicServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverIp = "127.0.0.1";
            int port = 4444;

            Server server = new Server(serverIp, port);

            server.Start();
        }
    }
}
