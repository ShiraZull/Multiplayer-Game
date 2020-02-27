using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace MultiplayerGameLibrary
{
    public class Server
    {
        private NetServer server;
        private MessageManager MM;

        public bool gameActive = false;
        private TurnManager turnManager;
        private Point grid = new Point(12);
        private List<Player> players;
        private List<Blob> blobs = new List<Blob>();
        private int startCountdown = 3000;
        private int turnDelay = 1000;



        public void Initialize()
        {
            StartServer();
            MM = new MessageManager(server);
            players = new List<Player>(4);
            turnManager = new TurnManager(turnDelay, startCountdown);
        }


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

        public void LobbySetup()
        {

        }

        public void LobbyRun()
        {

        }

        public void GameSetup()
        {
            blobs.Add(new Blob(new Point(1, 1)));
            Console.WriteLine($"Added blob at {blobs[0].position}");

        }



        public void GameRun()
        {
            MM.ReadClientMessages(this);
            if (!gameActive)
            {
                if (AllPlayersReady())
                {
                    RestartGame();
                    gameActive = true;
                }
            }

            turnManager.UpdateTimeDiff(20);
            if (gameActive)
            {
                turnManager.UpdateTurn();
                if (turnManager.nextTurn)
                {
                    if (turnManager.turn == 1) SendStartGame(false);
                    SendPlayerData(true);
                    if (blobs.Count == 0) AddBlob();
                        int alive = 0;
                    foreach (Player player in players)
                    {
                        if (player.alive) ++alive;
                    }
                        if (alive == 0)
                        {
                            EndGame();
                            RestartGame();
                        }
                }
            }
        }

        public void RestartGame()
        {
            gameActive = false;
            turnManager.Reset(startCountdown);
            foreach (Player player in players)
            {
                player.Reset(grid);
            }
            SendPlayerData(false);
            ChangeGridData(grid);
            blobs.Clear();
            if (grid.X % 2 == 1)
            {
                AddBlob(new Point((grid.X + 1) / 2, (grid.Y + 1) / 2));
            }
            if (grid.X % 2 == 0)
            {
                AddBlob(new Point(grid.X / 2, grid.Y / 2));
                AddBlob(new Point(grid.X / 2 + 1, grid.Y / 2));
                AddBlob(new Point(grid.X / 2, grid.Y / 2 + 1));
                AddBlob(new Point(grid.X / 2 + 1, grid.Y / 2 + 1));
            }

        }

        public bool AllPlayersReady()
        {
            int allPlayersReady = 0;
            foreach (Player player in players) if (player.ready) allPlayersReady++;
            if (players.Count == allPlayersReady && players.Count > 0)
            {
                SendStartGame(true);
                return true;
            }
            else return false;
        }

        #region GameLogic + Network Methods
        public void SendGeneralData(Player player, string data)
        {
            MM.SendMessageToClient(player, MessageManager.PacketType.GeneralData, data);
        }
        public void SendGeneralData(string data)
        {
            MM.SendMessageToAllClients(MessageManager.PacketType.GeneralData, data);
        }
        public void ReadGeneralData() // TODO: Is this needed?
        {
            
        } 
        public void ChangeGridData(Point newGridSize)
        {
            grid = newGridSize;
            Console.WriteLine($"Changed gridsize to {grid}");
            MM.SendMessageToAllClients(MessageManager.PacketType.GridData, grid);
        }
        public void PlayerConnected(NetConnection netConnection)
        {
            players.Add(new Player(netConnection, (byte)(players.Count + 1)));
            Console.WriteLine($"New player connected with {netConnection} and has now the ID as {players.Count}");
            SendGeneralData(players[players.Count - 1], (string)("ID:" + players.Count.ToString()));
            MM.SendMessageToAllClients(MessageManager.PacketType.PlayerConnected, (byte)players.Count);
            if (!gameActive)
            {
                
            }
        }
        public void PlayerDisconnected(NetConnection senderConnection)
        {
            foreach (Player player in players)
            {
                if (player.netConnection == senderConnection)
                {
                    Console.WriteLine($"Player{player.playerID} ({senderConnection}) has disconnected!");
                    if (gameActive)
                    {
                        player.disconnected = true;
                        Console.WriteLine($"Changed Player{player.playerID}'s state to disconnected");
                        player.alive = false;
                        Console.WriteLine($"Changed Player{player.playerID}'s state to dead, will be removed from player list after the match");
                        SendPlayerAlive(player.playerID, false);
                    }
                    else
                    {
                        players.Remove(player);
                        Console.WriteLine($"Removed Player{player.playerID} from the player list");
                        MM.SendMessageToAllClients(MessageManager.PacketType.PlayerDisconnected, player.playerID, "Disconnected");
                    }

                    return;
                }
            }
            Console.WriteLine($"Unknown ({senderConnection}) disconnected!");

        }
        public void PlayerDisconnected()
        {
            foreach (Player player in players)
            {
                if (player.disconnected)
                {
                    Console.WriteLine($"Player{player.playerID} ({player.netConnection}) has disconnected!");
                    if (gameActive)
                    {
                        player.disconnected = true;
                        Console.WriteLine($"Changed Player{player.playerID}'s state to disconnected");
                        player.alive = false;
                        Console.WriteLine($"Changed Player{player.playerID}'s state to dead, will be removed from player list after the match");
                        SendPlayerAlive(player.playerID, false);
                    }
                    else
                    {
                        players.Remove(player);
                        Console.WriteLine($"Removed Player{player.playerID} from the player list");
                        MM.SendMessageToAllClients(MessageManager.PacketType.PlayerDisconnected, player.playerID);
                    }

                    return;
                }
            }
            Console.WriteLine($"Error: Unhandled disconnection!");

        }
        public void SendStartGame(bool ready)
        {
            if (ready) MM.SendMessageToAllClients(MessageManager.PacketType.StartGame, "Ready"); // Declare turn 0 for client
            else MM.SendMessageToAllClients(MessageManager.PacketType.StartGame, "Start"); // Declare turn 1 for client

        }
        public void ReadDirection(byte playerID, byte newDirection)
        {
            if(gameActive)
            {
                players[playerID - 1].ChangeDirection((Player.Direction)newDirection);
            }
            else
            {
                if (players[playerID - 1].ready == false)
                {
                    players[playerID - 1].ready = true;
                    players[playerID - 1].direction = (Player.Direction)newDirection;
                    players[playerID - 1].prevDirection = (Player.Direction)newDirection;
                    Console.WriteLine($"Player{playerID} READY");
                }
                
            }
        }
        public void SendPlayerData(bool automaticallyUpdate)
        {
            if (!automaticallyUpdate)
            {
                foreach (Player player in players)
                {
                    MM.SendMessageToAllClients(MessageManager.PacketType.HeadPos, player.playerID, player.headPos);
                }
                    return;
            }
            foreach (Player player in players)
            {
                if (player.alive)
                {
                    player.MoveHead();
                    MM.SendMessageToAllClients(MessageManager.PacketType.HeadPos, player.playerID, player.headPos);
                    player.MoveBody(this, blobs);

                    //foreach (var body in player.bodies) // Debug
                    //{
                    //    Console.WriteLine($"Body: {body.ToString()} position: {body.position}");
                    //}
                }
            }
            foreach (Player player in players)
            {
                if (player.alive)
                {
                    if (player.CollisionPlayer(players))
                    {
                        player.alive = false;
                        Console.WriteLine($"Player{player.playerID} died");
                        SendPlayerAlive(player.playerID, false);
                    }
                }
            }
            

        }



        public void AddBlob(Point position)
        {
            blobs.Add(new Blob(position));
            Console.WriteLine($"Manually added a blob at {position}");
            MM.SendMessageToAllClients(MessageManager.PacketType.AddBlob, position);
        }
        public void AddBlob()
        {
            blobs.Add(new Blob(blobs, players, grid));
            Console.WriteLine($"Spawned a blob at {blobs[blobs.Count - 1].position}");
            MM.SendMessageToAllClients(MessageManager.PacketType.AddBlob, blobs[blobs.Count-1].position);
        }
        public void SendSubBlobAddBody(Point blobPosition, byte playerID) // Used in Player.cs
        {
            MM.SendMessageToAllClients(MessageManager.PacketType.SubBlobAddbody, playerID, blobPosition);
        }
        public void SendPlayerAlive(byte playerID, bool alive) // Used in Player.cs
        {
            MM.SendMessageToAllClients(MessageManager.PacketType.PlayerAlive, playerID, alive);
        }
        public void EndGame()
        {
            gameActive = false;
            Console.WriteLine($"Ended game, changed gameActive to false");
            MM.SendMessageToAllClients(MessageManager.PacketType.EndGame, "EndGame");
            PlayerDisconnected(); // All players who disconnected during a match
        }
        #endregion GameLogic + Network Methods

    }
}
