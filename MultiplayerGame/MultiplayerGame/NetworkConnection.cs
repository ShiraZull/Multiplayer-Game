using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using MultiplayerGameLibrary;

namespace MultiplayerGame
{
    class NetworkConnection
    {
        // A NetPeer that is used for a client-server communiction
        private NetClient _client;

        // A memorycell for storing the clients status to the server
        public NetConnectionStatus Status => _client.ConnectionStatus;

        // When function starts, then...
        public bool Start()
        {
            // Store your username as the string sugests
            NetworkLoginInformation loginInformation = new NetworkLoginInformation() { Name = "Julius" };
            // Makes the client configured to search for "networkGame" (like a key)
            _client = new NetClient(new NetPeerConfiguration("networkGame"));
            // Sets the information into the socket and creates a thread for messages
            _client.Start();
            // New variable for sending a message
            var outmsg = _client.CreateMessage();
            // Inserts a type (category) into the message
            outmsg.Write((byte)PacketType.Login);
            // Puts in the information into the type (category)
            outmsg.WriteAllProperties(loginInformation);
            // Connect with the named host with port and message
            _client.Connect("localhost", 14241, outmsg);
            // Looks if the client is getting information from server
            return EsablishInfo();
        }

        private bool EsablishInfo()
        {
            // Checks time and stores it
            var time = DateTime.Now;
            // New incoming messege
            NetIncomingMessage inc;
            // Loop this function while it's true
            while (true)
            {
                // If 5 seconds have passed, stop loop and turn established info to false
                if (DateTime.Now.Subtract(time).Seconds > 5)
                {
                    return false;
                }

                // If the messege is empty, then continue to the next segment
                if ((inc = _client.ReadMessage()) == null) continue;

                // Read messege type
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        // Read the messege
                        var data = inc.ReadByte();
                        // If data is login information
                        if (data == (byte)PacketType.Login)
                        { // check the next messege is true then return true (and EsablishInfo() = true)
                            var accepted = inc.ReadBoolean();
                            if (accepted)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                }
            }



        }
    }
}
