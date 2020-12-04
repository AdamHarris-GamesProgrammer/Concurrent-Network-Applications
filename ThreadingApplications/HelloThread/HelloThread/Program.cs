using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace HelloThread
{
    class Program
    {
        //Main is a single thread
        static void Main(string[] args)
        {
            var thread = new Thread(DifferentMethod);
            thread.Start();
        }

        
        static void DifferentMethod()
        {
            Console.WriteLine("Hello other thread");
        }
    }
}
