using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AStarTemp
{
    public class AStarTemp
    {
        public void Find(int[,] maze, Node a, Node b)
        {
            var closed = new HashSet<Node>();
            var open = new HashSet<Node>();
            var map = new HashSet<Node>();

            var length = maze.GetLength(0);

            for (int x = 0; x < length; x++)
                for (int y = 0; y < length; y++)
                {
                    var node = new Node(x, y);
                    node.estimate_h(b);
                    map.Add(node);
                }

            open.Add(a);

            while (!open.Contains(b) && open.Any())
            {
                var current = open.First();

                closed.Add(current);
                open.Remove(current);

                if (current.Equals(b))
                    break;

                var neighbours = get_neighbours(current);

                foreach (var neighbour in neighbours)
                {
                    
                }
            }
        }

        private Node[] get_neighbours(Node n)
        {
            return new Node[]
            {
                new Node(n.x+1, n.y),
                new Node(n.x-1, n.y),
                new Node(n.x, n.y+1),
                new Node(n.x, n.y-1)
            };
        }
    }

    public struct Node
    {
        public int x, y, g, h;
        public int f => g + h;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
            g = h = 0;
        }

        public void set_g(int g) => this.g = g;
        public void set_h(int h) => this.h = h;

        public void estimate_g(Node n) => g = n.g + 1;
        public void estimate_h(Node b) => h = Mathf.Abs(b.x - x) + Mathf.Abs(b.y - y);

        public override bool Equals(object obj) => obj is Node && obj.GetHashCode() == GetHashCode();

        public override int GetHashCode()
        {
            var t = y + (x + 1) / 2;
            return x + t * t;
        }
    }
}
