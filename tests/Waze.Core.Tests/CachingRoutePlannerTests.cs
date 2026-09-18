using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Xunit;

public class CachingRoutePlannerTests
{
    private sealed class CountingRoutePlanner : IRoutePlanner
    {
        private readonly IRoutePlanner _inner;
        public int CallCount { get; private set; }

        public CountingRoutePlanner(IRoutePlanner inner) => _inner = inner;

        public Route FindRoute(NodeId source, NodeId destination, RoadGraph graph)
        {
            CallCount++;
            return _inner.FindRoute(source, destination, graph);
        }
    }

    [Fact]
    public void FindRoute_SecondIdenticalRequest_DoesNotRecomputeUnderlyingPlanner()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1); var b = new NodeId(2);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddEdge(new RoadEdge(new EdgeId(1), a, b, length: 10, speedLimit: 50, capacity: 10));

        var counting = new CountingRoutePlanner(new DijkstraRoutePlanner());
        var caching = new CachingRoutePlanner(counting, new RouteCache());

        var first = caching.FindRoute(a, b, graph);
        var second = caching.FindRoute(a, b, graph);

        Assert.Equal(1, counting.CallCount);
        Assert.Equal(first.TotalCost, second.TotalCost);
    }
}
