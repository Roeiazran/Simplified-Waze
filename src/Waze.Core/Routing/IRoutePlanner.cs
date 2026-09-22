using Waze.Core.Domain;

namespace Waze.Core.Routing;

public interface IRoutePlanner
{
    Route FindRoute(NodeId source, NodeId destination, IRoutingGraph graph);
}
