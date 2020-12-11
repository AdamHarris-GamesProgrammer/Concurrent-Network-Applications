using System;
using System.Collections.Generic;
using System.Net;

namespace Packets
{
    public enum PacketType
    {
        Empty,
        Connect,
        Position,
        Login,
        NewPlayer,
        GUID,
        Players,
        Disconnect,
        Color,
        Velocity
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
    public class ConnectPacket : Packet
    {
        
        public ConnectPacket()
        {
            mPacketType = PacketType.Connect;
        }
    }

    [Serializable]
    public class PositionPacket : Packet
    {
        public float xPos;
        public float yPos;
        public string mId;

        public PositionPacket(string id, float x, float y)
        {
            mId = id;
            xPos = x;
            yPos = y;
            mPacketType = PacketType.Position;
        }
    }

    [Serializable]
    public class VelocityPacket : Packet
    {
        public string mId;
        public float xVel;
        public float yVal;

        public VelocityPacket(string id, float x, float y)
        {
            mId = id;
            xVel = x;
            yVal = y;
            mPacketType = PacketType.Velocity;
        }
    }

    [Serializable]
    public class NewPlayer : Packet
    {
        public string mId;
        
        public NewPlayer(string id)
        {
            mId = id;

            mPacketType = PacketType.NewPlayer;
        }

    }


    [Serializable]
    public class Players : Packet
    {
        public ICollection<string> mIds;

        public Players(ICollection<string> ids)
        {
            mIds = ids;

            mPacketType = PacketType.Players;
        }
    }

    [Serializable] 
    public class GUID : Packet
    {
        public string mId;

        public GUID(string id)
        {
            mId = id;

            mPacketType = PacketType.GUID;
        }
    }


    [Serializable]
    public class LoginPacket : Packet
    {
        public string mId;
        public string mEndPoint;

        public LoginPacket(string id, string endPoint)
        {
            mId = id;
            mEndPoint = endPoint;
            mPacketType = PacketType.Login;
        }
    }

    [Serializable] 
    public class DisconnectPacket : Packet
    {
        public string mId;

        public DisconnectPacket(string id)
        {
            mId = id;
            mPacketType = PacketType.Disconnect;
        }
    }
}


