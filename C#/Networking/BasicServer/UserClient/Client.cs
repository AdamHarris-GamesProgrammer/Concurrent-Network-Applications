using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UserClient
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;
        private ClientForm clientForm;

        public Client()
        {
            tcpClient = new TcpClient();
        }



        public bool Connect(string ipAddress, int port)
        {
            try
            {
                tcpClient.Connect(ipAddress, port);
                stream = tcpClient.GetStream();
                writer = new StreamWriter(stream, Encoding.UTF8);
                reader = new StreamReader(stream, Encoding.UTF8);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public void Run()
        {
            clientForm = new ClientForm(this);

            Thread thread = new Thread(ProcessServerResponse);

            thread.Start();

            

            clientForm.ShowDialog();

            tcpClient.Close();
        }

        public void SendMessage(string message)
        {
            writer.WriteLine(message);
            writer.Flush();
            clientForm.UpdateChatWindow(message);
        }

        private void ProcessServerResponse()
        {
            while (true)
            {
                clientForm.UpdateChatWindow(reader.ReadLine());
            }
        }

    }
}
