using Waze.Core.Domain;
using Waze.Simulation.Traffic;
using Xunit;

public class TrafficStateTests
{
    [Fact]
    public void GetOrCreateState_UnknownEdge_ReturnsDefaultState()
    {
        var traffic = new TrafficState();

        var state = traffic.GetOrCreateState(new EdgeId(1));

        Assert.Equal(0, state.VehicleCount);
        Assert.True(state.IsOpen);
    }

    [Fact]
    public void VehicleEntered_IncrementsVehicleCount()
    {
        var traffic = new TrafficState();
        var edge = new EdgeId(1);

        traffic.VehicleEntered(edge);
        traffic.VehicleEntered(edge);

        Assert.Equal(2, traffic.GetOrCreateState(edge).VehicleCount);
    }

    [Fact]
    public void VehicleLeft_NeverGoesBelowZero()
    {
        var traffic = new TrafficState();
        var edge = new EdgeId(1);

        traffic.VehicleLeft(edge);

        Assert.Equal(0, traffic.GetOrCreateState(edge).VehicleCount);
    }
}
