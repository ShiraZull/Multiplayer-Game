using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace MultiplayerGameServer
{
    class Server
    {
        private NetServer server;

        public enum DataType : byte
        {
            GameInfo, // e.g. PlayerID, How many players, how much time is left to start or if the game is active, reset.
            Board,
            AddBlob,
            SubBlob,
            Direction,

            HeadPos,
            TurnAction // e.g. If the tail shall disepear, to add a body, or becomes dead (collision).
        }
        DataType dataType;

        public bool gameActive = false;
        private TurnManager turnManager = new TurnManager(1000, 2000);
        private Board board = new Board(6);
        private List<Player> players;
        private List<Blob> blobs = new List<Blob>();
        private List<Point> allCoordinates;
        

        public void GameSetup()
        {
            blobs.Add(new Blob(new Point(1,1)));
            Console.WriteLine($"Added blob at {blobs[0].position}");
            allCoordinates = GetAllCoordinates();
            
        }

        public void GameRun()
        {
            turnManager.UpdateTimeDiff();
            if (gameActive)
            {
                turnManager.UpdateTurn();
                if (turnManager.nextTurn)
                {
                    players[0].alive = true;
                    players[0].board = board.size;
                    foreach (Player player in players)
                    {
                        if (player.alive)
                        {
                            player.Move();
                            if (player.CollisionBlob(blobs)) blobs.Remove(player.collidedBlob);
                            else player.MoveBody();
                            foreach (var body in player.bodies)
                            {
                                Console.WriteLine($"Body position: {body.position}");
                            }
                            player.CollisionPlayer(players);
                        }
                        
                    }
                    if(blobs.Count == 0)
                    blobs.Add(new Blob(blobs, players, allCoordinates));
                    Console.WriteLine($"New blob position: {blobs[blobs.Count-1].position}");

                    SendGameData();
                }
            }
        }

        public void RestartGame()
        {
            foreach (Player player in players)
            {
                player.Reset(board.size);
            }
            blobs.Clear();
            if (board.size.X % 2 == 1)
            {
                blobs.Add(new Blob(new Point((board.size.X + 1) / 2, (board.size.Y + 1) / 2)));
            }
            if (board.size.X % 2 == 0)
            {
                blobs.Add(new Blob(new Point(board.size.X / 2, board.size.Y / 2)));
                blobs.Add(new Blob(new Point(board.size.X / 2 +1, board.size.Y / 2)));
                blobs.Add(new Blob(new Point(board.size.X / 2, board.size.Y / 2 +1)));
                blobs.Add(new Blob(new Point(board.size.X / 2 +1, board.size.Y / 2 +1)));
            }

        }

        public void EndGame()
        {

        }

        public void SendGameData()
        {
            foreach (Player player in players)
            {
                if (player.alive)
                {
                    SendMessageToAllPlayers(DataType.HeadPos, "X:" + player.headPos.X);
                    SendMessageToAllPlayers(DataType.HeadPos, "Y:" + player.headPos.Y);
                    if (player.CollisionBlob(blobs))
                    {
                        blobs.Remove(player.collidedBlob);
                        SendMessageToAllPlayers(DataType.SubBlob, $"BlobX:{player.collidedBlob.position.X}");
                        SendMessageToAllPlayers(DataType.SubBlob, $"BlobY:{player.collidedBlob.position.Y}");

                        SendMessageToAllPlayers(DataType.TurnAction, $"Player{player.playerID}: BODY");
                    }
                }
                else SendMessageToAllPlayers(DataType.TurnAction, $"Player{player.playerID}: DEAD");

            }
            foreach (Blob blob in blobs)
            {

            }
        }

        public void AddBlob(int X, int Y)
        {
            blobs.Add(new Blob(new Point(X, Y)));
            SendMessageToAllPlayers(DataType.AddBlob, new Microsoft.Xna.Framework.Point(X, Y));
        }

        public void SubBlob(Blob blob)
        {

        }


        public List<Point> GetAllCoordinates()
        {
            List<Point> allCoordinates = new List<Point>();
            for (int y = 1; y <= board.size.Y; y++)
            {
                for (int x = 1; x <= board.size.X; x++)
                {
                    allCoordinates.Add(new Point(x, y));
                }
            }
            return allCoordinates;
        }


        // A region full of network stuff
        #region Network
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

        public void StartServer()
        {
            var config = new NetPeerConfiguration("MultiplayerGame2020") { Port = 14242 };
            config.MaximumConnections = 4;
            server = new NetServer(config);
            server.Start();

            if (server.Status == NetPeerStatus.Running)
                Console.WriteLine("Server has started... \nPort: " + config.Port);
            else
                Console.WriteLine("Server unable to start...");

            players = new List<Player>();
        }

        public void SendMessage(Player player, DataType dataType, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)dataType);
            outMsg.Write(ObjectToByteArray(message));
            server.SendMessage(outMsg, player.netConnection, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Sent message to {0} with {1} containing {2}", player, dataType, message);
        }
        
        
        public void SendMessageToAllPlayers(DataType dataType, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)dataType);
            outMsg.Write(ObjectToByteArray(message));

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all players with {dataType} containing {message}");
        }
        #endregion

        public void ReadMessages()
        {
            NetIncomingMessage incMsg;

            while ((incMsg = server.ReadMessage()) != null)
            {
                switch (incMsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            var playerID = incMsg.ReadByte();
                            var dataType = incMsg.ReadByte();
                            var message = ByteArrayToObject(incMsg.ReadBytes(incMsg.LengthBytes - 2));
                            Console.WriteLine("Player{0} sent {1} containing: {2}", playerID, (DataType) dataType, message);
                            
                            switch (dataType)
                            {
                                case (byte)DataType.GameInfo:

                                    break;
                                case (byte)DataType.Board:

                                    break;
                                case (byte)DataType.AddBlob:

                                    break;
                                case (byte)DataType.SubBlob:

                                    break;
                                case (byte)DataType.Direction:
                                    if ((byte)message == ((byte)players[playerID - 1].prevDirection + 2) % 4 || (byte)message == (byte)players[playerID - 1].prevDirection)
                                    {
                                        Console.WriteLine("Player{0} direction change request ignored", playerID);
                                        break;
                                    }
                                    players[playerID - 1].direction = (Player.Direction)message;
                                    break;
                                case (byte)DataType.HeadPos:

                                    break;
                                case (byte)DataType.TurnAction:

                                    break;
                            }




                            break;
                        }
                    #region Network
                    case NetIncomingMessageType.DebugMessage:
                        Console.WriteLine(incMsg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        switch (incMsg.SenderConnection.Status)
                        {
                            case NetConnectionStatus.RespondedConnect:
                            Console.WriteLine("Someone tries to connect...");
                                break;

                            case NetConnectionStatus.Connected:
                                players.Add(new Player(incMsg.SenderConnection, (byte)(players.Count+1)));
                                Console.WriteLine("Player{0} has connected succesfully with {1}", players.Count, players[0].netConnection);
                                SendMessage(players[players.Count-1], DataType.GameInfo, "ID:"+ players.Count);
                                break;

                            case NetConnectionStatus.Disconnecting: // TODO: Fix so that the game wont break when someone disconnects!
                                {
                                    foreach (Player player in players)
                                    {
                                        if (player.netConnection == incMsg.SenderConnection)
                                        {
                                            if (gameActive)
                                            {
                                                player.alive = false;
                                            }
                                            else players.Remove(player);
                                            Console.WriteLine("Player{0} is disconnecting...", player.playerID);
                                        }
                                    }
                                }
                                break;

                            case NetConnectionStatus.Disconnected:
                                Console.WriteLine("Someone disconnected...!");
                                break;
                        default:
                            Console.WriteLine("Unhandled message type: {0}", incMsg.SenderConnection.Status);
                            break;
                        }
                        break;
                    default:
                        Console.WriteLine("Unhandled message type: {0}", incMsg.MessageType);
                        break;
                }
                server.Recycle(incMsg);
            }
            #endregion

        }
    }
}
