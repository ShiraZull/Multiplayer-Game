using System;
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
using Microsoft.Xna.Framework.Content;

namespace MultiplayerGameLibrary
{
    public class Client
    {
        private NetClient client;
        private MessageManager MM;
        
        public byte playerID;
        public bool gameActive = false;
        private Point grid = new Point(1);
        public List<Player> players;
        private List<Blob> blobs = new List<Blob>();
        public TurnManager turnManager;
        private int startCountdown = 3000;
        private bool allPlayersReady = false;

        public SpriteFont scoreFont, bigFont, smallFont;

        public void Initialize()
        {
            StartClient();
            MM = new MessageManager(client);
            players = new List<Player>(4);
            turnManager = new TurnManager(startCountdown);
        }

        public void Load(ContentManager Content)
        {
            scoreFont = Content.Load<SpriteFont>(@"ScoreFont");
            bigFont = Content.Load<SpriteFont>(@"BigFont");
            smallFont = Content.Load<SpriteFont>(@"SmallFont");
        }


        public void Update()
        {
            GameRun();
        }


        public void GameRun()
        {
            MM.ReadServerMessages(this);

            turnManager.UpdateTimeDiff(0);
            if (allPlayersReady) turnManager.UpdateCountdown();
            
            if (gameActive)
            {
                if (turnManager.nextTurn)
                {
                    turnManager.nextTurn = false;
                    foreach (Player player in players)
                    {
                        if (player.alive)
                        {
                            player.MoveBody();

                            foreach (var body in player.bodies) // Debug
                            {
                                Console.WriteLine($"Body: {body.life} position: {body.position}");
                            }
                        }
                    }
                    

                }
            }
        }





        // GameBoard
        public int gridSize = 12;
        public int squareSize;
        public int lineSize;
        public int playArea;
        public Vector2 gridPosition;



        public void Draw(SpriteBatch spriteBatch)
        {
            RestartGrid();

            foreach (Blob blob in blobs)
            {
                DrawBlob(spriteBatch, blob.position);
            }

            foreach (Player player in players)
            {
                DrawPlayerPart(spriteBatch, player.headPos, player.playerID, 1f);
                foreach (Body body in player.bodies)
                {
                    DrawPlayerPart(spriteBatch, body.position, player.playerID, 0.5f);
                }
            }
            Primitives2D.DrawGrid(spriteBatch, gridSize, gridSize, squareSize, gridPosition, Color.White, lineSize);
            for (int i = 0; i < players.Count; i++)
            {
                spriteBatch.DrawString(scoreFont, players[i].bestScore.ToString(), new Vector2(40 + 130 * i, 0), Color.LightGray);
                spriteBatch.DrawString(scoreFont, players[i].score.ToString(), new Vector2(40 + 130 * i, 80), playerColor(players[i].playerID, 1f));

            }
            if (playerID > 0)
            {
                int pil = playerID - 1;
                spriteBatch.DrawString(smallFont, "BEST SCORES", new Vector2(40, 4), Color.LightGray);
                spriteBatch.DrawString(smallFont, "YOU", new Vector2(40 + 130 * pil, 160), playerColor(playerID, 1f));
            }

            if (!gameActive)
            {
                if (playerID == 0) spriteBatch.DrawString(bigFont, "Waiting for server...", new Vector2(60, 300), Color.White);
                else if(!allPlayersReady)
                {
                    Primitives2D.DrawFilledRectangle(spriteBatch, new Rectangle(0,200,600,600), Color.LightGray);
                    spriteBatch.DrawString(bigFont, "Control with [WASD] \n\nPress one of the \nkeys when ready\nAll players need to \nbe ready to start", new Vector2(60, 320), Color.Black);
                }
                
            }



        }

        public void DrawPlayerPart(SpriteBatch spriteBatch, Point position, byte playerID, float alpha)
        {
            Primitives2D.DrawFilledRectangle(spriteBatch, new Rectangle((int)gridPosition.X + squareSize * (position.X - 1), (int)gridPosition.Y + squareSize * (position.Y - 1), squareSize, squareSize), playerColor(playerID, alpha));
        }

        public void DrawBlob(SpriteBatch spriteBatch, Point position)
        {
            Primitives2D.DrawCircle(spriteBatch, new Vector2((int)gridPosition.X + squareSize * (position.X - 1) + squareSize/2 - lineSize/2, 
                (int)gridPosition.Y + squareSize * (position.Y - 1) + squareSize / 2 + lineSize/2), squareSize / 3, 24, Color.Wheat, squareSize / 3);
        }

        public Color playerColor(byte playerID, float alpha)
        {
            Color color = new Color();
            foreach (Player player in players)
            {
                if (!player.alive && player.playerID == playerID)
                {
                    color = new Color(Color.LightGray, alpha);
                    return color;
                }
            }
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
            gridSize = grid.X;
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

        /// <summary>
        /// Initialize a client on a separate thread
        /// </summary>
        public void StartClient()
        {
            // A configuration for the server before initializing, with an identifier that makes sure no other apps has acces to this game project.
            var config = new NetPeerConfiguration("MultiplayerGame2020");
            // A setting that makes flushing the buffer a manual task.
            config.AutoFlushSendQueue = false;
            // Create a client with the specified configuration
            client = new NetClient(config);
            // Start on a separete thread
            client.Start();

            // ip-address to connect to server, this is what you change if you want to connect to other computer
            string ip = "localhost";
            int port = 14242;
            // Connect to the specefied adress
            client.Connect(ip, port);
        }



        #region GameLogic + Network Methods
        public void SendGeneralData(string data)
        {
            MM.SendMessageToServer(playerID, MessageManager.PacketType.GeneralData, data);
        }
        public void ReadGeneralData(string message)
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
            RestartGrid();
            Console.WriteLine($"Changed gridsize to {grid}");
        }
        public void ReadPlayerConnected(byte playerID)
        {
            if (playerID == this.playerID)
            {
                return;
            }
            players.Add(new Player(playerID));
            Console.WriteLine($"New player connected with the ID as {playerID}");
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
            if (ready)
            {
                foreach (Player player in players)
                {
                    player.Reset();
                }
                allPlayersReady = true;
                turnManager.turn = 0;
                turnManager.time = 3000;
                Console.WriteLine("All players are reset and ready! Staring countdown...");
            }
            else
            {
                gameActive = true;
            }

        } //TODO: Check if this works
        public void SendDirection(byte newDirection)
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
        public void ReadSubBlobAddBody(byte playerID, Point blobPosition)
        {
            int blobbies = blobs.Count;
            for (int i = 0; i < blobbies; i++)
            {
                if (blobs[i].position == blobPosition)
                {
                    blobs.Remove(blobs[i]);
                    i--;
                    blobbies--;
                }
            }
            players[playerID - 1].eatenBlob++;
            
        }
        public void ReadPlayerAlive(byte playerID, bool alive)
        {
            if (!alive)
            {
                players[playerID - 1].MoveBody();
            }
            players[playerID - 1].alive = alive;
        }
        public void ReadEndGame()
        {
            gameActive = false;
            foreach (Player player in players)
            {
                if (player.score > player.bestScore)
                {
                    player.bestScore = player.score;
                }
                player.Reset();
            }
            blobs.Clear();
            allPlayersReady = false;
            Console.WriteLine($"Ended game, reset game");
            turnManager.Reset(startCountdown);
        } 
        #endregion GameLogic + Network Methods




    }
}
