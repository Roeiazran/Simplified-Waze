using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;

public sealed class SimpleRoutingGraph : IRoutingGraph
{
    private readonly RoadGraph _graph;

    public SimpleRoutingGraph(RoadGraph graph) => _graph = graph;

    public IReadOnlyList<RoadEdge> GetOutgoingEdges(NodeId node) => _graph.GetOutgoingEdges(node);

    public RoadEdge GetEdge(EdgeId id) => _graph.GetEdge(id);

    public double GetCost(EdgeId id) => GetEdge(id).Length;
}
