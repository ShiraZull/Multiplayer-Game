using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace MultiplayerGameServer
{
    class Server
    {
        private NetServer server;
        // All the clients in the server
        private List<NetPeer> clients;

        /// <summary>
        /// Sets up and starts the server (configured localy)
        /// </summary>
        public void StartServer()
        {
            // Creates a configuration for how NetServer shall handle data.
            var config = new NetPeerConfiguration("tittut") { Port = 14242 };
            // Set configuration
            server = new NetServer(config);
            // Start server
            server.Start();

            // Check if server is running and let the user know
            if (server.Status == NetPeerStatus.Running)
            {
                Console.WriteLine("Server is running on port " + config.Port);
            }
            else
            {
                Console.WriteLine("Server not started...");
            }

            // Create list
            clients = new List<NetPeer>();
        }

        /// <summary>
        /// Reads the clients message
        /// </summary>
        public void ReadMessages()
        {
            // A message reciever
            NetIncomingMessage message;
            // A variable that is used to create a loop to connect with the client
            var stop = false;

            // Loop until someone exits and shutdown the server
            while (!stop)
            {
                // Loop until all messages has been read
                while ((message = server.ReadMessage()) != null)
                {
                    // Switch case depending on what type of message is recived
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            {
                                // Write a confirmation of message
                                Console.WriteLine("I got smth!");
                                // Read the message
                                var data = message.ReadString();
                                // Write the message
                                Console.WriteLine(data);

                                // If message is "exit", then exit application
                                if (data == "exit")
                                {
                                    stop = true;
                                }

                                break;
                            }
                        case NetIncomingMessageType.DebugMessage:
                            // Read the recieved debug message
                            Console.WriteLine(message.ReadString());
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            // Check the status of the client
                            Console.WriteLine(message.SenderConnection.Status);
                            if (message.SenderConnection.Status == NetConnectionStatus.Connected)
                            {
                                // Add client to list
                                clients.Add(message.SenderConnection.Peer);
                                Console.WriteLine("{0} has connected.", message.SenderConnection.Peer.Configuration.LocalAddress);
                            }
                            if (message.SenderConnection.Status == NetConnectionStatus.Disconnected)
                            {
                                // Remove client from list
                                clients.Remove(message.SenderConnection.Peer);
                                Console.WriteLine("{0} has disconnected.", message.SenderConnection.Peer.Configuration.LocalAddress);
                            }
                            break;
                        default:
                            // If the messege type is not known, write it out
                            Console.WriteLine("Unhandled message type: {message.MessageType}");
                            break;
                    }
                    // A function that make it easier for the garbage collector
                    server.Recycle(message);
                }
            }

            // Write that a exit message has been recived and shall be shut down
            Console.WriteLine("Shutdown package \"exit\" received. Press any key to finish shutdown");
            // Read the next key input for confirmation
            Console.ReadKey();
        }
    }
}
