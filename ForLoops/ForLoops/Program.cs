using System;
using System.Runtime.InteropServices;

namespace ForLoops
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] array = new int[10];

            int highest = 0;

            for(int i = 0; i < 10; i++)
            {
                int input = Int32.Parse(Console.ReadLine());
                
                if(input > highest)
                {
                    highest = input;
                }
                
                array[i] = input;
            }
            Console.WriteLine(highest);

            PrintReverse(array);
        }

        static void PrintReverse(int[] array)
        {
            for(int i = array.Length - 1; i >= 0; i--)
            {
                Console.WriteLine(array[i]);
            }
        }
    }
}
