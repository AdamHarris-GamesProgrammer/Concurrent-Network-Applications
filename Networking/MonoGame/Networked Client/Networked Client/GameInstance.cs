﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Packets;
using System;
using System.Collections.Generic;
using System.IO;
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

        bool mIsConnected;
        
        Ball player;
        Texture2D ballTexture;
        float ballSpeed;

        Dictionary<string, Ball> otherPlayers;

        public struct Ball
        {
            public Color PlayerColor;
            public Vector2 Position;
        }


        public void AddPlayer(string uid)
        {
            Ball newBall = new Ball();

            newBall.PlayerColor = Color.White;
            newBall.Position = new Vector2(100, 100);

            otherPlayers.Add(uid, newBall);
        }

        public GameInstance()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            mTcpClient = new TcpClient();

            otherPlayers = new Dictionary<string, Ball>();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            player.Position = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            ballSpeed = 100.0f;


            base.Initialize();
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

            

            if (kstate.IsKeyDown(Keys.Up)) player.Position.Y -= ballSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Down)) player.Position.Y += ballSpeed * deltaTime;

            if (kstate.IsKeyDown(Keys.Left)) player.Position.X -= ballSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Right)) player.Position.X += ballSpeed * deltaTime;



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(ballTexture, player.Position, null, Color.White, 0.0f, new Vector2(ballTexture.Width / 2, ballTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f);

            foreach(Ball ball in otherPlayers.Values)
            {
                _spriteBatch.Draw(ballTexture, ball.Position, null, ball.PlayerColor, 0.0f, new Vector2(ballTexture.Width / 2, ballTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f);
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


            LoginPacket loginPacket = new LoginPacket(mUdpClient.Client.LocalEndPoint.ToString());
            SerializePacket(loginPacket);

            udpThread.Start();
            tcpThread.Start();
            base.Run();
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
                        case PacketType.Connect:
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
                            break;

                        case PacketType.Players:
                            Players players = (Players)recievedPackage;
                            foreach(string id in players.mIds)
                            {
                                AddPlayer(id);
                            }
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
