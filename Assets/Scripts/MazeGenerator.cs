using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{
    private int[,] _maze;
    private Texture2D _texture;

    public int[,] GetMaze => _maze;

    public Dictionary<Node, List<Node>> WayPointsGrid { get; private set; }
    public Dictionary<Node, List<Node>> WayPointPorts { get; private set; }
    public List<Node> WayPoints { get; private set; }
    public Node Start { get; private set; }
    public Node End { get; private set; }

    private bool IsObstacle(int x, int y) =>
        x < 0 ||
        y < 0 ||
        x >= _maze.GetLength(0) ||
        y >= _maze.GetLength(0) ||
        _maze[x, y] == 1;

    private bool IsObstacleCollide(int x, int y, Node a, Node b) =>
        MathHelper.IsRectIntersect(a.x + 0.5f, a.y + 0.5f, b.x + 0.5f, b.y + 0.5f, x, y, 1, 1);

    public void GenerateMaze(Texture2D mazeMap, float noizeSensitivity = 0.5f)
    {
        _maze = new int[mazeMap.width, mazeMap.height];

        for (int x = 0; x < mazeMap.width; x++)
        {
            for (int y = 0; y < mazeMap.height; y++)
            {
                _maze[x, y] = (int)(mazeMap.GetPixel(x, y).grayscale < noizeSensitivity
                    ? RestrictionType.Wall
                    : RestrictionType.None);
            }
        }
    }

    public void GenerateWayPoints()
    {
        if (Start == null)
            return;

        if (End == null)
            return;

        var size = _maze.GetLength(0);
        var wplist = new Dictionary<Point, List<int[]>>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (_maze[x, y] == 0)
                    continue;

                if (!IsObstacle(x + 1, y) && !IsObstacle(x, y + 1) && !IsObstacle(x, y - 1))
                {
                    wplist.Add(
                        new Point(x + 1, y),
                        new List<int[]>
                        {
                            new int[] { x + 1, y + 1 },
                            new int[] { x + 1, y - 1 }
                        });
                }
                else if (!IsObstacle(x, y + 1) && !IsObstacle(x + 1, y) && !IsObstacle(x - 1, y))
                {
                    wplist.Add(
                        new Point(x, y + 1),
                        new List<int[]>
                        {
                            new int[] { x + 1, y + 1 },
                            new int[] { x - 1, y + 1 }
                        });
                }
                else if (!IsObstacle(x - 1, y) && !IsObstacle(x, y + 1) && !IsObstacle(x, y - 1))
                {
                    wplist.Add(
                        new Point(x - 1, y),
                        new List<int[]>
                        {
                            new int[] { x - 1, y + 1 },
                            new int[] { x - 1, y - 1 }
                        });
                }
                else if (!IsObstacle(x, y - 1) && !IsObstacle(x + 1, y) && !IsObstacle(x - 1, y))
                {
                    wplist.Add(
                        new Point(x, y - 1),
                        new List<int[]>
                        {
                            new int[] { x + 1, y - 1 },
                            new int[] { x - 1, y - 1 }
                        });
                }
                else
                {
                    if (!IsObstacle(x + 1, y + 1) && !IsObstacle(x, y + 1) && !IsObstacle(x + 1, y))
                        wplist.Add(new Point(x + 1, y + 1), new List<int[]>());

                    if (!IsObstacle(x + 1, y - 1) && !IsObstacle(x, y - 1) && !IsObstacle(x + 1, y))
                        wplist.Add(new Point(x + 1, y - 1), new List<int[]>());

                    if (!IsObstacle(x - 1, y + 1) && !IsObstacle(x, y + 1) && !IsObstacle(x - 1, y))
                        wplist.Add(new Point(x - 1, y + 1), new List<int[]>());

                    if (!IsObstacle(x - 1, y - 1) && !IsObstacle(x, y - 1) && !IsObstacle(x - 1, y))
                        wplist.Add(new Point(x - 1, y - 1), new List<int[]>());
                }
            }
        }

        WayPoints = new List<Node>();
        WayPointPorts = new Dictionary<Node, List<Node>>();
        WayPointsGrid = new Dictionary<Node, List<Node>>();

        WayPoints.Clear();
        WayPointsGrid.Clear();

        WayPoints.Add(Start);

        foreach (var item in wplist)
        {
            var wpNode = new Node(item.Key);
            wpNode.Estimate(End);
            WayPoints.Add(wpNode);
            MarkCell(wpNode.x, wpNode.y, MarkType.WayPoint);

            if (item.Value.Count > 0)
            {
                WayPointPorts.Add(wpNode, new List<Node>());
                foreach (var port in item.Value)
                {
                    WayPointPorts[wpNode].Add(new Node(port[0], port[1]));
                    MarkCell(port[0], port[1], MarkType.WayPointPort);
                }
            }
        }

        WayPoints.Add(End);

        //Debug.Log("wp count: " + wplist.Count);

        //var wpstr = string.Empty;

        //foreach (var item in WayPoints)
        //    wpstr += item.X + ":" + item.Y + " - ";

        //Debug.Log("wp: " + wpstr);

        foreach (var item in WayPoints)
            WayPointsGrid.Add(item, new List<Node>());

        foreach (var item in WayPointsGrid)
        {
            var node = item.Key;

            foreach (var point in WayPoints)
            {
                if (node.Equals(point))
                    continue;

                if (WayPointPorts.ContainsKey(node) && WayPointPorts[node].Count > 0)
                {
                    var p1 = WayPointPorts[node][0];
                    var p2 = WayPointPorts[node][1];

                    var port1 = WayPointPorts.ContainsKey(point)
                        ? GetClosestPort(p1, WayPointPorts[point][0], WayPointPorts[point][1])
                        : point;

                    var port2 = WayPointPorts.ContainsKey(point)
                        ? GetClosestPort(p2, WayPointPorts[point][0], WayPointPorts[point][1])
                        : point;

                    if (IsLinkPossible(p1, port1))
                        item.Value.Add(point);

                    if (IsLinkPossible(p2, port2) && !item.Value.Contains(point))
                        item.Value.Add(point);

                }
                else
                {
                    var port = WayPointPorts.ContainsKey(point)
                        ? GetClosestPort(node, WayPointPorts[point][0], WayPointPorts[point][1])
                        : point;

                    if (IsLinkPossible(node, port))
                        item.Value.Add(point);
                }
            }
        }

        //foreach (var item in WayPointsGrid)
        //{
        //    var str = "node " + item.Key.X + ":" + item.Key.Y + " linked to ";
        //    foreach (var point in item.Value)
        //        str += (point.X + ":" + point.Y + " - ");

        //    Debug.Log(str);
        //}
    }

    private Node GetClosestPort(Node node, Node p1, Node p2)
    {
        return node.GetEstimationValue(p1) < node.GetEstimationValue(p2) ? p1 : p2;
    }

    private bool IsLinkPossible(Node a, Node b)
    {
        //a is node 
        //b is point

        if (a.x < b.x && a.y < b.y)         //lower left
        {
            for (int x = a.x; x <= b.x; x++)
                for (int y = a.y; y <= b.y; y++)
                    if (IsObstacle(x, y) && IsObstacleCollide(x, y, a, b))
                        return false;
        }
        else if (a.x < b.x && a.y > b.y)    //upper left
        {
            for (int x = a.x; x <= b.x; x++)
                for (int y = a.y; y >= b.y; y--)
                    if (IsObstacle(x, y) && IsObstacleCollide(x, y, a, b))
                        return false;
        }
        else if (a.x > b.x && a.y > b.y)    //upper right
        {
            for (int x = a.x; x >= b.x; x--)
                for (int y = a.y; y >= b.y; y--)
                    if (IsObstacle(x, y) && IsObstacleCollide(x, y, a, b))
                        return false;
        }
        else if (a.x > b.x && a.y < b.y)    //lower right
        {
            for (int x = a.x; x >= b.x; x--)
                for (int y = a.y; y <= b.y; y++)
                    if (IsObstacle(x, y) && IsObstacleCollide(x, y, a, b))
                        return false;
        }
        else if (a.x == b.x)
        {
            var x = a.x;

            if (a.y < b.y)
                for (int y = a.y; y <= b.y; y++)
                    if (IsObstacle(x, y))
                        return false;

            if (a.y > b.y)
                for (int y = a.y; y >= b.y; y--)
                    if (IsObstacle(x, y))
                        return false;
        }
        else if (a.y == b.y)
        {
            var y = a.y;

            if (a.x < b.x)
                for (int x = a.x; x <= b.x; x++)
                    if (IsObstacle(x, y))
                        return false;

            if (a.x > b.x)
                for (int x = a.x; x >= b.x; x--)
                    if (IsObstacle(x, y))
                        return false;
        }
        else Debug.LogError(a + " - " + b + " has been linked illegal!");

        return true;
    }

    private bool IsNeighbour(Node n1, Node n2)
    {
        if (n1.x == n2.x && Mathf.Abs(n1.x - n2.x) < 4)
            return true;

        return false;
    }

    private void ClearWayPoints()
    {
        var size = _maze.GetLength(0);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                if (_texture.GetPixel(x, y) == Color.magenta)
                    _texture.SetPixel(x, y, Color.white);

        _texture.Apply();
    }

    /// <summary>
    /// The method generate maze texture based on maze array. Call GenerateMaze(Texture2D, float) firstly.
    /// </summary>
    /// <returns></returns>
    public Texture2D GenerateTexture()
    {
        if (_maze == null)
        {
            Debug.LogWarning("Call GenerateMaze firstly");
            return null;
        }

        var index = 0;
        var size = _maze.GetLength(0);
        _texture = new Texture2D(size, size);

        Color32[] color = new Color32[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                color[index++] = _maze[x, y] == 1
                    ? Color.black
                    : _maze[x, y] == 2 ? Color.cyan : Color.white;

        _texture.filterMode = FilterMode.Point;
        _texture.SetPixels32(color);
        _texture.Apply();

        SetStart(0, 0);
        SetEnd(size - 1, size - 1);

        return _texture;
    }

    internal void SetRandomStart()
    {
        SetStart(Random.Range(0, 99), Random.Range(0, 99));
    }

    internal void SetRandomEnd()
    {
        SetEnd(Random.Range(0, 99), Random.Range(0, 99));
    }

    internal void SetStart(int x, int y)
    {
        if (_maze[x, y] == 1)
            return;

        if (Start != null)
            MarkCell(Start.x, Start.y, MarkType.None);

        Start = new Node(x, y);
        MarkCell(x, y, MarkType.Start);

        ClearWayPoints();
        GenerateWayPoints();
    }

    internal void SetEnd(int x, int y)
    {
        if (_maze[x, y] == 1)
            return;

        if (End != null)
            MarkCell(End.x, End.y, MarkType.None);

        End = new Node(x, y);
        MarkCell(x, y, MarkType.End);

        ClearWayPoints();
        GenerateWayPoints();

        Debug.Log("end linked to: " + string.Join(" - ", WayPointsGrid[End]));
    }

    internal void EditCell(int x, int y)
    {
        if (IsStartEndPoint(x, y))
            return;

        _maze[x, y] = 1 - _maze[x, y];
        _texture.SetPixel(x, y, _maze[x, y] == 1 ? Color.black : Color.white);
        _texture.Apply();

        ClearWayPoints();
        GenerateWayPoints();
    }

    internal void MarkCell(int x, int y, MarkType markType)
    {
        var color = Color.white;

        switch (markType)
        {
            case MarkType.Path:
                color = Color.blue;
                break;
            case MarkType.Start:
                color = Color.green;
                break;
            case MarkType.End:
                color = Color.red;
                break;
            case MarkType.Open:
                color = Color.red;
                break;
            case MarkType.Closed:
                color = Color.green;
                break;
            case MarkType.WayPoint:
                color = Color.magenta;
                break;
            case MarkType.WayPointPort:
                color = Color.cyan;
                break;
            default:
                color = Color.white;
                break;
        }

        _texture.SetPixel(x, y, color);
        _texture.Apply();
    }

    private bool IsStartEndPoint(int x, int y)
    {
        var n = new Node(x, y);
        return n.Equals(Start) || n.Equals(End);
    }
}
