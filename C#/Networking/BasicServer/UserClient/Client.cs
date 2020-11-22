using Packets;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace UserClient
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private BinaryFormatter formatter;
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

        public string GetNickname()
        {
            return nickname;
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                tcpClient.Connect(ipAddress, port);
                stream = tcpClient.GetStream();
                writer = new BinaryWriter(stream, Encoding.UTF8);
                reader = new BinaryReader(stream, Encoding.UTF8);
                formatter = new BinaryFormatter();

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
            writer.Dispose();
            reader.Dispose();
            tcpClient.Close();
            isConnected = false;
        }

        public void SendMessage(string message)
        {
            //writer.WriteLine(nickname + ": " + message);
            //writer.Flush();
            clientForm.UpdateChatWindow("Me: " + message, System.Windows.HorizontalAlignment.Right);

            ChatMessagePacket chatMsg = new ChatMessagePacket(message);

            MemoryStream memStream = new MemoryStream();
            formatter.Serialize(memStream, chatMsg);
            byte[] buffer = memStream.GetBuffer();
            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }

        public void DisconnectedMessage()
        {
            //writer.WriteLine(nickname + " has left the chat");
            //writer.Flush();
        }

        private void ProcessServerResponse()
        {
            int numberOfBytes;

            while ((numberOfBytes = reader.ReadInt32()) != 0)
            {
                byte[] buffer = reader.ReadBytes(numberOfBytes);

                MemoryStream stream = new MemoryStream(buffer);

                Packet recievedMessage = new Packet();

                recievedMessage = formatter.Deserialize(stream) as Packet;

                switch (recievedMessage.packetType)
                {
                    case PacketType.ChatMessage:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                        clientForm.UpdateChatWindow(chatPacket.mMessage, System.Windows.HorizontalAlignment.Left);
                        break;
                    case PacketType.PrivateMessage:
                        break;
                    case PacketType.ClientName:
                        break;
                    default:
                        break;

                }
            }
        }

    }
}
