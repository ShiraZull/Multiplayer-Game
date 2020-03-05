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
        // Using the built in classes for handling network connectivity and communication from Lidgren in Monogame.
        private NetServer server;
        private NetClient client;

        public MessageManager(NetServer server) { this.server = server; }
        public MessageManager(NetClient client) { this.client = client; }

        // A byte that declares what type of message the server or client is sending, to be able and sort out what kind of data it is and what application it has.
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

        // -------------------------------------------------------------------
        // OBS: See StartClient() in Client.cs and StartServer() in Server.cs for some more information about the initialization steps of lidgren.
        // -------------------------------------------------------------------

        /// <summary>
        /// Server sends a message to a specific client in the player list, containing a packetType and a message
        /// </summary>
        /// <param name="player">Which player the message shall be sent to</param>
        /// <param name="packetType">what type of message it is</param>
        /// <param name="message">what information shall be sent to client</param>
        public void SendMessageToClient(Player player, PacketType packetType, string message)
        {
            // Creates a message that can be sent
            NetOutgoingMessage outMsg = server.CreateMessage();
            // Puts in what type shall be sent, in form of a byte
            outMsg.Write((byte)packetType);
            // A placeholder to make the old readcode on the client to actually work, this basically declares that this is a general message
            outMsg.Write((byte)0);
            // Write inte information contained in the message parameter
            outMsg.Write(message);
            // Server sends the message to the specified player and their connection (ip) using 
            // a ReliableOrdered delivery method (the message shall be read in order and shall be sent reliable which means 
            // it should not be lost on their way to the client) and lastly a specified channel which is mandatory for the server 
            // to send stuff to client, it's purpose is for the coder to be able to control if the newest 
            // message is important and the old ones can be ignored.
            server.SendMessage(outMsg, player.netConnection, NetDeliveryMethod.ReliableOrdered, 0);
            // Write what the server just sent in the console
            Console.WriteLine($"Sent message to {player.netConnection} with {packetType} containing {message}");
        }

        /// <summary>
        /// Similar to SendMessageToClient(player, packetType, message)
        /// But this one specify which player the message is about and it sends that message to all clients using server.connections 
        /// instead of all player.connections in players, so this include clients without a player identity.
        /// This method handles bools, there are multiple methods for different message types.
        /// </summary>
        /// <param name="packetType">What type of message it is</param>
        /// <param name="playerID">A variable that defines who the message is about. E.g. used for player head, 
        /// if you send a position you also need to specify which one of the players that has that position </param>
        /// <param name="message">What information shall be sent to client</param>
        public void SendMessageToAllClients(PacketType packetType, byte playerID, bool message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            // Here is the player that the message specifies
            outMsg.Write(playerID);
            outMsg.Write(message);
            // Here is server.connections used instead of the playerlist, which includes all clients connected to server.
            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} about player{playerID} containing {message}");
        }
        public void SendMessageToAllClients(PacketType packetType, byte playerID, byte message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(playerID);
            outMsg.Write(message);

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} about player{playerID} containing {message}");
        }
        public void SendMessageToAllClients(PacketType packetType, byte playerID, string message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(playerID);
            outMsg.Write(message);

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} about player{playerID} containing {message}");
        }
        public void SendMessageToAllClients(PacketType packetType, byte playerID, Point message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write(playerID);
            outMsg.Write(message.X);
            outMsg.Write(message.Y);

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} about player{playerID} containing {message}");
        }

        /// <summary>
        /// Similar to SendMessageToClient(player, packetType, message)
        /// But this one sends the message to all clients using server.connections 
        /// instead of all player.connections in players, so this include clients without a player identity.
        /// This method handles bools, there are multiple methods for different message types.
        /// </summary>
        /// <param name="packetType">What type of message it is</param>
        /// <param name="message">What information shall be sent to client</param>
        public void SendMessageToAllClients(PacketType packetType, bool message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            // The zero represents that the message is a general message, like what position food is at.
            outMsg.Write((byte)0);
            outMsg.Write(message);

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} containing {message}");
        }
        public void SendMessageToAllClients(PacketType packetType, byte message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write((byte)0);
            outMsg.Write(message);

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} containing {message}");
        }
        public void SendMessageToAllClients(PacketType packetType, string message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write((byte)0);
            outMsg.Write(message);

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} containing {message}");
        }
        public void SendMessageToAllClients(PacketType packetType, Point message)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)packetType);
            outMsg.Write((byte)0);
            outMsg.Write(message.X);
            outMsg.Write(message.Y);

            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            Console.WriteLine($"Sent message to all clients with {packetType} containing {message}");
        }

        /// <summary>
        /// Similar to SendMessageToClient(player, packetType, message)
        /// But this one sends the message to the server containing an identification which make the server be able to read which client this message is from.
        /// This also doesn't have a channel in the end of the client.SendMessage() method. As well is it the client that sends this message instead of the server.
        /// This method handles bools, there are multiple methods for different message types.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="packetType"></param>
        /// <param name="message"></param>
        public void SendMessageToServer(byte playerID, PacketType packetType, byte message)
        {
            NetOutgoingMessage outMsg = client.CreateMessage();
            outMsg.Write(playerID);
            outMsg.Write((byte)packetType);
            outMsg.Write(message);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
            // A method to flush the send buffer, this can be done automatically but for some reason I'm using it...
            client.FlushSendQueue();
            Console.WriteLine($"Sent message to server as player{playerID} with {packetType} containing {message}");
        }
        public void SendMessageToServer(byte playerID, PacketType packetType, string message)
        {
            NetOutgoingMessage outMsg = client.CreateMessage();
            outMsg.Write(playerID);
            outMsg.Write((byte)packetType);
            outMsg.Write(message);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
            Console.WriteLine($"Sent message to server as player{playerID} with {packetType} containing {message}");
        }

        /// <summary>
        /// Reads the messages from the server, checks if the message contains something, 
        /// looks at what messagetype it is and reads the message accordingly to that.
        /// </summary>
        /// <param name="gameClient">A parameter that is used to change variables in the client</param>
        public void ReadServerMessages(Client gameClient)
        {
            // Create a message
            NetIncomingMessage incMsg;
            // Read the incoming message, if there is something, read it.
            while ((incMsg = client.ReadMessage()) != null)
            {
                // Switch messageType
                switch (incMsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            // In case of data, read the packettype and determine what the message contains
                            PacketType packetType = (PacketType)incMsg.ReadByte();
                            // Read what player the message is about
                            byte playerID = (byte)incMsg.ReadByte();


                            // Create a bool, if packettype is playeralive, read the bool message and write to console what the message contained. (Personally, this is a very bad code writing)
                            // Do this with all other if methods...
                            bool boolMessage = false;
                            if(packetType == PacketType.PlayerAlive)
                            {
                                boolMessage = incMsg.ReadBoolean();
                                if (playerID == 0) Console.WriteLine($"Server sent {packetType} containing: {boolMessage}");
                                else Console.WriteLine($"Server sent {packetType} regarding player{playerID} containing: {boolMessage}");
                            }

                            byte byteMessage = 69;
                            if (packetType == PacketType.PlayerConnected || packetType == PacketType.PlayerDisconnected)
                            {
                                byteMessage = incMsg.ReadByte();
                                if (playerID == 0) Console.WriteLine($"Server sent {packetType} containing: {byteMessage}");
                                else Console.WriteLine($"Server sent {packetType} regarding player{playerID} containing: {byteMessage}");
                            }

                            string stringMessage = "N/A";
                            if (packetType == PacketType.GeneralData || packetType == PacketType.StartGame || packetType == PacketType.EndGame)
                            {
                                stringMessage = incMsg.ReadString();
                                if (playerID == 0) Console.WriteLine($"Server sent {packetType} containing: {stringMessage}");
                                else Console.WriteLine($"Server sent {packetType} regarding player{playerID} containing: {stringMessage}");
                            }

                            Point pointMessage = Point.Zero;
                            if (packetType == PacketType.GridData || packetType == PacketType.HeadPos || packetType == PacketType.AddBlob || packetType == PacketType.SubBlobAddbody)
                            {
                                if (packetType == PacketType.HeadPos && gameClient.gameActive && playerID == gameClient.players.Count)
                                {
                                    // If the client recieve a headposition from the server, and that the game is active, and that it is the last playerhead sent, 
                                    // make it next turn in the client side of the game. This is here because it's uneccerary to have a dedicated packettype for "nextTurn".
                                    gameClient.turnManager.NextTurn();
                                }
                                pointMessage = new Point(incMsg.ReadInt32(), incMsg.ReadInt32());
                                if (playerID == 0) Console.WriteLine($"Server sent {packetType} containing: {pointMessage}");
                                else Console.WriteLine($"Server sent {packetType} regarding player{playerID} containing: {pointMessage}");
                            }
                            
                            // A switch for what code to run with corresponding packettype
                            // All methods are in Client.cs
                            switch (packetType)
                            {
                                case PacketType.GeneralData:
                                    gameClient.ReadGeneralData(stringMessage);
                                    break;
                                case PacketType.GridData:
                                    gameClient.ReadGridData(pointMessage);
                                    break;
                                case PacketType.PlayerConnected:
                                    gameClient.ReadPlayerConnected(byteMessage);
                                    break;
                                case PacketType.PlayerDisconnected:
                                    gameClient.ReadPlayerDisconnected(playerID);
                                    break;
                                case PacketType.StartGame:
                                    if (stringMessage == "Ready") gameClient.ReadStartGame(true);
                                    else if (stringMessage == "Start") gameClient.ReadStartGame(false);
                                    else Console.WriteLine($"Unhandled StartGame Type: {stringMessage}");
                                    
                                    break;
                                case PacketType.HeadPos:
                                    gameClient.ReadHeadPos(playerID, pointMessage);
                                    break;
                                case PacketType.AddBlob:
                                    gameClient.ReadAddBlob(pointMessage);
                                    break;
                                case PacketType.SubBlobAddbody:
                                    gameClient.ReadSubBlobAddBody(playerID, pointMessage);
                                    break;
                                case PacketType.PlayerAlive:
                                    gameClient.ReadPlayerAlive(playerID, boolMessage);
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
                    // A switch for what code to run with corresponding packettype
                    // All methods are in Client.cs
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
                // Recycle the message so that the garbage collector runs more effecient.
                client.Recycle(incMsg);
            }
        }

        // Read ReadServerMessages(gameClient), it's excactly the same code with the exception that it's the server that handles the messages and reading the incoming message.
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


                            byte byteMessage = 69;
                            if (packetType == PacketType.PlayerConnected || packetType == PacketType.PlayerDisconnected || packetType == PacketType.Direction)
                            {
                                byteMessage = incMsg.ReadByte();
                                Console.WriteLine($"Player{playerID} sent {packetType} containing: {byteMessage}");
                            }

                            string stringMessage = "N/A";
                            if (packetType == PacketType.GeneralData || packetType == PacketType.StartGame || packetType == PacketType.EndGame)
                            {
                                stringMessage = incMsg.ReadString();
                                Console.WriteLine($"Player{playerID} sent {packetType} containing: {stringMessage}");
                            }


                            switch (packetType)
                            {
                                case PacketType.GeneralData:
                                    
                                    break;
                                case PacketType.Direction:
                                    gameServer.ReadDirection(playerID, byteMessage);
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

        //// Convert an Object to a byte array
        //private static byte[] ObjectToByteArray(Object obj)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    using (var ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, obj);
        //        return ms.ToArray();
        //    }
        //}

        //// Convert a byte array to an Object
        //private static Object ByteArrayToObject(byte[] arrBytes)
        //{
        //    using (var memStream = new MemoryStream())
        //    {
        //        var binForm = new BinaryFormatter();
        //        memStream.Write(arrBytes, 0, arrBytes.Length);
        //        memStream.Seek(0, SeekOrigin.Begin);
        //        var obj = binForm.Deserialize(memStream);
        //        return obj;
        //    }
        //}


    }
}
