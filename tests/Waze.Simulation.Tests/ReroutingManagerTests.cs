using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Waze.Simulation.Rerouting;
using Waze.Simulation.Vehicles;
using Xunit;

public class ReroutingManagerTests
{
    [Fact]
    public void HandleCongestedEdge_SetsRouteSourceToCurrentEdgeStart_NotOriginalSource()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1); var b = new NodeId(2); var c = new NodeId(3);
        var d = new NodeId(4); var e = new NodeId(5);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));
        graph.AddNode(new RoadNode(d));
        graph.AddNode(new RoadNode(e));

        var ab = new EdgeId(1); var bc = new EdgeId(2); var cd = new EdgeId(3);
        var ce = new EdgeId(4); var ed = new EdgeId(5);
        graph.AddEdge(new RoadEdge(ab, a, b, length: 5, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(bc, b, c, length: 5, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(cd, c, d, length: 100, speedLimit: 50, capacity: 10)); // becomes congested
        graph.AddEdge(new RoadEdge(ce, c, e, length: 1, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(ed, e, d, length: 1, speedLimit: 50, capacity: 10));    // bypass via e

        var vehicle = new Vehicle(new VehicleId(1), a, d);
        vehicle.AssignRoute(new Route(a, d, new List<EdgeId> { ab, bc, cd }, totalCost: 110));
        vehicle.MoveToNextEdge(); // simulate having already finished `ab`; now on `bc`, RouteIndex = 1

        var manager = new ReroutingManager(new DijkstraRoutePlanner());
        manager.HandleCongestedEdge(cd, new List<Vehicle> { vehicle }, graph);

        Assert.Equal(b, vehicle.CurrentRoute!.Source); // starts where the current edge starts, not at `a`
    }

    
    [Fact]
    public void HandleCongestedEdge_SplicesCurrentEdgeOntoNewRoute()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1); var b = new NodeId(2); var c = new NodeId(3); var d = new NodeId(4);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));
        graph.AddNode(new RoadNode(d));

        var ab = new EdgeId(1);
        var bc = new EdgeId(2);
        var cd = new EdgeId(3);
        var bd = new EdgeId(4);
        graph.AddEdge(new RoadEdge(ab, a, b, length: 5, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(bc, b, c, length: 100, speedLimit: 50, capacity: 10)); // becomes congested
        graph.AddEdge(new RoadEdge(cd, c, d, length: 1, speedLimit: 50, capacity: 10));
        graph.AddEdge(new RoadEdge(bd, b, d, length: 2, speedLimit: 50, capacity: 10));   // cheap bypass

        var vehicle = new Vehicle(new VehicleId(1), a, d);
        vehicle.AssignRoute(new Route(a, d, new List<EdgeId> { ab, bc, cd }, totalCost: 106));

        var manager = new ReroutingManager(new DijkstraRoutePlanner());
        manager.HandleCongestedEdge(bc, new List<Vehicle> { vehicle }, graph);

        Assert.Equal(ab, vehicle.CurrentEdge);
        Assert.Equal(new List<EdgeId> { ab, bd }, vehicle.CurrentRoute!.Edges);
    }

    [Fact]
    public void HandleCongestedEdge_DoesNotRerouteForUnaffectedVehicle()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1); var b = new NodeId(2);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        var edge = new EdgeId(1);
        graph.AddEdge(new RoadEdge(edge, a, b, length: 10, speedLimit: 50, capacity: 10));

        var vehicle = new Vehicle(new VehicleId(1), a, b);
        vehicle.AssignRoute(new Route(a, b, new List<EdgeId> { edge }, totalCost: 10));

        var manager = new ReroutingManager(new DijkstraRoutePlanner());
        manager.HandleCongestedEdge(new EdgeId(999), new List<Vehicle> { vehicle }, graph);

        Assert.Equal(edge, vehicle.CurrentRoute!.Edges[0]);
    }
}
