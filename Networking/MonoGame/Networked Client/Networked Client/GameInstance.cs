using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Packets;
using System;
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
        
        Texture2D ballTexture;
        Vector2 ballPosition;
        float ballSpeed;


        public GameInstance()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            mTcpClient = new TcpClient();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ballPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
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



            if (kstate.IsKeyDown(Keys.Up)) ballPosition.Y -= ballSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Down)) ballPosition.Y += ballSpeed * deltaTime;

            if (kstate.IsKeyDown(Keys.Left)) ballPosition.X -= ballSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Right)) ballPosition.X += ballSpeed * deltaTime;



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(ballTexture, ballPosition, null, Color.White, 0.0f, new Vector2(ballTexture.Width / 2, ballTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f);
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

                LoginPacket loginPacket = new LoginPacket((IPEndPoint)mUdpClient.Client.LocalEndPoint);
                SerializePacket(loginPacket);


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
            base.Run();


            Thread udpThread = new Thread(UdpProcessServerResponse);



            udpThread.Start();
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
