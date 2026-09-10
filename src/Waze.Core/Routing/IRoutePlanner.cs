using Waze.Core.Domain;
using Waze.Core.Graph;

namespace Waze.Core.Routing;

public interface IRoutePlanner
{
    Route FindRoute(NodeId source, NodeId destination, RoadGraph graph);
}
