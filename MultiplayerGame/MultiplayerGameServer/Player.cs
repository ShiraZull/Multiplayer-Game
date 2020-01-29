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
        public List<Body> bodies = new List<Body>();
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

            if (CollisionWall()) Console.WriteLine("Player{0} has moved {1} to {2} (Player{0} got outside the board)", playerID, (Direction)direction, headPos);
            else Console.WriteLine("Player{0} has moved {1} to {2}", playerID, (Direction) direction, headPos);
            
        }
        
        public bool CollisionWall()
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
            else
            {
                return false;
            }
        }

        public bool CollisionBlob(List<Blob> blobs)
        {
            foreach (Blob blob in blobs)
            {
                if (blob.position == headPos)
                {
                    bodies.Add(new Body(prevHeadPos, ++score));
                    Console.WriteLine("Player{0} has eaten a blob and has current score: {2}", playerID, blob, score);
                    return true;
                }
            }
            return false;
        }
        
        public void MoveBody()
        {
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
