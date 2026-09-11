using Waze.Core.Graph;
using Waze.Simulation.Engine;
using Waze.Simulation.Vehicles;
using Xunit;

public class SimulationEngineTests
{
    [Fact]
    public void Tick_IncrementsCurrentTick()
    {
        var graph = new RoadGraph();
        var engine = new SimulationEngine(graph, new VehicleManager());

        engine.Tick();

        Assert.Equal(1, engine.CurrentTick);
    }
}
