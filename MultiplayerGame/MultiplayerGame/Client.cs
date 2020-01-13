using Lidgren.Network;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MultiplayerGame
{
    class Client
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

        public byte playerID;



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
                            var message = ByteArrayToObject(incMsg.ReadBytes(incMsg.LengthBytes));

                            switch (dataType)
                            {
                                case (byte)DataType.GameInfo:

                                    // WTF is this? ---------------------------------- TODO
                                    if(message.ToString().StartsWith("ID:"))
                                    {
                                        message.ToString().Remove(3);
                                        Console.WriteLine(message.GetType());
                                    }

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
            if(client.ConnectionsCount > 0)
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
