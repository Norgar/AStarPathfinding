
public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        var p = (Point)obj;
        return p.x == x && p.y == y;
    }

    public override int GetHashCode()
    {
        int tmp = y + (x + 1) / 2;
        return x + (tmp * tmp);
    }

    public override string ToString()
    {
        return x + ":" + y;
    }
}
