using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathHelper
{
    public static bool IsLinesIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
    {
        float a = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
        float b = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

        return a >= 0 && a <= 1 && b >= 0 && b <= 1;
    }

    public static bool IsRectIntersect(float x1, float y1, float x2, float y2, float rx, float ry, float rw, float rh)
    {
        var left = IsLinesIntersect(x1, y1, x2, y2, rx, ry, rx, ry + rh);
        var right = IsLinesIntersect(x1, y1, x2, y2, rx + rw, ry, rx + rw, ry + rh);
        var top = IsLinesIntersect(x1, y1, x2, y2, rx, ry, rx + rw, ry);
        var bottom = IsLinesIntersect(x1, y1, x2, y2, rx, ry + rh, rx + rw, ry + rh);

        return left || right || top || bottom;
    }
}
