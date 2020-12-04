using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingIntroduction
{
    public class ThreadWork
    {
        public static void Countup(int index)
        {
            for (int i = 0; i < 3; i++)
            {

                Thread.Sleep(index * 500);
                Console.WriteLine("Thread: {0}", index);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Thread[] threads = new Thread[5];

            for (int i = 0; i < 5; i++)
            {
                int index = i;
                threads[i] = new Thread(() => ThreadWork.Countup(index));
            }

            for (int i = 0; i < 5; i++)
            {
                threads[i].Start();
            }
        }
    }
}
