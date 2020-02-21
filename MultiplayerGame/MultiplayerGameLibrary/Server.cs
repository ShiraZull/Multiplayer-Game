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
        private Point grid = new Point(6);
        private List<Player> players;
        private List<Blob> blobs = new List<Blob>();


        public void Initialize()
        {
            MM = new MessageManager(server);
            players = new List<Player>(4);
            turnManager = new TurnManager(1000, 2000);
        }


        public void GameSetup()
        {
            blobs.Add(new Blob(new Point(1, 1)));
            Console.WriteLine($"Added blob at {blobs[0].position}");

        }

        public void GameRun()
        {
            turnManager.UpdateTimeDiff();
            if (gameActive)
            {
                turnManager.UpdateTurn();
                if (turnManager.nextTurn)
                {
                    players[0].alive = true;
                    players[0].board = grid;
                    foreach (Player player in players)
                    {
                        if (player.alive)
                        {
                            player.MoveHead();
                            if (player.CollisionBlob(blobs))
                            {
                                player.MoveBody(true);
                            }
                            else player.MoveBody(false);
                            foreach (var body in player.bodies)
                            {
                                Console.WriteLine($"Body position: {body.position}");
                            }
                            player.CollisionPlayer(players);
                        }

                    }
                    if (blobs.Count == 0)
                        blobs.Add(new Blob(blobs, players, grid));
                    Console.WriteLine($"New blob position: {blobs[blobs.Count - 1].position}");

                    SendEndTurnData();
                }
            }
        }

        public void RestartGame()
        {
            foreach (Player player in players)
            {
                player.Reset(grid);
            }
            blobs.Clear();
            if (grid.X % 2 == 1)
            {
                blobs.Add(new Blob(new Point((grid.X + 1) / 2, (grid.Y + 1) / 2)));
            }
            if (grid.X % 2 == 0)
            {
                blobs.Add(new Blob(new Point(grid.X / 2, grid.Y / 2)));
                blobs.Add(new Blob(new Point(grid.X / 2 + 1, grid.Y / 2)));
                blobs.Add(new Blob(new Point(grid.X / 2, grid.Y / 2 + 1)));
                blobs.Add(new Blob(new Point(grid.X / 2 + 1, grid.Y / 2 + 1)));
            }

        }
        

        public void SendEndTurnData()
        {
            foreach (Player player in players)
            {
                if (player.alive)
                {
                    MM.SendMessageToAllClients(MessageManager.PacketType.HeadPos, player.playerID, player.headPos);
                    SendMessageToAllPlayers(DataType.HeadPos, "Y:" + player.headPos.Y);
                    if (player.CollisionBlob(blobs))
                    {
                        SendMessageToAllPlayers(DataType.SubBlob, $"BlobX:{player.collidedBlob.position.X}");
                        SendMessageToAllPlayers(DataType.SubBlob, $"BlobY:{player.collidedBlob.position.Y}");

                        SendMessageToAllPlayers(DataType.TurnAction, $"Player{player.playerID}: BODY");
                    }
                }
                else SendMessageToAllPlayers(DataType.TurnAction, $"Player{player.playerID}: DEAD");

            }
            foreach (Blob blob in blobs)
            {

            }
        }



        #region Network Methods
        public void GeneralData(Player player, object data)
        {
            MM.SendMessageToClient(player, MessageManager.PacketType.GeneralData, data);
        }
        public void GeneralData(object data)
        {
            MM.SendMessageToAllClients(MessageManager.PacketType.GeneralData, data);
        }
        public void GridData(Point newGridSize)
        {
            grid = newGridSize;
            Console.WriteLine($"Changed gridsize to {grid}");
            MM.SendMessageToAllClients(MessageManager.PacketType.GridData, grid);
        }
        public void PlayerConnected(NetConnection netConnection)
        {
            players.Add(new Player(netConnection, (byte)(players.Count + 1)));
            Console.WriteLine($"Player connected with {netConnection} and has now the ID as {players.Count}");
            MM.SendMessageToClient(players[players.Count-1], MessageManager.PacketType.GeneralData, "ID:" + players.Count);
            GeneralData(players[players.Count - 1], "ID:" + players.Count);
            MM.SendMessageToAllClients(MessageManager.PacketType.PlayerConnected, (byte)players.Count, netConnection);
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
                        PlayerAlive(player.playerID, false);
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
                        PlayerAlive(player.playerID, false);
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
            Console.WriteLine($"Unknown ({senderConnection}) disconnected!");

        }
        public void StartGame() // TODO: Seperate "Ready" and "Start" within this method, either with a string or bool
        {

        }
        public void Direction()
        {
           
        }
        public void HeadPos(bool automatic)
        {
            foreach (Player player in players)
            {
                if (automatic)
                {
                    if (player.alive)
                    {
                        player.MoveHead();
                        if (player.CollisionPlayer(players))
                        {
                            player.headPos = player.prevHeadPos;
                            player.alive = false;
                            Console.WriteLine($"Player{player.playerID} died");
                            PlayerAlive(player.playerID, false);
                        }
                        else
                        {
                            player.MoveBody(this, blobs);
                            // Debug
                            foreach (var body in player.bodies)
                            {
                                Console.WriteLine($"Body: {body.ToString()} position: {body.position}");
                            }
                        }
                    }
                    
                }
                MM.SendMessageToAllClients(MessageManager.PacketType.HeadPos, player.playerID, player.headPos);
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
        public void SubBlobAddBody(Point blobPosition, byte playerID) // Used in Player.cs
        {
            MM.SendMessageToAllClients(MessageManager.PacketType.SubBlobAddbody, playerID, blobPosition);
        }
        public void PlayerAlive(byte playerID, bool alive) // Used in Player.cs
        {
            MM.SendMessageToAllClients(MessageManager.PacketType.PlayerAlive, playerID, alive);
        }
        public void EndGame() // TODO: Make all disconnected players removed after a game
        {

        }


        #endregion Network Methods
        
    }
}
