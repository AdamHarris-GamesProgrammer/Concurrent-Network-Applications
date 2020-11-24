using System;

namespace Packets
{
    public enum PacketType
    {
        Empty,
        ChatMessage,
        PrivateMessage,
        ClientName,
        Nickname,
        NewNickname,
        Disconnect
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

    [Serializable]
    public class DisconnectPacket : Packet
    {
        public string nickname;

        public DisconnectPacket(string name)
        {
            nickname = name;
            packetType = PacketType.Disconnect;
        }
    }

    [Serializable]
    public class SetNicknamePacket : Packet
    {
        public string newNickname;
        public string oldNickname;

        public SetNicknamePacket(string oldName, string name)
        {
            oldNickname = oldName;
            newNickname = name;
            packetType = PacketType.NewNickname;
        }
    }

}
