using Lidgren.Network;
using System;

namespace MultiplayerGame
{
    class Client
    {

        public enum DataType
        {
            Identification,
            Player
        }

        private NetClient client;
        /// <summary>
        /// Sets up and starts the client (configured localy)
        /// </summary>
        public void StartClient()
        {
            // Creates a configuration for how NetClient shall handle data.
            var config = new NetPeerConfiguration("tittut");
            // Turns off the automatic erasing of data when the network queue is overflowing
            config.AutoFlushSendQueue = false;
            // Set configuration
            client = new NetClient(config);
            // Start client
            client.Start();

            // Declare what ip to connect to and what port shall be used
            string ip = "localhost";
            int port = 14242;
            // Try to connect
            client.Connect(ip, port);
        }
        /// <summary>
        /// Sends a categorized message
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="Message"></param>
        public void SendMessage(DataType type, string text)
        {
            // Creates a message
            NetOutgoingMessage message = client.CreateMessage((byte)type + text);

            // Send the message reliably
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            // Flush the memory of what shall be sent
            client.FlushSendQueue();
        }

        /// <summary>
        /// Client dissconects from the network
        /// </summary>
        public void Disconnect()
        {
            client.Disconnect("*Yeets out*");
        }
    }
}
