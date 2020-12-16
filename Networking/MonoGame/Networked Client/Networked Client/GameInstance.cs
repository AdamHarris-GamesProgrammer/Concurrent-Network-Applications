using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Packets;
using System;
using System.Collections.Concurrent;
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
        //Monogame variables
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private NetworkStream mStream;
        private BinaryWriter mWriter;
        private BinaryReader mReader;
        private BinaryFormatter mFormatter;

        private UdpClient mUdpClient;
        private TcpClient mTcpClient;



        private Vector2 mTempVelocity;

        bool mIsConnected;

        Ball mPlayer;
        Texture2D mBallTexture;

        float mBallSpeed;

        float mTimeSinceLastPositionPacket = float.MaxValue;
        float mPositionPacketTimer = 1.0f;

        ConcurrentDictionary<string, Ball> otherPlayers;

        public struct Ball
        {
            public Color PlayerColor;
            public Vector2 Position;
            public Vector2 Velocity;
            public string Id;

            //Ball Constructor
            public Ball(string id, Color color, Vector2 pos)
            {
                Id = id;
                PlayerColor = color;
                Position = pos;
                Velocity = new Vector2(0, 0);
            }
        }

        /// <summary>
        /// Adds a player to the player list
        /// </summary>
        public void AddPlayer(string uid)
        {
            Ball newBall = new Ball(uid, Color.White, new Vector2(400, 240));

            otherPlayers.TryAdd(uid, newBall);
        }

        /// <summary>
        /// Removes a player from the player list
        /// </summary>
        public void RemovePlayer(string uid)
        {
            Ball outBall;
            otherPlayers.TryRemove(uid, out outBall);
        }

        //Constructor for the game class
        public GameInstance()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            //Initializes the TCP client
            mTcpClient = new TcpClient();

            otherPlayers = new ConcurrentDictionary<string, Ball>();

            //Initializes the Player object
            mPlayer = new Ball("", Color.White, new Vector2(400, 240));
        }

        //Destructor for the game class
        ~GameInstance()
        {
            mReader.Close();
            mWriter.Close();
            mTcpClient.Close();
        }

        protected override void Initialize()
        {
            //Initializes the ball speed
            mBallSpeed = 100.0f;


            base.Initialize();
        }

        /// <summary>
        /// Sets the velocity of the desired velocity
        /// </summary>
        private void SetVelocity(string id, Vector2 velocity)
        {
            //Temp ball object used for setting the velocity
            Ball tempBall;

            //Attempts to get the ball based on the passed in id
            otherPlayers.TryGetValue(id, out tempBall);

            tempBall.Id = id;

            //Sets the velocity
            tempBall.Velocity = velocity;

            MoveBall(id, velocity);

            //Sets the player lists instance of the ball to the temp ball object
            otherPlayers[id] = tempBall;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);


            mBallTexture = Content.Load<Texture2D>("ball");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// Update method called during in MonoGames game loop. Used in this case to handle input and move players
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //Allows the player to close the game window by pressing escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //Gets the keyboard state
            var kstate = Keyboard.GetState();

            //Short hand for the delta time between frames
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Adds the delta time to the position timer
            mTimeSinceLastPositionPacket += deltaTime;

            //If the Up or Down arrow is down then set the velocity
            if (kstate.IsKeyDown(Keys.Up)) mPlayer.Velocity.Y = -mBallSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Down)) mPlayer.Velocity.Y = mBallSpeed * deltaTime;
            //If both the keys are up then set the velocity to 0
            else if (kstate.IsKeyUp(Keys.Up) && kstate.IsKeyUp(Keys.Down)) mPlayer.Velocity.Y = 0.0f;

            //If the left or right arrow is down then set the velocity
            if (kstate.IsKeyDown(Keys.Left)) mPlayer.Velocity.X = -mBallSpeed * deltaTime;
            else if (kstate.IsKeyDown(Keys.Right)) mPlayer.Velocity.X = mBallSpeed * deltaTime;
            //If both the left and right arrow keys are up then set the velocity to 0
            else if (kstate.IsKeyUp(Keys.Left) && kstate.IsKeyUp(Keys.Right)) mPlayer.Velocity.X = 0.0f;

            if (kstate.IsKeyDown(Keys.D))
            {
                int a = 3;
            }

            //Adds the velocity to the players position
            AddVelocity(ref mPlayer, mPlayer.Velocity);

            //Ensures a velocity packet is only sent when needed 
            if (mTempVelocity != mPlayer.Velocity)
            {
                //Resets the temp velocity to the players current velocity
                mTempVelocity = mPlayer.Velocity;
                
                //Serializes and Sends a velocity packet over the network via TCP
                VelocityPacket velocity = new VelocityPacket(mPlayer.Id, mPlayer.Velocity.X, mPlayer.Velocity.Y);
                UdpSendMessage(velocity);
            }

            //Moves each of the balls in the player list
            foreach (Ball ball in otherPlayers.Values.ToList())
            {
                MoveBall(ball.Id, ball.Velocity);
            }

            //if the time since the last position packet is greater than the timer
            if (mTimeSinceLastPositionPacket > mPositionPacketTimer)
            {
                //Then serialize and send a position packet over the network via TCP
                PositionPacket positionPacket = new PositionPacket(mPlayer.Id, mPlayer.Position.X, mPlayer.Position.Y);
                UdpSendMessage(positionPacket);

                //Resets the timer variable
                mTimeSinceLastPositionPacket = 0.0f;
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// Moves the ball based on the passed in velocity
        /// </summary>
        private void MoveBall(string uid, Vector2 velocity)
        {
            //Checks the uid is not null 
            if (uid != null)
            {
                //Creates a temp ball object to modify
                Ball tempBall;
    
                //Attempts to get the ball from the player list
                otherPlayers.TryGetValue(uid, out tempBall);

                //Adds the velocity to the balls position
                AddVelocity(ref tempBall, velocity);

                //Sets the lists instance of the ball equal to the temp ball object
                otherPlayers[uid] = tempBall;

            }
        }

        /// <summary>
        /// This method adds the velocity to the passed in balls position and confines it to screen space
        /// </summary>
        private void AddVelocity(ref Ball ball, Vector2 velocity)
        {
            //Adds the velocity
            ball.Position += velocity;

            //Checks the right bound of the screen
            if(ball.Position.X > _graphics.PreferredBackBufferWidth - mBallTexture.Width / 2)
            {
                ball.Position.X = _graphics.PreferredBackBufferWidth - mBallTexture.Width / 2;
            }
            //Checks the left bound of the screen
            else if(ball.Position.X < mBallTexture.Width / 2)
            {
                ball.Position.X = mBallTexture.Width / 2;
            }

            //Checks the bottom bound of the screen
            if(ball.Position.Y > _graphics.PreferredBackBufferHeight - mBallTexture.Height / 2)
            {
                ball.Position.Y = _graphics.PreferredBackBufferHeight - mBallTexture.Height / 2;
            }
            //Checks the top bound of the screen
            else if(ball.Position.Y < mBallTexture.Height / 2)
            {
                ball.Position.Y = mBallTexture.Height / 2;
            }
        }

        /// <summary>
        /// Moves a specified ball to a specified position used with the position packet
        /// </summary>
        private void MoveToPosition(string uid, Vector2 position)
        {
            //Checks to see if the unique identifier passed in is the players identifier
            if (uid == mPlayer.Id)
            {
                //Sets the players position
                mPlayer.Position = position;
            }
            else
            {
                //Creates a temp ball object
                Ball tempBall;

                //Gets the ball based on the unique ID passed in
                otherPlayers.TryGetValue(uid, out tempBall);

                tempBall.Id = uid;

                //Sets the position of the temp ball to the passed in position
                tempBall.Position = position;

                //Sets the players ball position equal to the temp balls position
                otherPlayers[uid] = tempBall;
            }
        }

        /// <summary>
        /// Draws the game to the screen
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //Clears the back buffers
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Begins the sprite drawing
            _spriteBatch.Begin();

            //Draws the player
            _spriteBatch.Draw(mBallTexture, mPlayer.Position, null, Color.White, 0.0f, new Vector2(mBallTexture.Width / 2, mBallTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f);

            //Draws the other players
            foreach (Ball ball in otherPlayers.Values.ToList())
            {
                if (ball.Id == mPlayer.Id) continue;
                
                //Draws all other balls
                _spriteBatch.Draw(mBallTexture, ball.Position, null, Color.White, 0.0f, new Vector2(mBallTexture.Width / 2, mBallTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.0f);
            }

            //ends the sprite drawing 
            _spriteBatch.End();


            base.Draw(gameTime);
        }

        /// <summary>
        /// Allows the client to connect to the server
        /// </summary>
        public bool Connect(string ipAddress, int port)
        {
            try
            {
                //connects the tcp client to the server
                mTcpClient.Connect(ipAddress, port);

                //Sets the stream to read from the tcp stream
                mStream = mTcpClient.GetStream();

                //Initializes the writer, reader and formatter
                mWriter = new BinaryWriter(mStream, Encoding.UTF8);
                mReader = new BinaryReader(mStream, Encoding.UTF8);
                mFormatter = new BinaryFormatter();

                //Initializes the UDP client and connects to the server
                mUdpClient = new UdpClient();
                mUdpClient.Connect(ipAddress, port);
                
                //Sets is connected to true
                mIsConnected = true;

                //Sends a Connect Packet over the network via TCP
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

        /// <summary>
        /// Run method is used to start the networking and graphics side of the program
        /// </summary>
        public void Run()
        {
            //Creates a thread for processing the TCP and UDP aspects of the project
            Thread udpThread = new Thread(UdpProcessServerResponse);
            Thread tcpThread = new Thread(TcpProcessServerResponse);

            //Sends a login packet via TCP
            LoginPacket loginPacket = new LoginPacket(mPlayer.Id, mUdpClient.Client.LocalEndPoint.ToString());
            SerializePacket(loginPacket);

            //Starts the UDP and TCP threads
            udpThread.Start();
            tcpThread.Start();

            //Starts the Monogame game loop
            base.Run();
        }

        /// <summary>
        /// OnExiting event called by MonoGame, in this case it is used to call the disconnect packet
        /// </summary>
        protected override void OnExiting(object sender, EventArgs args)
        {
            DisconnectPacket disconnectPacket = new DisconnectPacket(mPlayer.Id);
            SerializePacket(disconnectPacket);
        }

        /// <summary>
        /// Sends a packet via UDP
        /// </summary>
        public void UdpSendMessage(Packet packet)
        {
            MemoryStream msgStream = new MemoryStream();
            //Serializes the packet into the stream
            mFormatter.Serialize(msgStream, packet);
            //Gets the bytes buffer from the stream
            byte[] buffer = msgStream.GetBuffer();
            //Sends the buffer over the network via UDP
            mUdpClient.Send(buffer, buffer.Length);
        }

        /// <summary>
        /// Processes any UDP packets received from the network
        /// </summary>
        public void UdpProcessServerResponse()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    //Gets any UDP packets sent
                    byte[] buffer = mUdpClient.Receive(ref endPoint);

                    MemoryStream stream = new MemoryStream(buffer);

                    //Deserializes the received packet
                    Packet recievedPacket = mFormatter.Deserialize(stream) as Packet;

                    //Processes the packet depending if it is a velocity or a position packet
                    switch (recievedPacket.mPacketType)
                    {
                        case PacketType.Velocity:
                            VelocityPacket velocityPacket = (VelocityPacket)recievedPacket;


                            if(velocityPacket.mId != mPlayer.Id)
                            {
                                int a = 5;
                            }

                            //Sets the velocity of the referred player to the passed in velocity
                            SetVelocity(velocityPacket.mId, new Vector2(velocityPacket.xVel, velocityPacket.yVal));
                            break;
                        case PacketType.Position:
                            PositionPacket positionPacket = (PositionPacket)recievedPacket;

                            //Sets the position of the referred player to the passed in position
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

        /// <summary>
        /// Processes any received TCP packages
        /// </summary>
        private void TcpProcessServerResponse()
        {
            //Used to store the number of bytes that need to be read from the network
            int numberOfBytes;

            while (mIsConnected)
            {
                //sets the number of bytes equal to the size from the reader
                if ((numberOfBytes = mReader.ReadInt32()) != 0)
                {
                    //Breaks out of loop if we are no longer connected
                    if (!mIsConnected) break;

                    //Gets the bytes for the buffer from the reader
                    byte[] buffer = mReader.ReadBytes(numberOfBytes);

                    MemoryStream stream = new MemoryStream(buffer);

                    //Deserializes the data as a packet
                    Packet recievedPacket = mFormatter.Deserialize(stream) as Packet;

                    //Tests what type of packet we have received 
                    switch (recievedPacket.mPacketType)
                    {
                        case PacketType.NewPlayer:
                            //Casts the recievedPacket into a new player packet
                            NewPlayer newPlayer = (NewPlayer)recievedPacket;

                            //Adds the new player to the list of players 
                            AddPlayer(newPlayer.mId);

                            //Sends this players position to the new player
                            PositionPacket position = new PositionPacket(mPlayer.Id, mPlayer.Position.X, mPlayer.Position.Y);
                            UdpSendMessage(position);
                            break;

                        case PacketType.Players:
                            //Casts the received packet to the Players packet
                            Players players = (Players)recievedPacket;

                            //Adds each player to the local list
                            foreach (string id in players.mIds)
                            {
                                if (id == mPlayer.Id) continue;
                                AddPlayer(id);
                            }
                            break;
                        case PacketType.GUID:
                            //Casts the received packet into a GUID packet
                            GUID guidPacket = (GUID)recievedPacket;

                            //Sets the unique id for ourself and the player
                            mPlayer.Id = guidPacket.mId;
                            //Adds the player to the other players list

                            
                            otherPlayers.TryAdd(mPlayer.Id, mPlayer);
                            break;

                        case PacketType.Disconnect:
                            //Casts the received Packet to a disconnect packet
                            DisconnectPacket disconnectPacket = (DisconnectPacket)recievedPacket;
                            //Removes the disconnected player
                            RemovePlayer(disconnectPacket.mId);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Serializes a Packet and sends it via TCP
        /// </summary>
        private void SerializePacket(Packet packetToSerialize)
        {
            MemoryStream msgStream = new MemoryStream();
            //Serializes the packet and places the data into the msgStream
            mFormatter.Serialize(msgStream, packetToSerialize);

            //Creates a byte array for the packet data
            byte[] buffer = msgStream.GetBuffer();

            //Writes and then sends the data over the network
            mWriter.Write(buffer.Length);
            mWriter.Write(buffer);
            mWriter.Flush();
        }
    }
}
