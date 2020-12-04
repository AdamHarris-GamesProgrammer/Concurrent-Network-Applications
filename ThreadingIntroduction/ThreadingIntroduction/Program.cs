using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingIntroduction
{
    public class ThreadWork
    {
        public static void Countup()
        {
            for(int i = 0; i <= 100; i++)
            {
                Console.WriteLine(i);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Thread countingThread = new Thread(ThreadWork.Countup);
            countingThread.Start();
            for(int i = 100; i >= 0; i--)
            {
                Console.WriteLine(i);
            }
        }
    }
}
