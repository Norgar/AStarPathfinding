using System.Collections.Generic;

public class ResultDataCollector
{
    public int Passes;
    public string Result;
    public List<Node> Path;
    public Dictionary<int, List<Node>> Open;
    public Dictionary<int, List<Node>> Closed;
}
