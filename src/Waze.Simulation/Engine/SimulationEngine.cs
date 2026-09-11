using Waze.Core.Graph;
using Waze.Simulation.Vehicles;

namespace Waze.Simulation.Engine;

public sealed class SimulationEngine
{
    private readonly RoadGraph _graph;
    private readonly VehicleManager _vehicleManager;

    public int CurrentTick { get; private set; }

    public SimulationEngine(RoadGraph graph, VehicleManager vehicleManager)
    {
        _graph = graph;
        _vehicleManager = vehicleManager;
    }

    public void Tick()
    {
        _vehicleManager.AdvanceAll(_graph);
        CurrentTick++;
    }
}
