using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AStarAlgorithm
{
    public void FindThePath(MazeGenerator generator, ResultDataCollector collector)
    {
        Node node;
        var cost = 1;
        var counter = 0;
        var maze = generator.GetMaze;
        var open = new HashSet<Node>() { generator.Start };
        var closed = new HashSet<Node>();
        var sw = new System.Diagnostics.Stopwatch();
        collector.Open = new Dictionary<int, List<Node>>();
        collector.Closed = new Dictionary<int, List<Node>>();

        sw.Start();

        while (!open.Contains(generator.End) && open.Any())
        {
            node = open.OrderBy(n => n.FCost)./*ThenByDescending(n => n.GCost).*/First();

            collector.Closed.Add(counter, new List<Node>() { node });

            closed.Add(node);
            open.Remove(node);

            var neighbours = GetNeighbours(node);

            foreach (var n in neighbours)
            {
                if (!closed.Contains(n) && IsNodeAvailable(n, maze))
                {
                    if (collector.Open.ContainsKey(counter))
                        collector.Open[counter].Add(n);
                    else
                        collector.Open.Add(counter, new List<Node> { n });

                    if (open.Contains(n))
                    {
                        if (n.GCost < node.GCost)
                        {
                            node.SetParent(n);
                            node.SetCost(n.GCost + cost);
                        }
                    }
                    else
                    {
                        open.Add(n);
                        n.SetParent(node);
                        n.Estimate(generator.End);
                        n.SetCost(node.GCost + cost);
                    }
                }
            }

            ++counter;
        }

        collector.Passes = counter;
        collector.Path = new List<Node>();

        if (open.Contains(generator.End))
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