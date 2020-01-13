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
        private List<Player> players;

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
        DataType dataType;

        public bool gameActive = false;








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
            Console.WriteLine("Sent message to {0} with a {1} containing {2}", player, dataType, message);
        }

        //public void SendMessageToAllPlayers(DataType dataType, Object message)
        //{
        //    NetOutgoingMessage outMsg = server.CreateMessage();
        //    outMsg.Write((byte)dataType);
        //    outMsg.Write(ObjectToByteArray(message));

        //    List<NetConnection> connections = new List<NetConnection>();
        //    foreach (Player player in players) connections.Add(player.netConnection);

        //    server.SendMessage(outMsg, connections, NetDeliveryMethod.ReliableOrdered, 0);
        //}
        
        public void SendMessageToAllPlayers(DataType dataType, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)dataType);
            outMsg.Write(ObjectToByteArray(message));

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine("Sent message to all players with a {0} containing {1}", dataType, message);
        }


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
                            var message = ByteArrayToObject(incMsg.ReadBytes(incMsg.LengthBytes));
                            Console.WriteLine("Player{0} sent a {1}\nContaining: {2}", playerID, dataType.GetTypeCode(), message);
                            
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
                                            players.Remove(player);
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
        }
    }
}
