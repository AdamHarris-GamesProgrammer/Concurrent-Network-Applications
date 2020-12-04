using System;
using System.Threading;

namespace MultipleThreads
{
    class Program
    {
        static void Main(string[] args)
        {
            for(int i =0; i < 8; i++)
            {
                var thread = new Thread(OutputNum);
                thread.Start(i);
            }
            

            while(true)
                Console.WriteLine("Hello from main()");
        }

        static void DifferentMethod()
        {
            while (true)
                Console.WriteLine("Hello from DifferentMethod()");
        }

        static void OutputNum(object num)
        {
            while(true)
                Console.WriteLine("The number is: {0}", num);
        }
    }
}
