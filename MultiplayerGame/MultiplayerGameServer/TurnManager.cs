using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MultiplayerGameServer
{
    public class TurnManager
    {
        public float time;
        public float turnDelay;
        public int turn;
        public int prevTurn;
        public bool active;
        public GameTime gameTime;
        int i; // for testing
        

        public TurnManager(float turnDelay, float startCountdown)
        {
            this.turnDelay = turnDelay;
            time = -startCountdown;
            turn = 0;
            prevTurn = 0;
            active = false;
        }

        public bool NextTurn()
        {
            if(active)
            {
                if (turn != prevTurn) return true;
            }
            return false;
        }

        public void UpdateTime()
        {
            time += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (i % 5 == 0) Console.WriteLine("Time: {0}", time);

            if (turn == 0 && time >= 0)
            {
                turn = 1;
                Console.WriteLine("Turn: {0}", turn);
            }
        }

    }
}
