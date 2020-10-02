using System;

namespace AddMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter your first number: ");
            int a = Int32.Parse(Console.ReadLine());
            Console.Write("Please enter your second number: ");
            int b = Int32.Parse(Console.ReadLine());
            Console.WriteLine(Add(a, b));

            Console.Write("Please enter your first number: ");
            float af = float.Parse(Console.ReadLine());
            Console.Write("Please enter your second number: ");
            float bf = float.Parse(Console.ReadLine());
            Console.WriteLine(Add(af, bf));
        }

        static int Add(int a, int b)
        {
            return a + b;
        }
        static float Add(float a, float b)
        {
            return a + b;
        }
    }
}
