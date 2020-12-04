using System;
using System.Threading;

namespace AnotherThreadSynchExample
{
    class Program
    {
        static object doorLock = new Object();

        static Random rand = new Random();

        static void Main(string[] args)
        {
            for(int i = 0; i < 5; i++)
            {
                new Thread(UseRestroom).Start();
            }
        }

        static void UseRestroom()
        {
            Console.WriteLine("Person {0}: Trying to open the bathroom stall...", Thread.CurrentThread.ManagedThreadId);
            lock (doorLock)
            {
                Console.WriteLine("Person {0}: unlocked the door. Stall is now occupied...", Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(rand.Next(2000));
                Console.WriteLine("Person {0}: leaving the stall... Lock undone", Thread.CurrentThread.ManagedThreadId);
            }
            Console.WriteLine("Person {0}: is leaving the restaurant.", Thread.CurrentThread.ManagedThreadId);
        }
    }
}
