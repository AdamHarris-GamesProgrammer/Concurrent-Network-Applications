using System;

namespace NetworkedClient
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new GameInstance();

            bool connected = game.Connect("127.0.0.1", 4444);


            if (connected)
            {
                game.Run();
            }
            else
            {
                Console.WriteLine("Failed to connect to server");
            }

            game.Exit();
        }
    }
}
