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


        public void SendMessageToClient(Player player, PacketType packetType, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(ObjectToByteArray(message));
            server.SendMessage(outMsg, player.netConnection, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine($"Sent message to {player} with {packetType} containing {message}");
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

        public void SendMessageToAllClients(PacketType packetType, Object message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(0);
            outMsg.Write(ObjectToByteArray(message));

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
            while ((incMsg = server.ReadMessage()) != null)
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

                            switch (packetType)
                            {
                                case PacketType.GeneralData:
                                    // If message contains ID:i, extract the number i and put it as playerID
                                    if (message.ToString().StartsWith("ID:"))
                                    {
                                        byte i = 1;
                                        while (i <= 4)
                                        {
                                            if (message.ToString().EndsWith(i.ToString()))
                                            {
                                                playerID = i;
                                                Console.WriteLine($"Made playerID as {playerID}");
                                                for (int a = 0; a < i; a++)
                                                {
                                                    gameClient.players.Add(new Player((byte)(a + 1)));
                                                }
                                                Console.WriteLine($">>> There is currently {gameClient.players.Count} players in 'players'");
                                                break;
                                            }
                                            i++;
                                        }
                                    }
                                    break;
                                case PacketType.GridData:

                                    break;
                                case PacketType.PlayerConnected:

                                    break;
                                case PacketType.PlayerDisconnected:

                                    break;
                                case PacketType.StartGame:

                                    break;
                                case PacketType.Direction:

                                    break;
                                case PacketType.HeadPos:

                                    break;
                                case PacketType.AddBlob:

                                    break;
                                case PacketType.SubBlobAddbody:

                                    break;
                                case PacketType.PlayerAlive:

                                    break;
                                case PacketType.EndGame:

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
                server.Recycle(incMsg);
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
                                    gameServer.ReadDirection(playerID, (Player.Direction)message);
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
