using Waze.Core.Domain;
using Waze.Core.Graph;

namespace Waze.Simulation.Traffic;

public sealed class TrafficManager
{
    private readonly CongestionDetector _congestionDetector;

    public TrafficManager(TrafficState state, CongestionDetector congestionDetector)
    {
        State = state;
        _congestionDetector = congestionDetector;
    }

    public TrafficState State { get; }

    public IReadOnlyList<EdgeId> DetectCongestedEdges(IReadOnlyList<EdgeId> recentlyEnteredEdges, RoadGraph graph)
    {
        var congested = new List<EdgeId>();

        foreach (var edgeId in recentlyEnteredEdges)
        {
            var edge = graph.GetEdge(edgeId);
            var state = State.GetOrCreateState(edgeId);

            if (_congestionDetector.IsCongested(state, edge))
                congested.Add(edgeId);
        }

        return congested;
    }
}
