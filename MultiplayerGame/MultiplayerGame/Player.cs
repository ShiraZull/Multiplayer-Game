using Lidgren.Network;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MultiplayerGame
{
    public class Player
    {
        public byte playerID;

        public bool alive;
        public int score;
        public List<Body> bodies = new List<Body>();
        public Point headPos = new Point(3,3);
        public Point prevHeadPos;

        public Player(byte playerID) { this.playerID = playerID; }

        public void NewPosition(Point position)
        {
            prevHeadPos = headPos;
            headPos = position;
        }
        
        public void Dead() { alive = false; }

        /// <summary>
        /// Moves the body forward, deletes last and adds a body at prevHeadPos
        /// </summary>
        /// <param name="If player has eaten blob, it only adds a body and a point"></param>
        public void MoveBody(bool eatBlob)
        {
            if (eatBlob)
            {
                bodies.Add(new Body(prevHeadPos, ++score));
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

        public void Reset(Point headPosition)
        {
            alive = true;
            score = 0;
            bodies.Clear();
            headPos = headPosition;
            prevHeadPos = headPos;
        }

    }
}
