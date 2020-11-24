using System;

namespace Packets
{
    public enum PacketType
    {
        Empty,
        ChatMessage,
        PrivateMessage,
        NewNickname,
        Disconnect
    }

    [Serializable]
    public class Packet
    {
        public PacketType mPacketType { get; set; }

        public Packet()
        {
            mPacketType = PacketType.Empty;
        }
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string mMessage;
        public string mSender;

        public ChatMessagePacket(string sender, string message)
        {
            mMessage = message;
            mSender = sender;
            mPacketType = PacketType.ChatMessage;
        }
    }

    [Serializable]
    public class DisconnectPacket : Packet
    {
        public string mNickname;

        public DisconnectPacket(string name)
        {
            mNickname = name;
            mPacketType = PacketType.Disconnect;
        }
    }

    [Serializable]
    public class SetNicknamePacket : Packet
    {
        public string mNewNickname;
        public string mOldNickname;

        public SetNicknamePacket(string oldName, string name)
        {
            mOldNickname = oldName;
            mNewNickname = name;
            mPacketType = PacketType.NewNickname;
        }
    }

}
