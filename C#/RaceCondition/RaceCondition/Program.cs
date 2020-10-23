using System;
using System.Threading;

namespace RaceCondition
{
    class Program
    {
        private static Semaphore semaphore;

        static int num = 5;

        public static void Work(ref int num)
        {
            semaphore.WaitOne();
            int temp = num;
            temp++;
            num = temp;
            Console.WriteLine(num);
            Thread.Sleep(30);

            semaphore.Release();
        }

        static void Main(string[] args)
        {
            semaphore = new Semaphore(0, 1);

            for(int i = 0; i < 100; i++)
            {
                Thread t = new Thread(() => Work(ref num));
                t.Start();
            }

            Thread.Sleep(10);

            semaphore.Release(1);
        }
    }
}
