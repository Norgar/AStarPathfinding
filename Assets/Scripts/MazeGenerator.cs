using System;
using UnityEngine;

public class MazeGenerator
{
    private int[,] _maze;

    public int[,] GetMaze => _maze;

    public void Generate(Texture2D mazeMap, float noizeSensitivity = 0.5f)
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

    internal void OnMazeEdit(RestrictionType restrictionType, int x, int y)
    {
        _maze[x, y] = (int)restrictionType;
    }
}
