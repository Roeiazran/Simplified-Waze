using Waze.Core.Domain;

namespace Waze.Core.Routing;

public interface IRoutingGraph
{
    IReadOnlyList<RoadEdge> GetOutgoingEdges(NodeId node);
    RoadEdge GetEdge(EdgeId id);
    double GetCost(EdgeId id);
}
