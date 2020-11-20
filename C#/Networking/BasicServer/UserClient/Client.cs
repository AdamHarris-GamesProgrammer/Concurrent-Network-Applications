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

        private string nickname = "Username";

        bool isConnected = false;

        public Client()
        {
            tcpClient = new TcpClient();
        }

        public void SetNickname(string name)
        {
            nickname = name;
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                tcpClient.Connect(ipAddress, port);
                stream = tcpClient.GetStream();
                writer = new StreamWriter(stream, Encoding.UTF8);
                reader = new StreamReader(stream, Encoding.UTF8);

                isConnected = true;

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

            DisconnectFromServer();
        }

        public void DisconnectFromServer()
        {
            tcpClient.Close();
            reader.Close();
            writer.Close();
            isConnected = false;
        }

        public void SendMessage(string message)
        {
            writer.WriteLine(nickname + ": " + message);
            writer.Flush();
            clientForm.UpdateChatWindow("Me: " + message);
        }

        public void DisconnectedMessage()
        {
            writer.WriteLine(nickname + " has left the chat");
            writer.Flush();
        }

        private void ProcessServerResponse()
        {
            while (isConnected)
            {
                clientForm.UpdateChatWindow(reader.ReadLine());
            }
        }

    }
}
