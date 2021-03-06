using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AStarAlgorithm
{
    public void FindThePath(MazeGenerator generator, ResultDataCollector collector)
    {
        Node node;
        var cost = 1;
        var minCost = 0;
        var counter = 0;
        var maze = generator.GetMaze;
        var open = new Dictionary<int, Node>() { { generator.Start.GetHashCode(), generator.Start } };
        var closed = new HashSet<Node>();
        var sw = new System.Diagnostics.Stopwatch();
        collector.Open = new Dictionary<int, List<Node>>();
        collector.Closed = new Dictionary<int, Node>();

        sw.Start();

        while (!open.ContainsKey(generator.End.GetHashCode()) && open.Any())
        {
            minCost = open.Values.Min(m => m.FCost);
            node = open.Values.First(n => n.FCost == minCost);

            collector.Closed.Add(counter, node);

            closed.Add(node);
            open.Remove(node.GetHashCode());

            var neighbours = GetNeighbours(node);

            foreach (var item in neighbours)
            {
                var n = open.ContainsKey(item.GetHashCode())
                    ? open[item.GetHashCode()]
                    : item;

                if (closed.Contains(n) || !IsNodeAvailable(n, maze))
                    continue;

                if (collector.Open.ContainsKey(counter))
                    collector.Open[counter].Add(n);
                else
                    collector.Open.Add(counter, new List<Node> { n });

                if (!open.ContainsKey(n.GetHashCode()))
                {
                    n.SetParent(node);
                    n.Estimate(generator.End);
                    n.SetCost(node.GCost + cost);
                    open.Add(n.GetHashCode(), n);
                }
                else if (IsLowerCostWay(node, n, cost))
                {
                    node.SetParent(n);
                    node.SetCost(n.GCost + cost);
                }
            }

            ++counter;
        }

        collector.Passes = counter;
        collector.Path = new List<Node>();

        if (open.ContainsKey(generator.End.GetHashCode()))
        {
            BuildPath(closed.Last(), collector.Path);
            collector.Result = "End point reached!\nTime: " + sw.ElapsedMilliseconds + "ms\nPasses: " + counter + "\nPath length: " + collector.Path.Count;
        }
        else
            collector.Result = "End point couldn't be reached!";

        sw.Stop();
    }

    private Node[] GetNeighbours(Node node)
    {
        return new Node[]
        {
            new Node(node.X+1, node.Y),
            new Node(node.X-1, node.Y),
            new Node(node.X , node.Y+1),
            new Node(node.X , node.Y-1),
        };
    }

    private bool IsNodeAvailable(Node node, in int[,] maze)
    {
        var size = maze.GetLength(0);
        return node.X >= 0 && node.Y >= 0 && node.X < size && node.Y < size && maze[node.X, node.Y] != 1;
    }

    private bool IsLowerCostWay(Node current, Node neighbour, int transitionCost)
    {
        return neighbour.GCost + transitionCost < current.GCost;
    }

    private void BuildPath(Node node, List<Node> path)
    {
        if (node.Parent == null)
        {
            path.Reverse();
            return;
        }
        else
        {
            path.Add(node);
            BuildPath(node.Parent, path);
        }
    }
}

public class Node
{
    public Node Parent { get; private set; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public int GCost { get; private set; }
    public int HCost { get; private set; }
    public int FCost => GCost + HCost;

    public Node(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetParent(Node parent) => Parent = parent;

    public void SetCost(int cost) => GCost = cost;

    public void Estimate(Node end)
    {
        HCost = Mathf.Abs(end.X - X) + Mathf.Abs(end.Y - Y);
        //HCost = (int)(Mathf.Pow(end.X - X, 2) + Mathf.Pow(end.Y - Y, 2));
    }

    public override bool Equals(object obj)
    {
        var n = (Node)obj;
        return n.X == X && n.Y == Y;
    }

    public override int GetHashCode()
    {
        int tmp = Y + (X + 1) / 2;
        return X + (tmp * tmp);
    }
}
