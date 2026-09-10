using Waze.Core.Domain;
using Waze.Simulation.Vehicles;
using Xunit;

public class VehicleTests
{
    [Fact]
    public void NewVehicle_StartsInWaitingForRouteState()
    {
        var vehicle = new Vehicle(new VehicleId(1), new NodeId(1), new NodeId(2));

        Assert.Equal(VehicleState.WaitingForRoute, vehicle.State);
        Assert.Null(vehicle.CurrentRoute);
    }

    [Fact]
    public void AssignRoute_TransitionsToDriving()
    {
        var vehicle = new Vehicle(new VehicleId(1), new NodeId(1), new NodeId(2));
        var route = new Route(new NodeId(1), new NodeId(2), new List<EdgeId>(), totalCost: 5);

        vehicle.AssignRoute(route);

        Assert.Equal(VehicleState.Driving, vehicle.State);
        Assert.Equal(route, vehicle.CurrentRoute);
    }
}
