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
        private TcpClient mTcpClient;
        private NetworkStream mStream;
        private BinaryWriter mWriter;
        private BinaryReader mReader;
        private BinaryFormatter mFormatter;
        private ClientForm mClientForm;


        bool mIsConnected = false;

        private string mNickname;
        public String Nickname
        {
            get
            {
                if (mNickname == null) return "Username";
                return mNickname;
            }
            set
            {
                SetNickname(value);
                mNickname = value;
            }
        }

        private void SetNickname(string name)
        {
                SetNicknamePacket setNicknamePacket = new SetNicknamePacket(Nickname, name);
                SerializePacket(setNicknamePacket);

                if (mClientForm != null) mClientForm.SetWindowTitle(Nickname);
        }

        public Client()
        {
            mTcpClient = new TcpClient();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                mTcpClient.Connect(ipAddress, port);
                mStream = mTcpClient.GetStream();
                mWriter = new BinaryWriter(mStream, Encoding.UTF8);
                mReader = new BinaryReader(mStream, Encoding.UTF8);
                mFormatter = new BinaryFormatter();

                mIsConnected = true;

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
            mClientForm = new ClientForm(this);

            Thread thread = new Thread(ProcessServerResponse);

            mClientForm.SetWindowTitle(Nickname);

            thread.Start();

            mClientForm.ShowDialog();


            DisconnectFromServer();
        }



        public void DisconnectFromServer()
        {
            if (!mIsConnected) return;
            mIsConnected = false;
            mClientForm.DisconnectMessage(Nickname);

            DisconnectPacket disconnectPacket = new DisconnectPacket(Nickname);
            SerializePacket(disconnectPacket);
            
        }

        public void SendMessage(string message)
        {
            //Updates local chat window
            mClientForm.SendNicknameToWindow("You", System.Windows.HorizontalAlignment.Right);
            mClientForm.SendMessageToWindow(message, System.Windows.HorizontalAlignment.Right);

            //Send message packet to network
            ChatMessagePacket messagePacket = new ChatMessagePacket(Nickname, message);
            SerializePacket(messagePacket);
        }

        private void SerializePacket(Packet packetToSerialize)
        {
            MemoryStream msgStream = new MemoryStream();
            mFormatter.Serialize(msgStream, packetToSerialize);
            byte[] buffer = msgStream.GetBuffer();
            mWriter.Write(buffer.Length);
            mWriter.Write(buffer);
            mWriter.Flush();
        }

        private void ProcessServerResponse()
        {
            int numberOfBytes;

            while (mIsConnected)
            {
                if((numberOfBytes = mReader.ReadInt32()) != 0)
                {
                    if (!mIsConnected) break;

                    byte[] buffer = mReader.ReadBytes(numberOfBytes);

                    MemoryStream stream = new MemoryStream(buffer);

                    Packet recievedMessage = mFormatter.Deserialize(stream) as Packet;

                    switch (recievedMessage.mPacketType)
                    {
                        case PacketType.ChatMessage:
                            ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                            mClientForm.SendNicknameToWindow(chatPacket.mSender);
                            mClientForm.SendMessageToWindow(chatPacket.mMessage, System.Windows.HorizontalAlignment.Left);
                            break;
                        case PacketType.PrivateMessage:
                            break;
                        case PacketType.Empty:
                            break;
                        case PacketType.Disconnect:
                            DisconnectPacket disconnectPacket = (DisconnectPacket)recievedMessage;
                            mClientForm.DisconnectMessage(disconnectPacket.mNickname);
                            break;
                        case PacketType.NicknameWindow:
                            NicknameWindowPacket nicknameWindowPacket = (NicknameWindowPacket)recievedMessage;
                            
                            string[] names = nicknameWindowPacket.mNames.ToArray();
                            mClientForm.UpdateClientListWindow(names);
                            break;
                        default:
                            break;
                    }
                }
            }

            mReader.Close();
            mWriter.Close();
            mTcpClient.Close();
        }

    }
}
