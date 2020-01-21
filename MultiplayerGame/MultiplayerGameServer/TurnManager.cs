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
        int i; // for testing
        float j = 1000f; // for testing

        private float gameTime;
        private float currentGameTime;
        private float prevGameTime;

        public TurnManager(float turnDelay, float startCountdown)
        {
            this.turnDelay = turnDelay;
            time = -startCountdown;
            turn = 0;
            prevTurn = 0;
            active = false;
        }

        public void UpdateGameTime()
        {

            i++;
            if (i == (int)j)
            {
                prevGameTime = currentGameTime;
                currentGameTime = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                if (currentGameTime - prevGameTime < 0) gameTime = currentGameTime - prevGameTime + 60000;
                else gameTime = currentGameTime - prevGameTime;
                Console.WriteLine("Current amount of milliseconds: {0} change of gameTime: {1}", currentGameTime, gameTime);
            }
            if(i==(int)j)
            {
                j = j*1.01f;
                i = 0;
            }
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
            time += 1;

            if (i % 5 == 0) Console.WriteLine("Time: {0}", time);

            if (turn == 0 && time >= 0)
            {
                turn = 1;
                Console.WriteLine("Turn: {0}", turn);
            }
        }

    }
}
