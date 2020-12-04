using System;
using System.Threading;

namespace MultipleThreads
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                var thread = new Thread(OutputNum);
                //Background threads are terminated as soon as the main thread ends
                thread.IsBackground = true;
                thread.Start();
            }

            Console.WriteLine("Leaving main");
        }

        static void DifferentMethod()
        {
            while (true)
                Console.WriteLine("Hello from DifferentMethod()");
        }

        static void OutputNum()
        {
            while (true)
                Console.WriteLine("This thread is: {0}", Thread.CurrentThread.ManagedThreadId);
        }
    }
}
