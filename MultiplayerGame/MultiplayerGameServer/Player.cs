using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace MultiplayerGameServer
{
    class Player
    {
        public NetConnection netConnection;
        public NetPeer netPeer;
        public byte playerID;

        public bool alive;
        public int score;
        public List<Body> bodies;
        public Point headPos = new Point(3,3);
        public Point prevHeadPos;
        public enum Direction : byte
        {
            Up,
            Left,
            Down,
            Right
        }
        public Direction direction;
        public Direction prevDirection;

        public Point board;
        private Random rand = new Random();

        /// <summary>
        /// A contruct that add another player/client to the game/server
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ID"></param>
        public Player(NetConnection connection, byte ID)
        {
            netConnection = connection;
            netPeer = connection.Peer;
            playerID = ID;
            Console.WriteLine("Player{0} has connected as {1} with local address {2}", playerID, netConnection, netPeer.Configuration.LocalAddress);
        }

        public void Move()
        {
            prevHeadPos = headPos;
            prevDirection = direction;
            switch((byte)direction)
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
            Console.WriteLine("Player{0} has moved {1} to {2}", playerID, (Direction) direction, headPos);
        }

        
        public void CollisionWall()
        {
            if(headPos.X <= 0 || headPos.X > board.X)
            {
                headPos = prevHeadPos;

                if (headPos.Y <= 1) direction = Direction.Down;
                else if (headPos.Y >= board.Y) direction = Direction.Up;
                else // TODO: Fix so that player dont randomly go up when you're going down, at the right wall, pressing right, and by chance move up or down -----------------
                {
                    int i = rand.Next(2);
                    if(i == 1) direction = Direction.Up;
                    else direction = Direction.Down;
                }
                Move();
                return;
            }

            if (headPos.Y <= 0 || headPos.Y > board.Y)
            {
                headPos = prevHeadPos;

                if (headPos.X <= 1) direction = Direction.Right;
                else if (headPos.X >= board.X) direction = Direction.Left;
                else
                {
                    int i = rand.Next(2);
                    if (i == 1) direction = Direction.Left;
                    else direction = Direction.Right;
                }
                Move();
                return;
            }
        }


        /// <summary>
        /// Reset the player and its position according to the ID and board size
        /// </summary>
        /// <param name="boardSize"></param>
        public void Reset(Point boardSize)
        {
            switch (playerID)
            {
                case 1:
                    headPos = new Point(1, 1);
                    direction = (Direction)3;
                    Console.WriteLine("Player{0} spawned at " + headPos.ToString(), playerID);
                    break;
                case 2:
                    headPos = new Point(boardSize.X, 1);
                    direction = (Direction)1;
                    Console.WriteLine("Player{0} spawned at " + headPos.ToString(), playerID);
                    break;
                case 3:
                    headPos = new Point(1, boardSize.Y);
                    direction = (Direction)3;
                    Console.WriteLine("Player{0} spawned at " + headPos.ToString(), playerID);
                    break;
                case 4:
                    headPos = boardSize;
                    direction = (Direction)1;
                    Console.WriteLine("Player{0} spawned at " + headPos.ToString(), playerID);
                    break;
            }
            prevHeadPos = headPos;
            bodies.Clear();
            alive = true;
            score = 0;
            Console.WriteLine("Player{0} has been reset", playerID);
        }

    }
}
