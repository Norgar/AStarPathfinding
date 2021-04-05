using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AStarAlgorithm
{
    public void FindThePath(int[,] _maze, int startX, int startY, int endX, int endY, out List<Node> path, out Dictionary<int, List<Node>> open, out Dictionary<int, List<Node>> closed, out string result, int heuristicFactor)
    {
        var counter = 0;
        var endReached = false;
        var endNode = new Node(endX, endY);
        var startNode = new Node(startX, startY);
        List<Node> openList = new List<Node>() { startNode };
        List<Node> closedList = new List<Node>();
        List<Node> neighbours;
        Node node;

        open = new Dictionary<int, List<Node>>();
        closed = new Dictionary<int, List<Node>>();

        var sw = new System.Diagnostics.Stopwatch();

        sw.Start();

        while (true)
        {
            node = openList.OrderBy(i => i.FCost).First();

            closed.Add(counter, new List<Node>() { node });

            closedList.Add(node);
            openList.Remove(node);

            neighbours = GetNeighbours(node, _maze);

            foreach (var n in neighbours)
            {
                if (IsNodeInList(n, openList))
                {
                    if (n.GCost < node.GCost)
                    {
                        n.SetParent(node);
                        n.CalculateTransitions(node, endNode, heuristicFactor);
                    }
                }
                else if (!IsNodeInList(n, closedList))
                {
                    if (open.ContainsKey(counter))
                        open[counter].Add(n);
                    else
                        open.Add(counter, new List<Node> { n });

                    openList.Add(n);
                    n.SetParent(node);
                    n.CalculateTransitions(node, endNode, heuristicFactor);
                }
            }

            if (IsNodeInList(endNode, openList) || openList.Count == 0)
            {
                endReached = IsNodeInList(endNode, openList);
                break;
            }

            ++counter;
        }

        path = new List<Node>();

        if (endReached)
        {
            GetPath(closedList.Last(), ref path);

            path.Reverse();

            result = "End point reached!\nTime: " + sw.ElapsedMilliseconds + "ms\nPasses: " + counter;
        }
        else
            result = "End point couldn't be reached!";

        sw.Stop();
    }

    private List<Node> GetNeighbours(Node node, int[,] maze)
    {
        var list = new List<Node>();

        var mazeSize = maze.GetLength(0); //TODO Make size for non-quad maze

        for (int x = node.X - 1; x <= node.X + 1; x++)
        {
            for (int y = node.Y - 1; y <= node.Y + 1; y++)
            {
                if (x >= 0 && y >= 0 && x < mazeSize && y < mazeSize && maze[x, y] != 1)
                {
                    list.Add(new Node(x, y));
                }
            }
        }

        return list;
    }

    private bool IsNodeInList(Node node, in List<Node> list) => list.Count(n => n.IsSamePosition(node)) > 0;

    private void GetPath(Node node, ref List<Node> path)
    {
        if (node.Parent == null)
            return;
        else
        {
            path.Add(node);
            GetPath(node.Parent, ref path);
        }
    }
}

public class Node
{
    const int DirectTransitionCost = 10;
    const int DiagonalTransitionCost = 14;

    public Node Parent { get; private set; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public int GCost { get; private set; }
    public int HCost { get; private set; }
    public int FCost => GCost + HCost;

    public bool IsSamePosition(Node node) => node.X == X && node.Y == Y;
    public bool IsDiagonalTransition(Node node) => node.X != X && node.Y != Y;

    public Node(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetParent(Node parent) => Parent = parent;

    public int GetTransitionCostTo(Node node) => GCost + (IsDiagonalTransition(node) ? DiagonalTransitionCost : DirectTransitionCost);

    public void CalculateTransitions(Node node, Node end, int heuristicFactor)
    {
        GCost = node.GCost + (IsDiagonalTransition(node) ? DiagonalTransitionCost : DirectTransitionCost);
        HCost = (Mathf.Abs(end.X - X) + Mathf.Abs(end.Y - Y)) * heuristicFactor;
    }
}
