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
            clientForm.SetWindowTitle(nickname);

            DisconnectFromServer();
        }

        public void SetNickname(string name)
        {
            nickname = name;
            if (clientForm != null) clientForm.SetWindowTitle(name);
        }

        public void DisconnectFromServer()
        {
            if (!isConnected) return;
            isConnected = false;
            clientForm.DisconnectMessage(nickname);

            DisconnectPacket disconnectPacket = new DisconnectPacket(nickname);
            SerializePacket(disconnectPacket);
            
        }

        public void SendMessage(string message)
        {

            clientForm.SendNicknameToWindow("You", System.Windows.HorizontalAlignment.Right);
            clientForm.SendMessageToWindow(message, System.Windows.HorizontalAlignment.Right);


            NicknamePacket nicknamePacket = new NicknamePacket(nickname);
            SerializePacket(nicknamePacket);

            ChatMessagePacket messagePacket = new ChatMessagePacket(message);
            SerializePacket(messagePacket);
        }

        private void SerializePacket(Packet packetToSerialize)
        {
            MemoryStream msgStream = new MemoryStream();
            formatter.Serialize(msgStream, packetToSerialize);
            byte[] buffer = msgStream.GetBuffer();
            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }

        private void ProcessServerResponse()
        {
            int numberOfBytes;

            while (isConnected)
            {
                if((numberOfBytes = reader.ReadInt32()) != 0)
                {
                    if (!isConnected) break;

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
                        case PacketType.Disconnect:
                            DisconnectPacket disconnectPacket = (DisconnectPacket)recievedMessage;
                            clientForm.DisconnectMessage(disconnectPacket.nickname);
                            break;
                    }
                }
            }

            reader.Close();
            writer.Close();
            tcpClient.Close();
        }

    }
}
