using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace MultiplayerGameLibrary
{
    public class MessageManager
    {

        private NetServer server;
        private NetClient client;

        public MessageManager(NetServer server) { this.server = server; }
        public MessageManager(NetClient client) { this.client = client; }

        public enum PacketType : byte
        {
            // General
            GeneralData,    // e.g. PlayerID, How many players, how much time is left to start or if the game is active, reset. <---- Old comment
            // Lobby
            GridData,       // Info about grid before creating a new game
            PlayerConnected,// Info if another player connects to the lobby
            PlayerDisconnected, // Info if a player dissconnects from the lobby
            StartGame,      // Start the game

            // InGame
            Direction,      // From client
            HeadPos,        // Can also be used as a "TurnManager", every time head updates it's a new turn which is used to delete last bodypart (tail)
            AddBlob,        // Adds a blob on the grid
            SubBlobAddbody, // Includes score
            PlayerAlive,    // Collision with player
            EndGame         // End the game
        }
        public PacketType packetType;




        public void SendMessageToClient(Player player, PacketType packetType, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(0);
            outMsg.Write(ObjectToByteArray(message));
            server.SendMessage(outMsg, player.netConnection, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to {player.netConnection} with {packetType} containing {message}");
        }

        public void SendMessageToAllClients(PacketType packetType, byte playerID, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(playerID);
            outMsg.Write(ObjectToByteArray(message));

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} about player{playerID} containing {message}");
        }

        public void SendMessageToAllClients(PacketType packetType, byte playerID, Point message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(playerID);
            outMsg.Write(ObjectToByteArray(message.X));
            outMsg.Write(ObjectToByteArray(message.Y));

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} about player{playerID} containing {message}");
        }

        public void SendMessageToAllClients(PacketType packetType, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(0);
            outMsg.Write(ObjectToByteArray(message));

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} containing {message}");
        }

        public void SendMessageToAllClients(PacketType packetType, Point message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(0);
            outMsg.Write(ObjectToByteArray(message.X));
            outMsg.Write(ObjectToByteArray(message.Y));

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} containing {message}");
        }

        public void SendMessageToServer(byte playerID, PacketType packetType, Object message)
        {
            NetOutgoingMessage outMsg = client.CreateMessage();
            outMsg.Write(playerID);
            outMsg.Write((byte)packetType);
            outMsg.Write(ObjectToByteArray(message));
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
            Console.WriteLine($"Sent message to server as player{playerID} with {packetType} containing {message}");
        }


        public void ReadServerMessages(Client gameClient)
        {
            NetIncomingMessage incMsg;
            while ((incMsg = client.ReadMessage()) != null)
            {
                switch (incMsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            PacketType packetType = (PacketType)incMsg.ReadByte();
                            byte playerID = (byte)incMsg.ReadByte();
                            var message = ByteArrayToObject(incMsg.ReadBytes(incMsg.LengthBytes - 2));
                            if (playerID == 0) Console.WriteLine($"Server sent {packetType} containing: {message}");
                            else Console.WriteLine($"Server sent {packetType} regarding player{playerID} containing: {message}");

                            Point coordinateMessage = new Point(-13);
                            if (packetType == PacketType.GridData || packetType == PacketType.HeadPos || packetType == PacketType.AddBlob || packetType == PacketType.SubBlobAddbody)
                            {
                                coordinateMessage = new Point((int)message, (int)ByteArrayToObject(incMsg.ReadBytes(incMsg.LengthBytes - 3)));
                            }

                            switch (packetType)
                            {
                                case PacketType.GeneralData:
                                    gameClient.ReadGeneralData(message);
                                    break;
                                case PacketType.GridData:
                                    gameClient.ReadGridData(coordinateMessage);
                                    break;
                                case PacketType.PlayerConnected:
                                    gameClient.ReadPlayerConnected(playerID, (NetConnection)message); //TODO fix so there is no NetConnection
                                    break;
                                case PacketType.PlayerDisconnected:
                                    gameClient.ReadPlayerDisconnected(playerID);
                                    break;
                                case PacketType.StartGame:
                                    if ((string)message == "Ready") gameClient.ReadStartGame(true);
                                    else if ((string)message == "Start") gameClient.ReadStartGame(false);
                                    else Console.WriteLine($"Unhandled StartGame Type: {message}");
                                    
                                    break;
                                case PacketType.HeadPos:
                                    gameClient.ReadHeadPos(playerID, coordinateMessage);
                                    break;
                                case PacketType.AddBlob:
                                    gameClient.ReadAddBlob(coordinateMessage);
                                    break;
                                case PacketType.SubBlobAddbody:
                                    gameClient.ReadSubBlobAddBody(playerID, coordinateMessage);
                                    break;
                                case PacketType.PlayerAlive:
                                    gameClient.ReadPlayerAlive(playerID, (bool)message);
                                    break;
                                case PacketType.EndGame:
                                    gameClient.ReadEndGame();
                                    break;
                                default:
                                    Console.WriteLine($"Unhandled packetType: {packetType}");
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

                                break;

                            case NetConnectionStatus.Disconnecting: 
                                
                                break;

                            case NetConnectionStatus.Disconnected:
                                break;
                            default:
                                Console.WriteLine($"Unhandled message type: {incMsg.SenderConnection.Status}");
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine($"Unhandled message type: {incMsg.MessageType}");
                        break;
                }
                #endregion
                client.Recycle(incMsg);
            }
        }

        public void ReadClientMessages(Server gameServer)
        {
            NetIncomingMessage incMsg;
            while ((incMsg = server.ReadMessage()) != null)
            {
                switch (incMsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            var playerID = incMsg.ReadByte();
                            PacketType packetType = (PacketType)incMsg.ReadByte();
                            var message = ByteArrayToObject(incMsg.ReadBytes(incMsg.LengthBytes - 2));
                            Console.WriteLine($"Player{playerID} sent {packetType} containing: {message}");

                            switch (packetType)
                            {
                                case PacketType.GeneralData:
                                    
                                    break;
                                case PacketType.Direction:
                                    gameServer.ReadDirection(playerID, (byte)message);
                                    break;
                                default:
                                    Console.WriteLine($"Unhandled packageType: {packetType}");
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
                                Console.WriteLine($"{incMsg.SenderConnection} tries to connect...");
                                break;

                            case NetConnectionStatus.Connected:
                                gameServer.PlayerConnected(incMsg.SenderConnection);
                                break;

                            case NetConnectionStatus.Disconnected:
                                gameServer.PlayerDisconnected(incMsg.SenderConnection);
                                break;
                            default:
                                Console.WriteLine($"Unhandled message type: {incMsg.SenderConnection.Status}");
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine($"Unhandled message type: {incMsg.MessageType}");
                        break;
                }
                #endregion
                server.Recycle(incMsg);
            }
        }

        // Convert an Object to a byte array
        private static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        private static Object ByteArrayToObject(byte[] arrBytes)
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


    }
}
