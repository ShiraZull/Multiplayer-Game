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

        public Point board;
        public Blob collidedBlob;

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

        public void Move() // Server
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
                headPos.X = board.X;
                return true;
            }
            else if (headPos.X > board.X)
            {
                headPos.X = 1;
                return true;
            }
            else if (headPos.Y <= 0)
            {
                headPos.Y = board.Y;
                return true;
            }
            else if (headPos.Y > board.Y)
            {
                headPos.Y = 1;
                return true;
            }
            else return false;
        }

        public bool CollisionBlob(List<Blob> blobs)
        {
            foreach (Blob blob in blobs)
            {
                if (blob.position == headPos)
                {
                    blobs.Remove(blob);
                    Console.WriteLine($"Player{playerID} has eaten a blob at {blob.position}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Moves the body forward, deletes last and adds a body at prevHeadPos
        /// </summary>
        /// <param name="If player has eaten blob, it only adds a body and a point"></param>
        public void MoveBody(bool eatBlob)
        {
            if (eatBlob)
            {
                bodies.Add(new Body(prevHeadPos, ++score));
                Console.WriteLine($"Player{playerID} recived a new body at {prevHeadPos} and has current score: {score}");
                return;
            }
            foreach (var body in bodies)
            {
                body.SubLife();
                if (body.life <= 0)
                {
                    body.position = prevHeadPos;
                    body.life = score;
                }
            }
        }

        public void CollisionPlayer(List<Player> players)
        {
            foreach (Player player in players)
            {
                if (headPos == player.headPos && playerID != player.playerID)
                {
                    alive = false;
                    Console.WriteLine($"Player{playerID} collided with Player{player.playerID} at {headPos}");
                    return;
                }
                foreach (Body body in player.bodies)
                {
                    if (headPos == body.position)
                    {
                        alive = false;
                        Console.WriteLine($"Player{playerID} collided with Player{player.playerID} body at {headPos}");
                        return;
                    }
                }
            }

        }

        /// <summary>
        /// Reset the player and its position according to the ID and board size
        /// </summary>
        /// <param name="boardSize"></param>
        public void Reset(Point boardSize)
        {
            board = boardSize;
            bodies.Clear();
            alive = true;
            score = 0;
            Console.WriteLine($"Player{playerID} has been reset");

            switch (playerID)
            {
                case 1:
                    headPos = new Point(2, 2);
                    direction = (Direction)3;
                    break;
                case 2:
                    headPos = new Point(board.X - 1, 2);
                    direction = (Direction)1;
                    break;
                case 3:
                    headPos = new Point(2, board.Y - 1);
                    direction = (Direction)3;
                    break;
                case 4:
                    headPos = new Point(board.X - 1, board.Y - 1);
                    direction = (Direction)1;
                    break;
            }
            prevHeadPos = headPos;
            Console.WriteLine($"Player{playerID} spawned at " + headPos.ToString());
        }
    }
}
