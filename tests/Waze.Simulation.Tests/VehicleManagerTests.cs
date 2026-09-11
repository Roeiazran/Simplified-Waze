using Waze.Core.Domain;
using Waze.Simulation.Vehicles;
using Waze.Core.Graph;
using Xunit;

public class VehicleManagerTests
{
    [Fact]
    public void AdvanceAll_MovesVehicleAndMarksArrivedAtRouteEnd()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1);
        var b = new NodeId(2);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddEdge(new RoadEdge(new EdgeId(1), a, b, length: 10, speedLimit: 5));

        var vehicle = new Vehicle(new VehicleId(1), a, b);
        vehicle.AssignRoute(new Route(a, b, new List<EdgeId> { new(1) }, totalCost: 10));

        var manager = new VehicleManager();
        manager.Add(vehicle);

        manager.AdvanceAll(graph); // position 5, still Driving
        Assert.Equal(VehicleState.Driving, vehicle.State);

        manager.AdvanceAll(graph); // position 10 >= length 10 -> Arrived
        Assert.Equal(VehicleState.Arrived, vehicle.State);
    }
}
