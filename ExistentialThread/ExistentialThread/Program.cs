using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExistentialThread
{
    public class ThreadWork
    {
        public static int numberOfThreads = 0;
        public static int highestNum = 0;

        public static void Task(int startNum)
        {
            if (numberOfThreads > 110) return;
            if (highestNum == 500) return;

            numberOfThreads++;

            int target = startNum + 10;
            int threadNum = numberOfThreads;



            for (int i = startNum; i <= target || i == 500; i++)
            {
                Console.WriteLine(i);
                if(i > highestNum)
                {
                    highestNum = i;
                }

                Thread.Yield();
            }

            Thread threadA = new Thread(() => Task(target));
            Thread threadB = new Thread(() => Task(target));

            threadA.Start();
            threadA.Join();
            threadB.Start();
            threadB.Join();

            

            Console.WriteLine("thread {0} is finished", threadNum);

            if(threadA.ThreadState == ThreadState.Stopped && threadB.ThreadState == ThreadState.Stopped)
            {
                Console.WriteLine("Highest num {0}", highestNum);

            }

            Thread.Yield();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Thread thread = new Thread(() => ThreadWork.Task(0));
            thread.Start();
            if(thread.ThreadState == ThreadState.Stopped) Console.WriteLine("Total thread count: {0}", ThreadWork.numberOfThreads);
        }
    }
}
