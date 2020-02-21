using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;
using Tools_XNA_dotNET_Framework;

namespace MultiplayerGameLibrary
{
    public class Client
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


        public void Draw(SpriteBatch spriteBatch)
        {
            RestartGrid();

            foreach (Player player in players)
            {
                DrawPlayerPart(spriteBatch, player.headPos, player.playerID, 1f);
                foreach (Body body in player.bodies)
                {
                    DrawPlayerPart(spriteBatch, body.position, player.playerID, 0.7f);
                }
            }
            Primitives2D.DrawGrid(spriteBatch, gridSize, gridSize, squareSize, gridPosition, Color.White, lineSize);
        }

        public void DrawPlayerPart(SpriteBatch spriteBatch, Point position, byte playerID, float alpha)
        {
            Primitives2D.DrawFilledRectangle(spriteBatch, new Rectangle((int)gridPosition.X + squareSize * (position.X - 1), (int)gridPosition.Y + squareSize * (position.Y - 1), squareSize, squareSize), playerColor(playerID, alpha));
        }

        public Color playerColor(byte playerID, float alpha)
        {
            Color color = new Color();
            if (playerID == 1)
            {
                color = new Color(Color.Aqua, alpha);
                return color;
            }
            if (playerID == 2)
            {
                color = new Color(Color.Magenta, alpha);
                return color;
            }
            if (playerID == 3)
            {
                color = new Color(Color.Lime, alpha);
                return color;
            }
            if (playerID == 4)
            {
                color = new Color(Color.Yellow, alpha);
                return color;
            }

            color = Color.Gray;
            return color;
        }


        public void RestartGrid()
        {
            squareSize = (600 - (gridSize + 1) * lineSize) / gridSize;
            if (gridSize > 20) lineSize = 1;
            else lineSize = squareSize / (20);
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


        // Variables for Reading messages in order
        byte headPosPlayerIndex = 0;



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
                            Console.WriteLine($"Server sent a {(DataType)dataType} containing {message}");

                            switch (dataType)
                            {
                                case (byte)DataType.GameInfo:

                                    // If message contains ID:i, extract the number i and put it as playerID
                                    if (message.ToString().StartsWith("ID:"))
                                    {

                                        byte i = 1;
                                        while (i <= 4)
                                        {
                                            if (message.ToString().EndsWith(i.ToString()))
                                            {
                                                playerID = i;
                                                Console.WriteLine("Made playerID as {0}", playerID);
                                                for (int a = 0; a < i; a++)
                                                {
                                                    players.Add(new Player((byte)(a + 1)));
                                                }
                                                Console.WriteLine($">>> There is currently {players.Count} players in 'players'");
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
                                case (byte)DataType.HeadPos:

                                    if (message.ToString().StartsWith("X:"))
                                    {
                                        players[headPosPlayerIndex].headPos.X = int.Parse(message.ToString().Remove(0, 2));
                                    }
                                    else if (message.ToString().StartsWith("Y:"))
                                    {
                                        players[headPosPlayerIndex].headPos.Y = int.Parse(message.ToString().Remove(0, 2));
                                        headPosPlayerIndex++;
                                    }
                                    else Console.WriteLine($"Unhandled message: {message.ToString()}");

                                    if (headPosPlayerIndex == players.Count)
                                    {
                                        headPosPlayerIndex = 0;
                                    }

                                    break;
                                case (byte)DataType.TurnAction:

                                    break;
                                default:
                                    Console.WriteLine($"Unhandled DataType: {(DataType)dataType}");
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
            if (client.ConnectionsCount > 0)
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
