using Waze.Core.Domain;
using Waze.Simulation.Traffic;
using Xunit;

public class CongestionDetectorTests
{
    [Fact]
    public void IsCongested_BelowCapacity_ReturnsFalse()
    {
        var edge = new RoadEdge(new EdgeId(1), new NodeId(1), new NodeId(2), length: 10, speedLimit: 50, capacity: 5);
        var state = new EdgeTrafficState();
        state.IncrementVehicleCount();

        Assert.False(new CongestionDetector().IsCongested(state, edge));
    }

    [Fact]
    public void IsCongested_AtCapacity_ReturnsTrue()
    {
        var edge = new RoadEdge(new EdgeId(1), new NodeId(1), new NodeId(2), length: 10, speedLimit: 50, capacity: 2);
        var state = new EdgeTrafficState();
        state.IncrementVehicleCount();
        state.IncrementVehicleCount();

        Assert.True(new CongestionDetector().IsCongested(state, edge));
    }
}
