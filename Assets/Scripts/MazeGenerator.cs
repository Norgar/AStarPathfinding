using System;
using UnityEngine;

public class MazeGenerator
{
    private int[,] _maze;
    private Texture2D _texture;

    public int[,] GetMaze => _maze;

    public Node Start { get; private set; }
    public Node End { get; private set; }

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
        SetFinish(size - 1, size - 1);

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

    internal void SetFinish(int x, int y)
    {
        if (_maze[x, y] == 1)
            return;

        if (End != null)
            MarkCell(End.X, End.Y, MarkType.None);

        End = new Node(x, y);
        MarkCell(x, y, MarkType.Finish);
    }

    internal void EditCell(int x, int y)
    {
        if (IsStartEndPoint(x, y))
            return;

        _maze[x, y] = 1 - _maze[x, y];
        _texture.SetPixel(x, y, _maze[x, y] == 1 ? Color.black : Color.white);
        _texture.Apply();
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
            case MarkType.Finish:
                color = Color.red;
                break;
            case MarkType.Open:
                color = Color.red;
                break;
            case MarkType.Closed:
                color = Color.green;
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
