﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AStarAlgorithm
{
    public void FindThePath(MazeGenerator mazeGenerator, ResultDataCollector resultDataCollector, int heuristicFactor)
    {
        var counter = 0;
        var endReached = false;
        List<Node> openList = new List<Node>() { mazeGenerator.Start };
        List<Node> closedList = new List<Node>();
        List<Node> neighbours;
        Node currentNode;

        resultDataCollector.Open = new Dictionary<int, List<Node>>();
        resultDataCollector.Closed = new Dictionary<int, List<Node>>();

        var sw = new System.Diagnostics.Stopwatch();

        sw.Start();

        while (true)
        {
            currentNode = openList.OrderBy(i => i.FCost).First();

            resultDataCollector.Closed.Add(counter, new List<Node>() { currentNode });

            closedList.Add(currentNode);
            openList.Remove(currentNode);

            neighbours = GetNeighbours(currentNode, mazeGenerator.GetMaze);

            foreach (var n in neighbours)
            {
                if (openList.Contains(n))
                {
                    if (n.GCost < currentNode.GCost)
                    {
                        n.SetParent(currentNode);
                        n.CalculateTransitions(currentNode, mazeGenerator.Finish, heuristicFactor);
                    }
                }
                else if (!closedList.Contains(n))
                {
                    if (resultDataCollector.Open.ContainsKey(counter))
                        resultDataCollector.Open[counter].Add(n);
                    else
                        resultDataCollector.Open.Add(counter, new List<Node> { n });

                    openList.Add(n);
                    n.SetParent(currentNode);
                    n.CalculateTransitions(currentNode, mazeGenerator.Finish, heuristicFactor);
                }
            }

            if (currentNode.Equals(mazeGenerator.Finish) || openList.Count == 0)
            {
                endReached = currentNode.Equals(mazeGenerator.Finish);
                break;
            }

            ++counter;
        }

        resultDataCollector.Passes = counter;
        resultDataCollector.Path = new List<Node>();

        if (endReached)
        {
            GetPath(closedList.Last(), resultDataCollector.Path);

            resultDataCollector.Path.Reverse();

            resultDataCollector.Result = "End point reached!\nTime: " + sw.ElapsedMilliseconds + "ms\nPasses: " + counter;
        }
        else
            resultDataCollector.Result = "End point couldn't be reached!";

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

    private void GetPath(Node node, List<Node> path)
    {
        if (node.Parent == null)
            return;
        else
        {
            path.Add(node);
            GetPath(node.Parent, path);
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

    public override bool Equals(object obj)
    {
        var n = (Node)obj;
        return n.X == X && n.Y == Y;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
