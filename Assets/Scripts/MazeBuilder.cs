using System;
using UnityEngine;
using System.Collections.Generic;

public class MazeBuilder
{
    public List<Node> Path;
    public Dictionary<int, List<Node>> Open;
    public Dictionary<int, List<Node>> Closed;
    public Vector2 Start { get; private set; }
    public Vector2 Finish { get; private set; }

    private Action<Vector2, MarkType, Node> UpdateMark;
    public Action<RestrictionType, int, int> MazeEdit;


    public void Build(int[,] maze)
    {
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(0); y++)
            {

            }
        }
    }

    private void OnMark(MarkType markType, int x, int y)
    {
        switch (markType)
        {
            case MarkType.None:
                break;
            case MarkType.Path:
                break;
            case MarkType.Start:
                Start = new Vector2(x, y);
                break;
            case MarkType.Finish:
                Finish = new Vector2(x, y);
                break;
            default:
                break;
        }

        UpdateMark(new Vector2(x, y), markType, null);
    }

    private void OnEdit(RestrictionType restrictionType, int x, int y)
    {
        MazeEdit?.Invoke(restrictionType, x, y);
    }

    public void ShowPath()
    {
        foreach (var item in Path)
            UpdateMark(new Vector2(item.X, item.Y), MarkType.Path, item);
    }

    public void ShowOpen()
    {
        foreach (var pass in Open)
            foreach (var item in pass.Value)
                UpdateMark(new Vector2(item.X, item.Y), MarkType.Open, item);
    }

    public void ShowClosed()
    {
        foreach (var pass in Closed)
            foreach (var item in pass.Value)
                UpdateMark(new Vector2(item.X, item.Y), MarkType.Closed, item);
    }

    public void MarkPoint(Vector2 v2, MarkType markType, Node node)
    {
        UpdateMark(v2, markType, node);
    }

    public void Reset(int[,] maze, Transform mazeRoot)
    {
        if (mazeRoot.childCount > 0)
        {
            var index = 0;

            for (int x = 0; x < maze.GetLength(0); x++)
                for (int y = 0; y < maze.GetLength(0); y++)
                { }
        }


    }
}
