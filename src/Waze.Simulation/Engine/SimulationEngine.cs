using Waze.Core.Graph;
using Waze.Simulation.Rerouting;
using Waze.Simulation.Traffic;
using Waze.Simulation.Vehicles;

namespace Waze.Simulation.Engine;

public sealed class SimulationEngine
{
    private readonly RoadGraph _graph;
    private readonly VehicleManager _vehicleManager;
    private readonly TrafficManager _trafficManager;
    private readonly ReroutingManager _reroutingManager;

    public int CurrentTick { get; private set; }
    public int TotalCongestionEvents { get; private set; }
    public int TotalReroutes { get; private set; }
    public int TotalRerouteAttempts { get; private set; }
    public SimulationEngine(RoadGraph graph, VehicleManager vehicleManager, TrafficManager trafficManager, ReroutingManager reroutingManager)
    {
        _graph = graph;
        _vehicleManager = vehicleManager;
        _trafficManager = trafficManager;
        _reroutingManager = reroutingManager;
    }

    public void Tick()
    {
        var enteredEdges = _vehicleManager.AdvanceAll(_graph, _trafficManager.State);
        var congestedEdges = _trafficManager.DetectCongestedEdges(enteredEdges, _graph);

        TotalCongestionEvents += congestedEdges.Count;

        foreach (var edgeId in congestedEdges)
        {
            var (attempts, reroutes) = _reroutingManager.HandleCongestedEdge(edgeId, _vehicleManager.Vehicles, _graph);
            TotalRerouteAttempts += attempts;
            TotalReroutes += reroutes;
        }

        CurrentTick++;
    }
}
