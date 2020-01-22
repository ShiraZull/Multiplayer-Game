using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MultiplayerGameServer
{
    public class Board
    {
        public Point size;

        public Board(int boardSize)
        {
            size = new Point(boardSize);
        }


    }
}
