using Waze.Core.Domain;

namespace Waze.Simulation.Traffic;

public sealed class TrafficState
{
    private readonly Dictionary<EdgeId, EdgeTrafficState> _edgeStates = new();

    public EdgeTrafficState GetOrCreateState(EdgeId edgeId)
    {
        if (!_edgeStates.TryGetValue(edgeId, out var state))
        {
            state = new EdgeTrafficState();
            _edgeStates[edgeId] = state;
        }

        return state;
    }

    public void VehicleEntered(EdgeId edgeId) => GetOrCreateState(edgeId).IncrementVehicleCount();

    public void VehicleLeft(EdgeId edgeId) => GetOrCreateState(edgeId).DecrementVehicleCount();
}
