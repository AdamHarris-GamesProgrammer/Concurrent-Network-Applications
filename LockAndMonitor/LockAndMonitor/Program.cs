using System;
using System.Threading;
using System.Xml.Serialization;

namespace LockAndMonitor
{
    public class Buffer
    {
        private int data;
        private bool empty = true;

        public void Read(ref int data)
        {
            lock (this)
            {
                if (empty) Monitor.Wait(this);

                empty = true;
                data = this.data;
                Console.WriteLine("        {0} read", data);
                Monitor.Pulse(this);
            }
        }

        public void Write(int data)
        {
            lock(this)
            {
                if (!empty) Monitor.Wait(this);

                empty = false;
                this.data = data;
                Console.WriteLine(data + " write");
                Monitor.Pulse(this);
            }
        }
    }

    public class Producer
    {
        private Buffer buffer;
        private Random random = new Random();

        public Producer(Buffer buffer)
        {
            this.buffer = buffer;
        }

        public void Production()
        {
            for (int i = 1; i <= 10; i++)
            {
                Thread.Sleep(random.Next(501));
                buffer.Write(i);
            }
        }
    }

    public class Consumer
    {
        private Buffer buffer;
        private Random random = new Random();

        public Consumer(Buffer buffer)
        {
            this.buffer = buffer;
        }

        public void Consumption()
        {
            int data = -1;

            for (int i = 1; i <= 10; i++)
            {
                Thread.Sleep(random.Next(501));
                buffer.Read(ref data);
                data -= 1;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Buffer buff = new Buffer();
            Producer prod = new Producer(buff);
            Consumer con = new Consumer(buff);

            Thread ProducerThread = new Thread(new ThreadStart(prod.Production));
            Thread ConsumerThread = new Thread(new ThreadStart(con.Consumption));
            ProducerThread.Start();
            ConsumerThread.Start();
        }
    }
}
