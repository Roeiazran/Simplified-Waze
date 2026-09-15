using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Waze.Simulation.Engine;
using Waze.Simulation.Rerouting;
using Waze.Simulation.Traffic;
using Waze.Simulation.Vehicles;
using Xunit;

public class SimulationEngineTests
{
    [Fact]
    public void Tick_IncrementsCurrentTick()
    {
        var graph = new RoadGraph();
        var engine = new SimulationEngine(graph, new VehicleManager(), new TrafficManager(new TrafficState(), new CongestionDetector()), new ReroutingManager(new DijkstraRoutePlanner()));

        engine.Tick();

        Assert.Equal(1, engine.CurrentTick);
    }
    
    [Fact]
    public void Tick_UpdatesTrafficStateAsVehicleCrossesEdges()
    {
        var graph = new RoadGraph();
        var a = new NodeId(1); var b = new NodeId(2); var c = new NodeId(3);
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));
        var e1 = new EdgeId(1); var e2 = new EdgeId(2);
        graph.AddEdge(new RoadEdge(e1, a, b, length: 10, speedLimit: 10, capacity: 10));
        graph.AddEdge(new RoadEdge(e2, b, c, length: 10, speedLimit: 10, capacity: 10));

        var trafficState = new TrafficState();
        trafficState.VehicleEntered(e1); // simulate the vehicle already being registered on its first edge

        var vehicle = new Vehicle(new VehicleId(1), a, c);
        vehicle.AssignRoute(new Route(a, c, new List<EdgeId> { e1, e2 }, totalCost: 20));
        var vehicleManager = new VehicleManager();
        vehicleManager.Add(vehicle);

        var engine = new SimulationEngine(graph, vehicleManager,
            new TrafficManager(trafficState, new CongestionDetector()),
            new ReroutingManager(new DijkstraRoutePlanner()));

        engine.Tick(); // finishes e1, enters e2

        Assert.Equal(0, trafficState.GetOrCreateState(e1).VehicleCount);
        Assert.Equal(1, trafficState.GetOrCreateState(e2).VehicleCount);
    }

    [Fact]
    public void Tick_ReroutesVehicleAwayFromEdgeCongestedByAnotherVehicle()
    {
        var graph = new RoadGraph();
        var x = new NodeId(1); var a = new NodeId(2); var b = new NodeId(3);
        var c = new NodeId(4); var d = new NodeId(5); var e = new NodeId(6);
        graph.AddNode(new RoadNode(x));
        graph.AddNode(new RoadNode(a));
        graph.AddNode(new RoadNode(b));
        graph.AddNode(new RoadNode(c));
        graph.AddNode(new RoadNode(d));
        graph.AddNode(new RoadNode(e));

        var ab = new EdgeId(1); var bc = new EdgeId(2); var cd = new EdgeId(3);
        var xb = new EdgeId(4); var be = new EdgeId(5); var ed = new EdgeId(6);
        graph.AddEdge(new RoadEdge(ab, a, b, length: 20, speedLimit: 10, capacity: 10)); // 2 ticks to cross
        graph.AddEdge(new RoadEdge(bc, b, c, length: 10, speedLimit: 10, capacity: 1));  // congested the instant vehicle2 enters
        graph.AddEdge(new RoadEdge(cd, c, d, length: 100, speedLimit: 10, capacity: 10)); // expensive original plan
        graph.AddEdge(new RoadEdge(xb, x, b, length: 10, speedLimit: 10, capacity: 10)); // vehicle2's feeder edge, 1 tick
        graph.AddEdge(new RoadEdge(be, b, e, length: 1, speedLimit: 10, capacity: 10));
        graph.AddEdge(new RoadEdge(ed, e, d, length: 1, speedLimit: 10, capacity: 10));  // cheap bypass from b

        var vehicle1 = new Vehicle(new VehicleId(1), a, d);
        vehicle1.AssignRoute(new Route(a, d, new List<EdgeId> { ab, bc, cd }, totalCost: 130));

        var vehicle2 = new Vehicle(new VehicleId(2), x, c);
        vehicle2.AssignRoute(new Route(x, c, new List<EdgeId> { xb, bc }, totalCost: 20));

        var vehicleManager = new VehicleManager();
        vehicleManager.Add(vehicle1);
        vehicleManager.Add(vehicle2);

        var engine = new SimulationEngine(graph, vehicleManager,
            new TrafficManager(new TrafficState(), new CongestionDetector()),
            new ReroutingManager(new DijkstraRoutePlanner()));

        engine.Tick(); // vehicle1 stays on ab (needs 2 ticks); vehicle2 finishes xb, enters bc, bc hits capacity 1

        Assert.Equal(ab, vehicle1.CurrentEdge); // vehicle1 hasn't moved off its current edge
        Assert.Equal(new List<EdgeId> { ab, be, ed }, vehicle1.CurrentRoute!.Edges); // rerouted around bc/cd before ever reaching them
        Assert.Equal(new List<EdgeId> { xb, bc }, vehicle2.CurrentRoute!.Edges); // vehicle2 unaffected — bc is its own current edge
    }



}
