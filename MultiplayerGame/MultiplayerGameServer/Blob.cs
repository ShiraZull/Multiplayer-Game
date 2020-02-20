using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace MultiplayerGameServer
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
            List<Point> availableCoordinates = new List<Point>(allCoordinates);

            List<Point> collisionCoordinates = new List<Point>();



            Console.WriteLine($"||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
            Console.WriteLine($"Collision before calc: {collisionCoordinates.Count}");
            Console.WriteLine($"AllCoord before calc: {allCoordinates.Count}");
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
            Console.WriteLine($"AllCoord after calc: {allCoordinates.Count}");
            Console.WriteLine($"Available after calc: {availableCoordinates.Count}");
            Console.WriteLine($"SpawnPlace Index: {spawnPlace}");
            Console.WriteLine($"||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");

            position = availableCoordinates[spawnPlace];
        }


    }
}
