
public class Node
{
    private Point _point;

    public int x => _point.x;
    public int y => _point.y;

    public Node Parent { get; private set; }
    public int GCost { get; private set; }
    public int HCost { get; private set; }
    public int FCost => GCost + HCost;

    public Node(int x, int y) => _point = new Point(x, y);
    public Node(Point point) => _point = point;

    public void SetCost(int cost) => GCost = cost;

    public void SetParent(Node parent) => Parent = parent;

    public void Estimate(Node end) => HCost = MathHelper.ManhattanDistance(this, end);

    public void EstimateG(Node start) => GCost = MathHelper.ManhattanDistance(this, start);

    public int GetEstimationValue(Node n) => MathHelper.ManhattanDistance(this, n);

    public override bool Equals(object obj)
    {
        return obj is Node && ((Node)obj)._point.Equals(_point);
    }

    public override int GetHashCode()
    {
        return _point.GetHashCode();
    }

    public override string ToString()
    {
        return _point.ToString();
    }
}
