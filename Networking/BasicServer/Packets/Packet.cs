using System;
using System.Collections.Generic;
using System.Net;

namespace Packets
{
    public enum PacketType
    {
        Empty,
        ChatMessage,
        PrivateMessage,
        NewNickname,
        Disconnect,
        NicknameWindow,
        Login
    }

    /// <summary>
    /// Base Class for the Packet system, this is used to serialize data to send over the network
    /// </summary>
    [Serializable]
    public class Packet
    {
        /// <summary>
        /// Stores the Type of packet that is being stored
        /// </summary>
        public PacketType mPacketType { get; set; }

        public Packet()
        {
            mPacketType = PacketType.Empty;
        }
    }


    [Serializable]
    public class PrivateMessagePacket : Packet
    {
        public string mMessage;
        public string mSender;
        public string mReciever;

        public PrivateMessagePacket(string sender, string reciever, string message)
        {
            mSender = sender;
            mReciever = reciever;
            mMessage = message;

            mPacketType = PacketType.PrivateMessage;
        }
    }

    /// <summary>
    /// Group Chat Packet system, this stores the message and the sender of the message so the form can add there nickname to the message
    /// </summary>
    [Serializable]
    public class ChatMessagePacket : Packet
    {
        /// <summary>
        /// The message contents
        /// </summary>
        public string mMessage;

        /// <summary>
        /// The nickname of the sender of the message
        /// </summary>
        public string mSender;

        public ChatMessagePacket(string sender, string message)
        {
            mMessage = message;
            mSender = sender;
            mPacketType = PacketType.ChatMessage;
        }
    }

    /// <summary>
    /// Disconnect Packet, this is used to send a disconnected message to the clients in the server
    /// </summary>
    [Serializable]
    public class DisconnectPacket : Packet
    {
        /// <summary>
        /// The nickname of the user who left
        /// </summary>
        public string mNickname;

        public DisconnectPacket(string name)
        {
            mNickname = name;
            mPacketType = PacketType.Disconnect;
        }
    }

    /// <summary>
    /// Set Nickname Packet, this used to send a clients new name to the server for use 
    /// </summary>
    [Serializable]
    public class SetNicknamePacket : Packet
    {
        /// <summary>
        /// The new nickname of the user
        /// </summary>
        public string mNewNickname;

        /// <summary>
        /// The old nickname of the user, this is used for updating the correct nickname on the server
        /// </summary>
        public string mOldNickname;

        public SetNicknamePacket(string oldName, string name)
        {
            mOldNickname = oldName;
            mNewNickname = name;
            mPacketType = PacketType.NewNickname;
        }
    }

    /// <summary>
    /// Nickname Window Packet, this is used for updating the client list of each client 
    /// </summary>
    [Serializable]
    public class NicknameWindowPacket : Packet
    {
        /// <summary>
        /// Stores the sorted nicknames from the server
        /// </summary>
        public List<string> mNames;
        public NicknameWindowPacket(List<string> names)
        {
            mNames = new List<string>(names);
            mPacketType = PacketType.NicknameWindow;
        }
    }

    [Serializable]
    public class LoginPacket : Packet
    {
        public string mEndPoint;
        
        public LoginPacket(string endPoint)
        {
            mEndPoint = endPoint;
            mPacketType = PacketType.Login;
        }
    }

}
