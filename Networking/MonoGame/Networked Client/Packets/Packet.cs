using System;
using System.Net;

namespace Packets
{
    public enum PacketType
    {
        Empty,
        Connect,
        Position,
        Login
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
        public string mName;
        
        public ConnectPacket(string name)
        {
            mName = name;
            mPacketType = PacketType.Connect;
        }
    }

    [Serializable]
    public class PositionPacket : Packet
    {
        public float xPos;
        public float yPos;
        public float zPos;

        public PositionPacket(float x, float y, float z)
        {
            xPos = x;
            yPos = y;
            zPos = z;

            mPacketType = PacketType.Position;
        }
    }

    [Serializable]
    public class LoginPacket : Packet
    {
        public IPEndPoint mEndPoint;

        public LoginPacket(IPEndPoint endPoint)
        {
            mEndPoint = endPoint;
            mPacketType = PacketType.Login;
        }
    }
}
