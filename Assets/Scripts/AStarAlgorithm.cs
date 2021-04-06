using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AStarAlgorithm
{
    public void FindThePath(MazeGenerator mazeGenerator, ResultDataCollector resultDataCollector, int transitionCost, int heuristicFactor)
    {
        var counter = 0;
        var endReached = false;
        HashSet<Node> open = new HashSet<Node>() { mazeGenerator.Start };
        HashSet<Node> closed = new HashSet<Node>();
        HashSet<Node> neighbours;
        Node currentNode;

        resultDataCollector.Open = new Dictionary<int, List<Node>>();
        resultDataCollector.Closed = new Dictionary<int, List<Node>>();

        var sw = new System.Diagnostics.Stopwatch();

        sw.Start();

        while (true)
        {
            currentNode = open.OrderBy(i => i.FCost).First();

            resultDataCollector.Closed.Add(counter, new List<Node>() { currentNode });

            closed.Add(currentNode);
            open.Remove(currentNode);

            neighbours = GetNeighbours(currentNode, mazeGenerator.GetMaze);

            foreach (var n in neighbours)
            {
                if (open.Contains(n))
                {
                    if (n.GCost < currentNode.GCost)
                    {
                        n.SetParent(currentNode);
                        n.CalculateTransitions(currentNode, mazeGenerator.Finish, transitionCost, heuristicFactor);
                    }
                }
                else if (!closed.Contains(n) && IsNodeAvailable(n, mazeGenerator.GetMaze))
                {
                    if (resultDataCollector.Open.ContainsKey(counter))
                        resultDataCollector.Open[counter].Add(n);
                    else
                        resultDataCollector.Open.Add(counter, new List<Node> { n });

                    open.Add(n);
                    n.SetParent(currentNode);
                    n.CalculateTransitions(currentNode, mazeGenerator.Finish, transitionCost, heuristicFactor);
                }
            }

            if (open.Contains(mazeGenerator.Finish) || open.Count == 0)
            {
                endReached = open.Contains(mazeGenerator.Finish);
                break;
            }

            ++counter;
        }

        resultDataCollector.Passes = counter;
        resultDataCollector.Path = new List<Node>();

        if (endReached)
        {
            BuildPath(closed.Last(), resultDataCollector.Path);
            resultDataCollector.Result = "End point reached!\nTime: " + sw.ElapsedMilliseconds + "ms\nPasses: " + counter + "\nPath length: " + resultDataCollector.Path.Count;
        }
        else
            resultDataCollector.Result = "End point couldn't be reached!";

        sw.Stop();
    }

    private HashSet<Node> GetNeighbours(Node node, int[,] maze)
    {
        return new HashSet<Node>()
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

    public void CalculateTransitions(Node node, Node end, int transitionCost, int heuristicFactor)
    {
        GCost = node.GCost + transitionCost;
        HCost = (Mathf.Abs(end.X - X) + Mathf.Abs(end.Y - Y)) * heuristicFactor;
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
