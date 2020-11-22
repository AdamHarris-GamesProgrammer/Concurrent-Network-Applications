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

            clientForm.SendMessageToWindow("Me: " + message, System.Windows.HorizontalAlignment.Right);


            NicknamePacket nicknamePacket = new NicknamePacket(nickname);
            MemoryStream nicknameStream = new MemoryStream();
            formatter.Serialize(nicknameStream, nicknamePacket);
            byte[] nicknameBuffer = nicknameStream.GetBuffer();
            writer.Write(nicknameBuffer.Length);
            writer.Write(nicknameBuffer);
            writer.Flush();

            ChatMessagePacket messagePacket = new ChatMessagePacket(message);
            MemoryStream msgStream = new MemoryStream();
            formatter.Serialize(msgStream, messagePacket);
            byte[] buffer = msgStream.GetBuffer();
            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }

        public void DisconnectedMessage()
        {
            SendMessage(nickname + " has disconnected");
        }

        private void ProcessServerResponse()
        {
            int numberOfBytes;

            while (isConnected)
            {
                while ((numberOfBytes = reader.ReadInt32()) != 0)
                {
                    byte[] buffer = reader.ReadBytes(numberOfBytes);

                    MemoryStream stream = new MemoryStream(buffer);

                    Packet recievedMessage = formatter.Deserialize(stream) as Packet;

                    switch (recievedMessage.packetType)
                    {
                        case PacketType.ChatMessage:
                            ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                            clientForm.SendMessageToWindow(chatPacket.mMessage, System.Windows.HorizontalAlignment.Left);
                            break;
                        case PacketType.PrivateMessage:
                            break;
                        case PacketType.ClientName:
                            break;
                        case PacketType.Empty:
                            break;
                        case PacketType.Nickname:
                            NicknamePacket nicknamePacket = (NicknamePacket)recievedMessage;
                            clientForm.SendNicknameToWindow(nicknamePacket.nickname);
                            break;
                        default:
                            break;
                    }
                }
            }

        }

    }
}
