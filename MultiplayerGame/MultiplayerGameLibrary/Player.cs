using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace MultiplayerGameLibrary
{
    public class Player
    {
        public NetConnection netConnection; //Server
        public byte playerID;
        public bool disconnected = false;

        public bool ready = false;

        public bool alive;
        public int score;
        public List<Body> bodies = new List<Body>();
        public Point headPos = new Point(3, 3);
        public Point prevHeadPos;
        public enum Direction : byte
        {
            Up,
            Left,
            Down,
            Right
        }
        public Direction direction;
        public Direction prevDirection; //Server

        public Point grid;
        public int eatenBlob = 0; // Client

        /// <summary>
        /// A contruct that add another player/client to the game/server
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ID"></param>
        public Player(NetConnection connection, byte ID)
        {
            netConnection = connection;
            playerID = ID;
            Console.WriteLine($"Player{playerID} has connected as {netConnection} with local address {netConnection.Peer.Configuration.LocalAddress}");
        }

        public Player(byte ID) { playerID = ID; }

        public void ChangeDirection(Direction newDirection)
        {
            if ((byte)newDirection == ((byte)(prevDirection+2)%4) || (byte)newDirection == (byte)prevDirection)
            {
                Console.WriteLine($"Player{playerID} sent a controdictional direction from {prevDirection} to {newDirection} : Request ignored");
                if (prevDirection == direction)
                {
                    
                }
                return;
            }
            else
            {
                direction = newDirection;
            }
            Console.WriteLine($"Changed Player{playerID} direction's to {direction}");
        }

        public void MoveHead() // Server
        {
            prevHeadPos = headPos;
            prevDirection = direction;
            switch ((byte)direction)
            {
                case 0:
                    headPos.Y--;
                    break;
                case 1:
                    headPos.X--;
                    break;
                case 2:
                    headPos.Y++;
                    break;
                case 3:
                    headPos.X++;
                    break;
            }

            if (CollisionWall()) Console.WriteLine($"Player{playerID} has moved {direction} outside the grid to {headPos}");
            else Console.WriteLine($"Player{playerID} has moved {direction} to {headPos}");
        }

        public void NewPosition(Point position) // Client
        {
            prevHeadPos = headPos;
            headPos = position;
        }

        private bool CollisionWall() // Is included in Move()
        {
            if (headPos.X <= 0)
            {
                headPos.X = grid.X;
                return true;
            }
            else if (headPos.X > grid.X)
            {
                headPos.X = 1;
                return true;
            }
            else if (headPos.Y <= 0)
            {
                headPos.Y = grid.Y;
                return true;
            }
            else if (headPos.Y > grid.Y)
            {
                headPos.Y = 1;
                return true;
            }
            else return false;
        }

        private bool CollisionBlob(Server gameServer, List<Blob> blobs) // Is included in MoveBody()
        {
            foreach (Blob blob in blobs)
            {
                if (blob.position == headPos)
                {
                    blobs.Remove(blob);
                    Console.WriteLine($"Player{playerID} has eaten a blob at {blob.position}");
                    gameServer.SendSubBlobAddBody(blob.position, playerID);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Moves the body forward, deletes last and adds a body at prevHeadPos
        /// </summary>
        /// <param name="If player has eaten blob, it adds a body and a point"></param>
        public void MoveBody(Server gameServer, List<Blob> blobs)
        {
            if (CollisionBlob(gameServer, blobs))
            {
                bodies.Add(new Body(prevHeadPos, ++score));
                Console.WriteLine($"Player{playerID} recived a new body at {prevHeadPos} and has current score: {score}");
                return;
            }
            foreach (var body in bodies)
            {
                body.SubLife();
            }
            int totalBodies = bodies.Count;
            for (int i = 0; i < totalBodies; i++)
            {
                if (bodies[i].life <= 0)
                {
                    bodies.Remove(bodies[i]);
                    bodies.Add(new Body(prevHeadPos, score));
                    --totalBodies;
                }
            }
        }

        /// <summary>
        /// Moves the body forward, deletes last and adds a body at prevHeadPos
        /// </summary>
        /// <param name="If player has eaten blob, it adds a body and a point"></param>
        public void MoveBody()
        {
            if (eatenBlob > 0)
            {
                eatenBlob--;
                bodies.Add(new Body(prevHeadPos, ++score));
                Console.WriteLine($"Player{playerID} recived a new body at {prevHeadPos} and has current score: {score}");
                return;
            }
            foreach (var body in bodies)
            {
                body.SubLife();
            }
            int totalBodies = bodies.Count;
            for (int i = 0; i < totalBodies; i++)
            {
                if (bodies[i].life <= 0)
                {
                    bodies.Remove(bodies[i]);
                    bodies.Add(new Body(prevHeadPos, score));
                    --totalBodies;
                }
            }
        }

        public bool CollisionPlayer(List<Player> players)
        {
            foreach (Player player in players)
            {
                if (headPos == player.headPos && playerID != player.playerID)
                {
                    Console.WriteLine($"Player{playerID} collided with Player{player.playerID} head at {headPos}");
                    return true;
                }
                foreach (Body body in player.bodies)
                {
                    if (headPos == body.position)
                    {
                        Console.WriteLine($"Player{playerID} collided with Player{player.playerID} body at {headPos}");
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Reset the player and its position according to the ID and board size
        /// </summary>
        /// <param name="gridSize"></param>
        public void Reset(Point gridSize)
        {
            grid = gridSize;
            bodies.Clear();
            alive = true;
            score = 0;
            ready = false;
            Console.WriteLine($"RESET_Player{playerID}'s settings: Grid = {grid}");

            switch (playerID)
            {
                case 1:
                    headPos = new Point(2, 2);
                    break;
                case 2:
                    headPos = new Point(grid.X - 1, 2);
                    break;
                case 3:
                    headPos = new Point(2, grid.Y - 1);
                    break;
                case 4:
                    headPos = new Point(grid.X - 1, grid.Y - 1);
                    break;
            }
            prevHeadPos = headPos;
            Console.WriteLine($"Player{playerID} spawned at " + headPos.ToString());
        }

        public void Reset()
        {
            bodies.Clear();
            alive = true;
            score = 0;
            prevHeadPos = headPos;
        }
    }
}
