using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Xunit;

public class DijkstraRoutePlannerTests
{
    [Fact]
    public void FindRoute_PrefersCheaperMultiHopPathOverExpensiveDirectEdge()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1);
        var b = new NodeId(2);
        var c = new NodeId(3);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));

        graph.AddEdge(new RoadEdge(new EdgeId(1), a, c, length: 10, speedLimit: 50, capacity: 10)); // direct, expensive
        graph.AddEdge(new RoadEdge(new EdgeId(2), a, b, length: 3, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(new EdgeId(3), b, c, length: 3, speedLimit: 50, capacity: 10));  // A->B->C cheaper

        var planner = new DijkstraRoutePlanner();
        var route = planner.FindRoute(a, c, graph);

        Assert.Equal(6, route.TotalCost);
        Assert.Equal(2, route.Edges.Count);
    }
}
