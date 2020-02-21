﻿using Microsoft.Xna.Framework;

namespace MultiplayerGameLibrary
{
    public class Body
    {
        public Point position;
        public int life;

        public Body(Point position, int lifeSpan)
        {
            this.position = position;
            life = lifeSpan;
        }

        public void AddLife() { life++; }
        public void SubLife() { life--; }
    }
}
