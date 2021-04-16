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

        node = generator.WayPointsGrid.Keys.OrderBy(w => w.GCost).First();
        var trackedNodes = new HashSet<Node>() { node/*, new Node(85, 32) */};
        collector.Waypoints = new List<Node> { node };

        // searching by way points loop

        while (true)
        {
            ++counter;

            var waypoints = generator.WayPointsGrid[node];

            //Debug.Log("at node " + node.x + ":" + node.y
            //    + " - wp coun: " + waypoints.Count
            //    + " - tracked: " + waypoints.Count(w => trackedNodes.Contains(w)));

            var closestWayPoint = waypoints
                .Where(w => !trackedNodes.Contains(w))
                .OrderBy(w => w.FCost).First();

            closestWayPoint.EstimateG(node);
            node = closestWayPoint;
            trackedNodes.Add(node);

            collector.Waypoints.Add(node);

            if (node.Equals(generator.End) || counter > 1000)
            {
                if (node.Equals(generator.End))
                    Debug.Log("end reched :) " + counter + " passes " + sw.ElapsedMilliseconds + "ms");
                else
                    Debug.Log("end not reched :( " + counter);

                Debug.Log("route: " + string.Join(" - ", collector.Waypoints));

                break;
            }
        }

        // build path loop
        counter = 0;
        collector.Path = new List<Node>();
        var subPath = new List<Node>();

        for (int i = 0; i < collector.Waypoints.Count; i++)
        {
            var start = collector.Waypoints[i];

            if (start.Equals(generator.End))
                break;

            var end = collector.Waypoints[i + 1];
            node = start;

            open.Clear();
            open.Add(node.GetHashCode(), node);
            closed.Clear();


            while (true)
            {
                node = open.Values.OrderBy(o => o.FCost).First();

                if (node.Equals(end) || counter > 10000)
                    break;

                ++counter;

                closed.Add(node);
                open.Remove(node.GetHashCode());
                collector.Closed.Add(counter, node);

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
                        n.Estimate(end);
                        n.SetCost(node.GCost + cost);
                        open.Add(n.GetHashCode(), n);
                    }
                    else if (IsLowerCostWay(node, n, cost))
                    {
                        node.SetParent(n);
                        node.SetCost(n.GCost + cost);
                    }
                }
            }

            BuildPath(closed.Last(), subPath);
            subPath.Reverse();
            collector.Path.AddRange(subPath);
            subPath.Clear();
        }

        collector.Passes = counter;

        //Debug.Log("path: " + collector.Path.Count + " / time: " + sw.ElapsedMilliseconds + "ms");
        if (node.Equals(generator.End)/*open.ContainsKey(generator.End.GetHashCode())*/)
        {
            //BuildPath(closed.Last(), collector.Path);
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
            new Node(node.x+1, node.y),
            new Node(node.x-1, node.y),
            new Node(node.x , node.y+1),
            new Node(node.x , node.y-1),
        };
    }

    private bool IsNodeAvailable(Node node, in int[,] maze)
    {
        var size = maze.GetLength(0);
        return node.x >= 0 && node.y >= 0 && node.x < size && node.y < size && maze[node.x, node.y] != 1;
    }

    private bool IsLowerCostWay(Node current, Node neighbour, int transitionCost)
    {
        return neighbour.GCost + transitionCost < current.GCost;
    }

    private void BuildPath(Node node, List<Node> path)
    {
        if (node.Parent == null)
        {
            return;
        }
        else
        {
            path.Add(node);
            BuildPath(node.Parent, path);
        }
    }
}
