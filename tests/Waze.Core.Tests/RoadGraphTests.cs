using Waze.Core.Domain;
using Waze.Core.Graph;
using Xunit;

public class RoadGraphTests
{
    [Fact]
    public void GetOutgoingEdges_ReturnsOnlyEdgesFromThatNode()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1);
        var b = new NodeId(2);
        var c = new NodeId(3);

        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));

        var e1 = new RoadEdge(new EdgeId(1), a, b, length: 100, speedLimit: 50, capacity: 10);
        var e2 = new RoadEdge(new EdgeId(2), a, c, length: 200, speedLimit: 50, capacity: 10);
        var e3 = new RoadEdge(new EdgeId(3), b, c, length: 50, speedLimit: 50, capacity: 10);

        graph.AddEdge(e1);
        graph.AddEdge(e2);
        graph.AddEdge(e3);

        var outgoing = graph.GetOutgoingEdges(a);

        Assert.Equal(2, outgoing.Count);
        Assert.Contains(outgoing, e => e.Id == e1.Id);
        Assert.Contains(outgoing, e => e.Id == e2.Id);
    }
}
