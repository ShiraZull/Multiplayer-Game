using Lidgren.Network;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;
using Tools_XNA_dotNET_Framework;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MultiplayerGame
{
    class Client
    {
        private NetClient client;

        // A identification for what kind of data is sent
        public enum DataType
        {
            GameInfo, // e.g. PlayerID, How many players, how much time is left to start or if the game is active, reset.
            Board,
            AddBlob,
            SubBlob,
            Direction,

            HeadPos,
            TurnAction // e.g. If the tail shall disepear, to add a body, or becomes dead (collision).
        }

        // Local
        public byte playerID;

        // Player
        public List<Player> players = new List<Player>(4);

        // GameBoard
        public int gridSize = 6;
        public int squareSize;
        public int lineSize;
        public int playArea;
        public Vector2 gridPosition;


        public void Draw(SpriteBatch spriteBacth)
        {
            Restart();

            foreach (Player player in players)
            {

            }
            for(int x = 0; x < 4; x++)
                for(int y = 0; y < 4; y++)
                {
                    if (x == 0)
                        Primitives2D.DrawFilledRectangle(spriteBacth, new Rectangle((int)gridPosition.X + squareSize * x, (int)gridPosition.Y + squareSize * y, squareSize, squareSize), playerColor((byte)(y + 1)));
                    else
                    {
                        Primitives2D.DrawFilledRectangle(spriteBacth, new Rectangle((int)gridPosition.X + squareSize * x, (int)gridPosition.Y + squareSize * y, squareSize, squareSize), new Color(playerColor((byte)(y + 1)), 0.2f));
                    }
                }
            Primitives2D.DrawFilledRectangle(spriteBacth, new Rectangle((int)gridPosition.X + squareSize * 4, (int)gridPosition.Y + squareSize * 4, squareSize, squareSize), new Color(playerColor((byte)(0 + 1)), 0f));
            //Primitives2D.DrawFilledRectangle(spriteBacth, new Rectangle((int)gridPosition.X + squareSize * 4, (int)gridPosition.Y + squareSize * 4, squareSize, squareSize), playerColor(true, 1));
            //Primitives2D.DrawGrid(spriteBacth, gridSize, gridSize, squareSize, gridPosition, Color.White, lineSize);
        }

        public Color playerColor(byte playerID)
        {
            Color color = new Color();
                if (playerID == 1)
                {
                    color = Color.Aqua;
                    return color;
                }
                if (playerID == 2)
                {
                    color = Color.Magenta;
                    return color;
                }
                if (playerID == 3)
                {
                    color = Color.Lime;
                    return color;
                }
                if (playerID == 4)
                {
                    color = Color.Yellow;
                    return color;
                }
            
            color = Color.Gray;
            return color;
        }

        public void Restart()
        {
            squareSize = (600 - (gridSize + 1) * lineSize) / gridSize;
            if(gridSize>20) lineSize = 1;
            else lineSize = squareSize/(20);
            playArea = gridSize * squareSize;
            gridPosition = new Vector2((600 - playArea + lineSize) / (2), 200 + (600 - playArea - lineSize) / 2);
        }


        // Convert an Object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }




        public void StartClient()
        {
            var config = new NetPeerConfiguration("MultiplayerGame2020");
            config.AutoFlushSendQueue = false;
            client = new NetClient(config);
            client.Start();

            string ip = "localhost";
            int port = 14242;
            client.Connect(ip, port);
        }

        public void SendMessage(DataType dataType, Object message)
        {
            NetOutgoingMessage outMsg = client.CreateMessage();
            outMsg.Write(playerID);
            outMsg.Write((byte)dataType);
            outMsg.Write(ObjectToByteArray(message));
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
        }

        public void ReadMessage()
        {
            NetIncomingMessage incMsg;

            while ((incMsg = client.ReadMessage()) != null)
            {
                switch (incMsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            var dataType = incMsg.ReadByte();
                            var message = ByteArrayToObject(incMsg.ReadBytes(incMsg.LengthBytes - 1));

                            switch (dataType)
                            {
                                case (byte)DataType.GameInfo:

                                    // If message contains ID:i, extract the number i and put it as playerID
                                    if(message.ToString().StartsWith("ID:"))
                                    {
                                        
                                        byte i = 1;
                                        while (i <= 4)
                                        {
                                            if(message.ToString().EndsWith(i.ToString()))
                                            {
                                                playerID = i;
                                                Console.WriteLine("playerID = {0}", playerID);
                                                break;
                                            }
                                            i++;
                                        }
                                    }

                                    break;
                                case (byte)DataType.Board:

                                    break;
                                case (byte)DataType.AddBlob:

                                    break;
                                case (byte)DataType.SubBlob:

                                    break;
                                case (byte)DataType.Direction:

                                    break;
                                case (byte)DataType.HeadPos:

                                    break;
                                case (byte)DataType.TurnAction:

                                    break;
                            }




                            break;
                        }
                    case NetIncomingMessageType.DebugMessage:
                        Console.WriteLine(incMsg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        Console.WriteLine(incMsg.SenderConnection.Status);
                        break;
                    default:
                        Console.WriteLine("Unhandled message type: {0}", incMsg.MessageType);
                        break;
                }
                client.Recycle(incMsg);
            }
        }

        public bool IsConnectedToServer()
        {
            if(client.ConnectionsCount > 0)
                return true;
            else return false;
        }

        public void Connect(string ip, int port)
        {
            client.Connect(ip, port);
        }

        public void Disconnect(string playerID)
        {
            client.Disconnect(playerID + " is disconnecting");
        }

        

    }
}
