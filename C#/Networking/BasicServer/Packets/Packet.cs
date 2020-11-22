using System;

namespace Packets
{
    public enum PacketType
    {
        ChatMessage,
        PrivateMessage,
        ClientName
    }

    [Serializable]
    public class Packet
    {
        public PacketType packetType { get; set; }
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
}
