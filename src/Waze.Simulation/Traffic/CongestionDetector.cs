using Waze.Core.Domain;

namespace Waze.Simulation.Traffic;

public sealed class CongestionDetector
{
    public bool IsCongested(EdgeTrafficState state, RoadEdge edge) => state.VehicleCount >= edge.Capacity;
}
