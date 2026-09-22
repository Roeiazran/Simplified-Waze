using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Waze.Simulation.Traffic;

namespace Waze.Simulation.Routing;

public sealed class RoutingGraphView : IRoutingGraph
{
    private readonly RoadGraph _graph;
    private readonly TrafficState _trafficState;

    public RoutingGraphView(RoadGraph graph, TrafficState trafficState)
    {
        _graph = graph;
        _trafficState = trafficState;
    }

    public IReadOnlyList<RoadEdge> GetOutgoingEdges(NodeId node) => _graph.GetOutgoingEdges(node);

    public RoadEdge GetEdge(EdgeId id) => _graph.GetEdge(id);

    public double GetCost(EdgeId id)
    {
        var edge = _graph.GetEdge(id);
        var state = _trafficState.GetOrCreateState(id);
        var freeFlowTime = edge.Length / edge.SpeedLimit;
        var loadRatio = (double)state.VehicleCount / edge.Capacity;
        return freeFlowTime * (1 + loadRatio);
    }
}
