using System;
using System.Threading;

namespace FibonacciThreading
{

    class Fibonacci
    {
        public static void Sequence(int[] arr, int startIndex, int firstNum, int secondNum)
        {
            for(int i = startIndex; i < startIndex + 10; i++)
            {
                if (i < 2)
                {
                    arr[i] = 1;
                }
                else
                {
                    arr[i] = firstNum + secondNum;
                    firstNum = secondNum;
                    secondNum = arr[i];
                }
                
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int[] arr = new int[30];

            Thread thread1 = new Thread(new ThreadStart(()=> Fibonacci.Sequence(arr, 0,1, 1)));
            Thread thread2 = new Thread(new ThreadStart(()=> Fibonacci.Sequence(arr, 10,34, 55)));
            Thread thread3 = new Thread(new ThreadStart(()=> Fibonacci.Sequence(arr, 20,4181, 6765)));

            thread1.Start();
            thread2.Start();
            thread3.Start();

            Console.WriteLine("First 30 Fibonacci Numbers");
            for (int i = 0; i < 30; i++)
            {
                if (i > 0 && i % 10 == 0) Console.WriteLine();
                Console.Write("{0}\t", arr[i]);
            }
        }
    }
}
