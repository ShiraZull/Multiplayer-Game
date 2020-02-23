﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;
using Tools_XNA_dotNET_Framework;

namespace MultiplayerGameLibrary
{
    public class Client
    {
        private NetClient client;
        private MessageManager MM;
        
        public byte playerID;
        public bool gameActive = false;
        private Point grid;
        private List<Player> players;
        private List<Blob> blobs = new List<Blob>();
        private TurnManager turnManager;
        private int startCountdown = 3000;
        private bool allPlayersReady = false;


        public void Initialize()
        {
            StartClient();
            MM = new MessageManager(client);
            players = new List<Player>(4);
            turnManager = new TurnManager(startCountdown);
        }

        public void Update(GameTime gameTime)
        {
            MM.ReadServerMessages(this);
        }

        // GameBoard
        public int gridSize = 6;
        public int squareSize;
        public int lineSize;
        public int playArea;
        public Vector2 gridPosition;



        public void Draw(SpriteBatch spriteBatch)
        {
            RestartGrid();

            foreach (Player player in players)
            {
                DrawPlayerPart(spriteBatch, player.headPos, player.playerID, 1f);
                foreach (Body body in player.bodies)
                {
                    DrawPlayerPart(spriteBatch, body.position, player.playerID, 0.7f);
                }
            }
            Primitives2D.DrawGrid(spriteBatch, gridSize, gridSize, squareSize, gridPosition, Color.White, lineSize);
        }

        public void DrawPlayerPart(SpriteBatch spriteBatch, Point position, byte playerID, float alpha)
        {
            Primitives2D.DrawFilledRectangle(spriteBatch, new Rectangle((int)gridPosition.X + squareSize * (position.X - 1), (int)gridPosition.Y + squareSize * (position.Y - 1), squareSize, squareSize), playerColor(playerID, alpha));
        }

        public Color playerColor(byte playerID, float alpha)
        {
            Color color = new Color();
            if (playerID == 1)
            {
                color = new Color(Color.Aqua, alpha);
                return color;
            }
            if (playerID == 2)
            {
                color = new Color(Color.Magenta, alpha);
                return color;
            }
            if (playerID == 3)
            {
                color = new Color(Color.Lime, alpha);
                return color;
            }
            if (playerID == 4)
            {
                color = new Color(Color.Yellow, alpha);
                return color;
            }

            color = Color.Gray;
            return color;
        }


        public void RestartGrid()
        {
            squareSize = (600 - (gridSize + 1) * lineSize) / gridSize;
            if (gridSize > 20) lineSize = 1;
            else lineSize = squareSize / (20);
            playArea = gridSize * squareSize;
            gridPosition = new Vector2((600 - playArea + lineSize) / (2), 200 + (600 - playArea - lineSize) / 2);
        }

        
        
        

        
        

        public bool IsConnectedToServer()
        {
            if (client.ConnectionsCount > 0)
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



        #region GameLogic + Network Methods
        public void SendGeneralData(object data)
        {
            MM.SendMessageToServer(playerID, MessageManager.PacketType.GeneralData, data);
        }
        public void ReadGeneralData(object message)
        {
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
                            players.Add(new Player((byte)(a + 1)));
                        }
                        Console.WriteLine($">>> There is currently {players.Count} players in 'players'");
                        break;
                    }
                    i++;
                }
            }
        }
        public void ReadGridData(Point newGridSize)
        {
            grid = newGridSize;
            Console.WriteLine($"Changed gridsize to {grid}");
        }
        public void ReadPlayerConnected(byte playerID, NetConnection netConnection)
        {
            players.Add(new Player(netConnection, playerID));
            Console.WriteLine($"New player connected with {netConnection} and has now the ID as {players.Count}");
        } 
        public void ReadPlayerDisconnected(byte playerID)
        {
            foreach (Player player in players)
            {
                if (player.playerID == playerID)
                {
                    Console.WriteLine($"Player{player.playerID} ({player.netConnection}) has disconnected!");
                    players.Remove(player);
                    Console.WriteLine($"Removed Player{player.playerID} from the player list");
                    MM.SendMessageToAllClients(MessageManager.PacketType.PlayerDisconnected, player.playerID);
                    return;
                }
            }
            Console.WriteLine($"Unknown has disconnected!");

        } 
        public void ReadStartGame(bool ready)
        {
            if (ready) allPlayersReady = true;
            else gameActive = true;

        } //TODO: Check if this works
        public void SendDirection(Player.Direction newDirection)
        {
            MM.SendMessageToServer(playerID, MessageManager.PacketType.Direction, newDirection);
        } 
        public void ReadHeadPos(byte playerID, Point newHeadPos)
        {
            players[playerID - 1].NewPosition(newHeadPos);
        }
        public void ReadAddBlob(Point position)
        {
            blobs.Add(new Blob(position));
            Console.WriteLine($"Added a blob at {position}");
        } 
        public void ReadSubBlobAddBody(Point blobPosition, byte playerID)
        {
            foreach (Blob blob in blobs)
            {
                if (blob.position == blobPosition)
                {
                    blobs.Remove(blob);
                }
            }
            players[playerID - 1].eatenBlob = true;
            
        }
        public void ReadPlayerAlive(byte playerID, bool alive)
        {
            players[playerID - 1].alive = alive;
        }
        public void ReadEndGame()
        {
            gameActive = false;
            Console.WriteLine($"Ended game, changed gameActive to false");
        } //TODO: Check if this works
        #endregion GameLogic + Network Methods




    }
}
