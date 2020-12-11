using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace NetworkedClient
{
    public class GameInstance : Game
    {
        private NetworkStream mStream;
        private BinaryWriter mWriter;
        private BinaryReader mReader;
        private BinaryFormatter mFormatter;

        private UdpClient mUdpClient;
        private TcpClient mTcpClient;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Vector2 tempVelocity;

        private string uniqueID;

        bool mIsConnected;

        Ball player;
        Texture2D ballTexture;

        float ballSpeed;

        float timeSinceLastPositon = float.MaxValue;
        float positionPacketTimer = 1.0f;

        Dictionary<string, Ball> otherPlayers;

        public struct Ball
        {
            public Color PlayerColor;
            public Vector2 Position;
            public Vector2 Velocity;
            public string Id;

            public Ball(string id, Color color, Vector2 pos)
            {
                Id = id;
                PlayerColor = color;
                Position = pos;
                Velocity = new Vector2(0, 0);
            }
        }


        public void AddPlayer(string uid)
        {
            Ball newBall = new Ball(uid, Color.White, new Vector2(400, 240));

            otherPlayers.TryAdd(uid, newBall);
        }

        public void RemovePlayer(string uid)
        {
            otherPlayers.Remove(uid);
        }

        public GameInstance()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            mTcpClient = new TcpClient();

            otherPlayers = new Dictionary<string, Ball>();

            player = new Ball("", Color.White, new Vector2(400, 240));
        }

        ~GameInstance()
        {
            mReader.Close();
            mWriter.Close();
            mTcpClient.Close();
        }

        protected override void Initialize()
        {
            ballSpeed = 100.0f;


            base.Initialize();
        }

        private void SetVelocity(string id, Vector2 velocity)
        {
            Ball tempBall;

            otherPlayers.TryGetValue(id, out tempBall);

            MoveBall(id, velocity);

            tempBall.Velocity = velocity;

            otherPlayers[id] = tempBall;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);


            ballTexture = Content.Load<Texture2D>("ball");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // TODO: Add your update logic here
            var kstate = Keyboard.GetState();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            timeSinceLastPositon += deltaTime;


            if (kstate.IsKeyDown(Keys.Up)) player.Velocity.Y = -ballSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Down)) player.Velocity.Y = ballSpeed * deltaTime;
            else if (kstate.IsKeyUp(Keys.Up) && kstate.IsKeyUp(Keys.Down)) player.Velocity.Y = 0.0f;

            if (kstate.IsKeyDown(Keys.Left)) player.Velocity.X = -ballSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Right)) player.Velocity.X = ballSpeed * deltaTime;
            else if (kstate.IsKeyUp(Keys.Left) && kstate.IsKeyUp(Keys.Right)) player.Velocity.X = 0.0f;

            player.Position += player.Velocity;

            if (tempVelocity != player.Velocity)
            {
                tempVelocity = player.Velocity;
                VelocityPacket velocity = new VelocityPacket(uniqueID, player.Velocity.X, player.Velocity.Y);
                UdpSendMessage(velocity);
            }

            foreach (Ball ball in otherPlayers.Values.ToList())
            {
                MoveBall(ball.Id, ball.Velocity);
            }

            if (timeSinceLastPositon > positionPacketTimer)
            {
                PositionPacket positionPacket = new PositionPacket(uniqueID, player.Position.X, player.Position.Y);
                UdpSendMessage(positionPacket);

                timeSinceLastPositon = 0.0f;
            }


            base.Update(gameTime);
        }

        private void MoveBall(string uid, Vector2 velocity)
        {
            if (uid != null)
            {
                Ball tempBall;


                otherPlayers.TryGetValue(uid, out tempBall);

                tempBall.Position += velocity;

                otherPlayers[uid] = tempBall;

            }
        }

        private void MoveToPosition(string uid, Vector2 position)
        {
            if(uid == uniqueID)
            {
                player.Position = position;
            }
            else
            {
                Ball tempBall;

                otherPlayers.TryGetValue(uid, out tempBall);

                tempBall.Position = position;

                otherPlayers[uid] = tempBall;
            }


        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            //Draw the player
            _spriteBatch.Draw(ballTexture, player.Position, null, Color.White, 0.0f, new Vector2(ballTexture.Width / 2, ballTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f);

            foreach (Ball ball in otherPlayers.Values.ToList())
            {
                if (ball.Id == player.Id)
                {
                    int a = 4;
                    continue;
                }
                _spriteBatch.Draw(ballTexture, ball.Position, null, Color.White, 0.0f, new Vector2(ballTexture.Width / 2, ballTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f);
            }

            _spriteBatch.End();


            base.Draw(gameTime);
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

                mUdpClient = new UdpClient();
                mUdpClient.Connect(ipAddress, port);

                mIsConnected = true;

                ConnectPacket connectPacket = new ConnectPacket();
                SerializePacket(connectPacket);


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
            Thread udpThread = new Thread(UdpProcessServerResponse);
            Thread tcpThread = new Thread(TcpProcessServerResponse);


            LoginPacket loginPacket = new LoginPacket(uniqueID,mUdpClient.Client.LocalEndPoint.ToString());
            SerializePacket(loginPacket);

            udpThread.Start();
            tcpThread.Start();
            base.Run();


        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            DisconnectPacket disconnectPacket = new DisconnectPacket(uniqueID);
            SerializePacket(disconnectPacket);


        }


        public void UdpSendMessage(Packet packet)
        {
            MemoryStream msgStream = new MemoryStream();
            mFormatter.Serialize(msgStream, packet);
            byte[] buffer = msgStream.GetBuffer();
            mUdpClient.Send(buffer, buffer.Length);
        }


        public void UdpProcessServerResponse()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] buffer = mUdpClient.Receive(ref endPoint);

                    MemoryStream stream = new MemoryStream(buffer);

                    Packet recievedPackage = mFormatter.Deserialize(stream) as Packet;

                    switch (recievedPackage.mPacketType)
                    {
                        case PacketType.Velocity:
                            VelocityPacket velocityPacket = (VelocityPacket)recievedPackage;

                            SetVelocity(velocityPacket.mId, new Vector2(velocityPacket.xVel, velocityPacket.yVal));
                            break;
                        case PacketType.Position:
                            PositionPacket positionPacket = (PositionPacket)recievedPackage;

                            MoveToPosition(positionPacket.mId, new Vector2(positionPacket.xPos, positionPacket.yPos));
                            break;
                        default:
                            break;

                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client UDP Read Method Exception: " + e.Message);
            }
        }

        private void TcpProcessServerResponse()
        {
            int numberOfBytes;

            while (mIsConnected)
            {
                if ((numberOfBytes = mReader.ReadInt32()) != 0)
                {
                    if (!mIsConnected) break;

                    byte[] buffer = mReader.ReadBytes(numberOfBytes);

                    MemoryStream stream = new MemoryStream(buffer);

                    Packet recievedPackage = mFormatter.Deserialize(stream) as Packet;

                    switch (recievedPackage.mPacketType)
                    {
                        case PacketType.NewPlayer:
                            NewPlayer newPlayer = (NewPlayer)recievedPackage;

                            AddPlayer(newPlayer.mId);

                            Ball tempPlayer = otherPlayers[uniqueID];
                            PositionPacket position = new PositionPacket(uniqueID, tempPlayer.Position.X, tempPlayer.Position.Y);
                            UdpSendMessage(position);

                            break;

                        case PacketType.Players:
                            Players players = (Players)recievedPackage;
                            foreach (string id in players.mIds)
                            {
                                if (id == uniqueID) continue;
                                AddPlayer(id);
                            }

                            break;
                        case PacketType.GUID:
                            GUID guidPacket = (GUID)recievedPackage;

                            uniqueID = guidPacket.mId;
                            player.Id = uniqueID;
                            otherPlayers.Add(uniqueID, player);

                            break;

                        case PacketType.Disconnect:
                            DisconnectPacket disconnectPacket = (DisconnectPacket)recievedPackage;
                            RemovePlayer(disconnectPacket.mId);
                            break;
                        default:
                            break;
                    }
                }
            }
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
    }
}
