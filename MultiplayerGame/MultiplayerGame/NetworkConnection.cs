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
        private NetClient _client;

        public NetConnectionStatus Status => _client.ConnectionStatus;

        public bool Start()
        {
            NetworkLoginInformation loginInformation = new NetworkLoginInformation() { Name = "RandomName" };
            _client = new NetClient(new NetPeerConfiguration("networkGame"));
            _client.Start();
            var outmsg = _client.CreateMessage();
            outmsg.Write((byte)PacketType.Login);
            outmsg.WriteAllProperties(loginInformation);
            _client.Connect("localhost", 14241, outmsg);
            return EsablishInfo();
        }

        private bool EsablishInfo()
        {
            var time = DateTime.Now;
            NetIncomingMessage inc;
            while (true)
            {
                if (DateTime.Now.Subtract(time).Seconds > 5)
                {
                    return false;
                }

                if ((inc = _client.ReadMessage()) == null) continue;

                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        var data = inc.ReadByte();
                        if (data == (byte)PacketType.Login)
                        {
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
