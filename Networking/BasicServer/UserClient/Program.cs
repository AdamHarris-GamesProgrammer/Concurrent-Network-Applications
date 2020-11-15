﻿using System;
using System.Windows;

namespace UserClient
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Client client = new Client();

            bool connected = client.Connect("127.0.0.1", 4444);


            if (connected)
            {
                client.Run();
            }
            else
            {
                Console.WriteLine("Failed to connect to server");
            }
        }
    }
}
