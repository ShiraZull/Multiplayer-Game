using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MultiplayerGameLibrary
{
    public class Blob
    {
        public Point position;
        public Point board;



        public Blob(Point position)
        {
            this.position = position;
        }


        public Blob(List<Blob> blobs, List<Player> players, Point grid)
        {
            List<Point> availableCoordinates = new List<Point>(GetAllCoordinates(grid));

            List<Point> collisionCoordinates = new List<Point>();



            Console.WriteLine($"||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
            Console.WriteLine($"Collision before calc: {collisionCoordinates.Count}");
            Console.WriteLine($"Available before calc: {availableCoordinates.Count}");

            foreach (Blob blob in blobs) collisionCoordinates.Add(blob.position);
            foreach (Player player in players)
            {
                collisionCoordinates.Add(player.headPos);
                foreach (Body body in player.bodies) collisionCoordinates.Add(body.position);
            }

            foreach (Point position in collisionCoordinates) availableCoordinates.Remove(position);
            Random rand = new Random();
            int spawnPlace = rand.Next(availableCoordinates.Count);

            Console.WriteLine($"--------------------------------------------------------------------");
            Console.WriteLine($"Collision after calc: {collisionCoordinates.Count}");
            Console.WriteLine($"Available after calc: {availableCoordinates.Count}");
            Console.WriteLine($"SpawnPlace Index: {spawnPlace}");
            Console.WriteLine($"||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");

            position = availableCoordinates[spawnPlace];
        }

        public List<Point> GetAllCoordinates(Point grid)
        {
            List<Point> allCoordinates = new List<Point>();
            for (int y = 1; y <= grid.Y; y++)
            {
                for (int x = 1; x <= grid.X; x++)
                {
                    allCoordinates.Add(new Point(x, y));
                }
            }
            return allCoordinates;
        }
    }
}
