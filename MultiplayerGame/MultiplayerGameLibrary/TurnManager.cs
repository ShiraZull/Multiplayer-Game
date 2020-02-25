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
        public TurnManager(float startCountdown)
        {
            time = -startCountdown;
            turn = 0;
            nextTurn = false;
            currentDateTime = DateTime.Now;
            Console.WriteLine($"TurnSettings: Countdown = {startCountdown} milliseconds\nTurnSettings: N/A");
        }

        /// <summary>
        /// Updates the time difference between viritual updates
        /// </summary>
        /// <param name="warningMilliseconds"></param>
        public void UpdateTimeDiff(int warningMilliseconds)
        {
            prevDateTime = currentDateTime;
            currentDateTime = DateTime.Now;
            timeSpan = currentDateTime - prevDateTime;
            timeDiff = timeSpan.TotalMilliseconds;
            
            // A warning if the lag is above warning milliseconds, unless it's 0 and it will ignore the case
            if (timeDiff >= warningMilliseconds && warningMilliseconds != 0) Console.WriteLine($"Warning: GameTime is {timeDiff} milliseconds");
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
                Console.WriteLine("|||||| START ||||||");
                Console.WriteLine("----- Turn: 1 -----");
            }
        }

        /// <summary>
        /// Changes to next turn, made for client, also gives the time difference between each turn
        /// </summary>
        public void NextTurn()
        {
            turn++;
            nextTurn = true;
            prevDateTime = currentDateTime;
            currentDateTime = DateTime.Now;
            timeSpan = currentDateTime - prevDateTime;
            if (turn == 1)
            {
                Console.WriteLine("|||||| START ||||||");
                Console.WriteLine($"----- Turn: 1 ----- ({timeSpan.TotalMilliseconds})");
            }
            else Console.WriteLine($"----- Turn: {turn} ----- ({timeSpan.TotalMilliseconds})");
        }

        /// <summary>
        /// Used for countdowns for the client to then change to first turn
        /// </summary>
        public void UpdateCountdown()
        {
            if (time < 0)
            {
                time += (float)timeDiff;
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
