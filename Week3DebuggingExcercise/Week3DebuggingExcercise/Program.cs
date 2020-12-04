using System;
using System.Threading;

public class ThreadingStuff
{
    public static void Sort(int[] numbers, int count)
    {
        Array.Sort(numbers);

        Console.WriteLine("Sorted {0}", count);
    }
}

public class SortArrays
{
    static void Generate(int[] newNumbers)
    {
        System.Random random = new System.Random();

        for(int i = 0; i < 10; i++)
        {
            newNumbers[i] = random.Next(50);
        }
    }

    static void ToScreen(int[] numsToPrint)
    {
        for(int i  =0; i < 10; i++)
        {
            Console.Write(numsToPrint[i] + "\t");
        }
        Console.WriteLine(); ;
    }

    static void Main()
    {
        int[] numbers1 = new int[10];
        int[] numbers2 = new int[10];
        int[] numbers3 = new int[10];
        Generate(numbers1);
        Generate(numbers2);
        Generate(numbers3);
        Console.WriteLine("The initial lists are: ");
        ToScreen(numbers1);
        ToScreen(numbers2);
        ToScreen(numbers3);

        Thread thread1 = new Thread(new ThreadStart(() => ThreadingStuff.Sort(numbers1, 1)));
        Thread thread2 = new Thread(new ThreadStart(() => ThreadingStuff.Sort(numbers2, 2)));
        Thread thread3 = new Thread(new ThreadStart(() => ThreadingStuff.Sort(numbers3, 3)));


        thread1.Start();
        thread1.Join();

        thread2.Start();
        thread2.Join();

        thread3.Start();
        thread3.Join();

        //while(thread1.IsAlive || thread2.IsAlive || thread3.IsAlive){}

        Console.WriteLine("Sorted lists are: ");
        ToScreen(numbers1);
        ToScreen(numbers2);
        ToScreen(numbers3);
    }
}