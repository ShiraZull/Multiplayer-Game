using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGame
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
