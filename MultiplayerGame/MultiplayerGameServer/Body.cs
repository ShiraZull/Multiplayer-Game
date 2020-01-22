using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MultiplayerGameServer
{
    public class Body
    {
        public Point position;
        public int life;

        public Body(int x, int y, int lifeSpan)
        {
            position = new Point(x, y);
            life = lifeSpan;
        }

        public void AddLife() { life++; }
        public void SubLife() { life--; }

    }
}
