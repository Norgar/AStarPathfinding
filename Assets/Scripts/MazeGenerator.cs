using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{
    private int[,] _maze;
    private Texture2D _texture;

    public int[,] GetMaze => _maze;

    public Dictionary<Node, List<Node>> WayPointsGrid { get; private set; }
    public List<Node> WayPoints { get; private set; }
    public Node Start { get; private set; }
    public Node End { get; private set; }

    private bool IsObstacle(int x, int y) =>
        x < 0 ||
        y < 0 ||
        x >= _maze.GetLength(0) ||
        y >= _maze.GetLength(0) ||
        _maze[x, y] == 1;

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
        var size = _maze.GetLength(0);
        var wplist = new List<int[]>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {

                if (_maze[x, y] == 0)
                {
                    // maze corners
                    //if ((x == 0 || x == size - 1) && (y == 0 || y == size - 1))
                    //    wplist.Add(new int[] { x, y });

                    continue;
                }

                // inner corners
                //if (!IsObstacle(x + 1, y + 1) && IsObstacle(x, y + 1) && IsObstacle(x + 1, y))
                //    wplist.Add(new int[] { x + 1, y + 1 });

                //if (!IsObstacle(x + 1, y - 1) && IsObstacle(x, y - 1) && IsObstacle(x + 1, y))
                //    wplist.Add(new int[] { x + 1, y - 1 });

                //if (!IsObstacle(x - 1, y + 1) && IsObstacle(x, y + 1) && IsObstacle(x - 1, y))
                //    wplist.Add(new int[] { x - 1, y + 1 });

                //if (!IsObstacle(x - 1, y - 1) && IsObstacle(x, y - 1) && IsObstacle(x - 1, y))
                //    wplist.Add(new int[] { x - 1, y - 1 });

                // outter corners
                if (!IsObstacle(x + 1, y + 1) && !IsObstacle(x, y + 1) && !IsObstacle(x + 1, y))
                    wplist.Add(new int[] { x + 1, y + 1 });

                if (!IsObstacle(x + 1, y - 1) && !IsObstacle(x, y - 1) && !IsObstacle(x + 1, y))
                    wplist.Add(new int[] { x + 1, y - 1 });

                if (!IsObstacle(x - 1, y + 1) && !IsObstacle(x, y + 1) && !IsObstacle(x - 1, y))
                    wplist.Add(new int[] { x - 1, y + 1 });

                if (!IsObstacle(x - 1, y - 1) && !IsObstacle(x, y - 1) && !IsObstacle(x - 1, y))
                    wplist.Add(new int[] { x - 1, y - 1 });

                //if ((x == 0 || x == size - 1) && y > 0 && IsObstacle(x, y))
                //{
                //    wplist.Add(new int[] { x, y + 1 });
                //    wplist.Add(new int[] { x, y - 1 });
                //}

                //if ((y == 0 || y == size - 1) && x > 0 && IsObstacle(x, y))
                //{
                //    wplist.Add(new int[] { x + 1, y });
                //    wplist.Add(new int[] { x - 1, y });
                //}



            }
        }

        WayPoints = new List<Node>();
        WayPointsGrid = new Dictionary<Node, List<Node>>();

        WayPoints.Clear();
        WayPointsGrid.Clear();


        WayPoints.Add(Start);

        foreach (var item in wplist)
        {
            var wpNode = new Node(item[0], item[1]);
            wpNode.IsTracked = false;
            wpNode.Estimate(End);
            //wpNode.EstimateG(Start);
            WayPoints.Add(wpNode);
            MarkCell(item[0], item[1], MarkType.WayPoint);
        }

        WayPoints.Add(End);

        Debug.Log("wp count: " + wplist.Count);

        var wpstr = string.Empty;

        foreach (var item in WayPoints)
            wpstr += item.X + ":" + item.Y + " - ";

        Debug.Log("wp: " + wpstr);

        foreach (var item in WayPoints)
            WayPointsGrid.Add(item, new List<Node>());

        foreach (var item in WayPointsGrid)
        {
            var node = item.Key;

            foreach (var point in WayPoints)
            {
                if (node.Equals(point))
                    continue;

                if (node.X <= point.X && node.Y <= point.Y)
                {
                    for (int x = node.X; x <= point.X; x++)
                        for (int y = node.Y; y <= point.Y; y++)
                            if (IsObstacle(x, y))
                                goto next;
                }

                if (node.X <= point.X && node.Y >= point.Y)
                {
                    for (int x = node.X; x <= point.X; x++)
                        for (int y = node.Y; y >= point.Y; y--)
                            if (IsObstacle(x, y))
                                goto next;
                }

                if (node.X >= point.X && node.Y <= point.Y)
                {
                    for (int x = node.X; x >= point.X; x--)
                        for (int y = node.Y; y <= point.Y; y++)
                            if (IsObstacle(x, y))
                                goto next;
                }

                if (node.X >= point.X && node.Y >= point.Y)
                {
                    for (int x = node.X; x >= point.X; x--)
                        for (int y = node.Y; y >= point.Y; y--)
                            if (IsObstacle(x, y))
                                goto next;
                }

                item.Value.Add(point);

            next:
                continue;
            }
        }

        //Debug.Log("wp grid count: " + WayPointsGrid.Count);

        //foreach (var item in WayPointsGrid)
        //{
        //    var str = "node " + item.Key.X + ":" + item.Key.Y + " linked to ";
        //    foreach (var point in item.Value)
        //        str += (point.X + ":" + point.Y + " - ");

        //    Debug.Log(str);
        //}
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
                color[index++] = _maze[x, y] == 1 ? Color.black : Color.white;

        _texture.filterMode = FilterMode.Point;
        _texture.SetPixels32(color);
        _texture.Apply();

        SetStart(0, 0);
        SetEnd(size - 1, size - 1);

        return _texture;
    }

    internal void SetStart(int x, int y)
    {
        if (_maze[x, y] == 1)
            return;

        if (Start != null)
            MarkCell(Start.X, Start.Y, MarkType.None);

        Start = new Node(x, y);
        MarkCell(x, y, MarkType.Start);
    }

    internal void SetEnd(int x, int y)
    {
        if (_maze[x, y] == 1)
            return;

        if (End != null)
            MarkCell(End.X, End.Y, MarkType.None);

        End = new Node(x, y);
        MarkCell(x, y, MarkType.End);
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
