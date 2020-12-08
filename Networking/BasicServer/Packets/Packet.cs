using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography; 

namespace Packets
{
    public enum PacketType
    {
        Empty,
        NewNickname,
        Disconnect,
        NicknameWindow,
        Login,
        EncryptedMessage,
        EncryptedPrivateMessage,
        EncryptedNickname
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

        [NonSerialized]
        public RSAParameters mPublicKey;
        
        public LoginPacket(string endPoint, RSAParameters key)
        {
            mEndPoint = endPoint;
            mPublicKey = key;
            mPacketType = PacketType.Login;
        }
    }

    [Serializable]
    public class EncryptedChatMessage : Packet
    {
        public byte[] mNickname;
        public byte[] mMessage;

        public EncryptedChatMessage(byte[] nickname, byte[] message)
        {
            mNickname = nickname;
            mMessage = message;

            mPacketType = PacketType.EncryptedMessage;
        }
    }

    [Serializable]
    public class EncryptedPrivateMessagePacket : Packet
    {
        public byte[] mMessage;
        public byte[] mSender;
        public byte[] mReciever;

        public EncryptedPrivateMessagePacket(byte[] sender, byte[] reciever, byte[] message)
        {
            mSender = sender;
            mReciever = reciever;
            mMessage = message;

            mPacketType = PacketType.EncryptedPrivateMessage;
        }
    }

}
