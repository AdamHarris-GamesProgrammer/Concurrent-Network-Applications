using System;

namespace Packets
{
    public enum PacketType
    {
        Empty,
        ChatMessage,
        PrivateMessage,
        ClientName,
        Nickname
    }

    [Serializable]
    public class Packet
    {
        public PacketType packetType { get; set; }

        public Packet()
        {
            packetType = PacketType.Empty;
        }
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string mMessage;

        public ChatMessagePacket(string message)
        {
            mMessage = message;
            packetType = PacketType.ChatMessage;
        }
    }

    [Serializable]
    public class NicknamePacket : Packet
    {
        public string nickname;

        public NicknamePacket(string name)
        {
            nickname = name;
            packetType = PacketType.Nickname;
        }
    }

}
