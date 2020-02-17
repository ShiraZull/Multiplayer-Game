using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGame
{
    public class Blob
    {
        public Point position;
        public Point board;
         


        public Blob(Point position)
        {
            this.position = position;
        }


        public Blob(List<Blob> blobs, List<Player> players, List<Point> allCoordinates)
        {
            //List<Point> availableCoordinates = allCoordinates;
            List<Point> availableCoordinates = new List<Point>(allCoordinates);

            List<Point> collisionCoordinates = new List<Point>();

            foreach (Blob blob in blobs) collisionCoordinates.Add(blob.position);
            foreach (Player player in players)
            {
                collisionCoordinates.Add(player.headPos);
                foreach (Body body in player.bodies) collisionCoordinates.Add(body.position);
            }
            
            foreach (Point position in collisionCoordinates) availableCoordinates.Remove(position);
            Random rand = new Random();
            int spawnPlace = rand.Next(availableCoordinates.Count);
            position = availableCoordinates[spawnPlace];
        }


    }
}
