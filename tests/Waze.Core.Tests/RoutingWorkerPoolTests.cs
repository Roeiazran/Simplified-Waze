using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Xunit;

public class RoutingWorkerPoolTests
{
    [Fact]
    public void FindRoutes_ProducesCorrectResultForEachIndependentRequest()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1); var b = new NodeId(2); var c = new NodeId(3);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));
        graph.AddEdge(new RoadEdge(new EdgeId(1), a, b, length: 3, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(new EdgeId(2), b, c, length: 4, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(new EdgeId(3), a, c, length: 100, speedLimit: 50, capacity: 10));

        var pool = new RoutingWorkerPool(new DijkstraRoutePlanner(), workerCount: 4);
        var requests = new List<(NodeId, NodeId)> { (a, b), (a, c), (b, c) };

        var results = pool.FindRoutes(requests, graph);

        Assert.Equal(3, results[0].TotalCost); // a->b direct
        Assert.Equal(7, results[1].TotalCost);  // a->b->c cheaper than direct a->c
        Assert.Equal(4, results[2].TotalCost); // b->c direct
    }
}
