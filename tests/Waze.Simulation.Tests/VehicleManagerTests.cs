using Waze.Core.Domain;
using Waze.Simulation.Vehicles;
using Waze.Core.Graph;
using Waze.Simulation.Traffic;
using Waze.Core.Routing;
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
        graph.AddEdge(new RoadEdge(new EdgeId(1), a, b, length: 10, speedLimit: 5, capacity: 10));

        var vehicle = new Vehicle(new VehicleId(1), a, b);
        vehicle.AssignRoute(new Route(a, b, new List<EdgeId> { new(1) }, totalCost: 10));

        var manager = new VehicleManager();
        manager.Add(vehicle);

        manager.AdvanceAll(graph, new TrafficState()); // position 5, still Driving
        Assert.Equal(VehicleState.Driving, vehicle.State);

        manager.AdvanceAll(graph, new TrafficState()); // position 10 >= length 10 -> Arrived
        Assert.Equal(VehicleState.Arrived, vehicle.State);
    }

    [Fact]
    public void CreateVehicle_AssignsComputedRouteAndRegistersFirstEdgeInTraffic()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1); var b = new NodeId(2); var c = new NodeId(3);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));
        var ab = new EdgeId(1); var bc = new EdgeId(2); var ac = new EdgeId(3);
        graph.AddEdge(new RoadEdge(ab, a, b, length: 3, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(bc, b, c, length: 4, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(ac, a, c, length: 100, speedLimit: 50, capacity: 10)); // expensive direct

        var trafficState = new TrafficState();
        var manager = new VehicleManager();

        manager.CreateVehicle(new VehicleId(1), a, c, new DijkstraRoutePlanner(), graph, trafficState);

        var vehicle = manager.Vehicles[0];
        Assert.Equal(VehicleState.Driving, vehicle.State);
        Assert.Equal(new List<EdgeId> { ab, bc }, vehicle.CurrentRoute!.Edges); // Dijkstra correctly avoids the expensive direct edge
        Assert.Equal(1, trafficState.GetOrCreateState(ab).VehicleCount);
    }

    [Fact]
    public void CreateVehicle_SourceEqualsDestination_MarksArrivedImmediately()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1);
        graph.AddNode(new RoadNode(a));

        var manager = new VehicleManager();
        manager.CreateVehicle(new VehicleId(1), a, a, new DijkstraRoutePlanner(), graph, new TrafficState());

        Assert.Equal(VehicleState.Arrived, manager.Vehicles[0].State);
    }

}
