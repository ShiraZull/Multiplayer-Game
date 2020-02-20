using System;

namespace MultiplayerGameLibrary
{
    public class TurnManager
    {
        public float time;
        public float turnDelay;
        public int turn;
        public bool nextTurn;

        private TimeSpan timeSpan = new TimeSpan();
        private double timeDiff;
        private DateTime currentDateTime;
        private DateTime prevDateTime;

        /// <summary>
        /// A class that handles time and what turn shall be next
        /// </summary>
        /// <param name="turnDelay"></param>
        /// <param name="startCountdown"></param>
        public TurnManager(float turnDelay, float startCountdown)
        {
            this.turnDelay = turnDelay;
            time = -startCountdown;
            turn = 0;
            nextTurn = false;
            currentDateTime = DateTime.Now;
            Console.WriteLine($"TurnSettings: Countdown = {startCountdown} milliseconds\nTurnSettings: Interval = {turnDelay} milliseconds");
        }

        /// <summary>
        /// Updates the time difference between viritual updates
        /// </summary>
        public void UpdateTimeDiff()
        {
            prevDateTime = currentDateTime;
            currentDateTime = DateTime.Now;
            timeSpan = currentDateTime - prevDateTime;
            timeDiff = timeSpan.TotalMilliseconds;
            
            // A warning if the lag is above 20 milliseconds
            if (timeDiff >= 20) Console.WriteLine("Warning: GameTime is {0} milliseconds", timeDiff);
        }

        /// <summary>
        /// Updates the time for the game, after hitting 0, first turn will begin and then change every turnDelay of milliseconds.
        /// </summary>
        public void UpdateTurn()
        {
            nextTurn = false;
            time += (float)timeDiff;

            // Whenever time is over the turninterval, next turn
            if (time >= turnDelay)
            {
                turn++;
                nextTurn = true;
                time -= turnDelay;
                Console.WriteLine("----- Turn: {0} -----", turn);
                return;
            }

            // Whenever times is passing 0, first turn
            if (turn == 0 && time >= 0)
            {
                turn = 1;
                nextTurn = true;
                Console.WriteLine("||||| START ||||||");
                Console.WriteLine("----- Turn: 1 -----");
            }


        }

        /// <summary>
        /// A method for resseting the clock
        /// </summary>
        /// <param name="startCountdown"></param>
        public void Reset(float startCountdown)
        {
            time = -startCountdown;
            turn = 0;
            nextTurn = false;
            currentDateTime = DateTime.Now;

            Console.WriteLine($"RESET_TurnSettings: Countdown = {startCountdown} milliseconds\nTurnSettings: Interval = {turnDelay} milliseconds");
        }

    }
}
