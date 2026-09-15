using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Waze.Simulation.Traffic;

namespace Waze.Simulation.Vehicles;

public sealed class VehicleManager
{
    private readonly List<Vehicle> _vehicles = new();

    public IReadOnlyList<Vehicle> Vehicles => _vehicles;

    public void Add(Vehicle vehicle) => _vehicles.Add(vehicle);

    public IReadOnlyList<EdgeId> AdvanceAll(RoadGraph graph, TrafficState trafficState)
    {
        var enteredEdges = new List<EdgeId>();

        foreach (var vehicle in _vehicles)
        {
            var entered = AdvanceVehicle(vehicle, graph, trafficState);
            if (entered.HasValue)
                enteredEdges.Add(entered.Value);
        }

        return enteredEdges;
    }

    private static EdgeId? AdvanceVehicle(Vehicle vehicle, RoadGraph graph, TrafficState trafficState)
    {
        if (vehicle.State != VehicleState.Driving)
            return null;

        var currentEdgeId = vehicle.CurrentEdge;
        var edge = graph.GetEdge(currentEdgeId);
        vehicle.Advance(edge.SpeedLimit);

        if (vehicle.PositionOnEdge < edge.Length)
            return null;

        trafficState.VehicleLeft(currentEdgeId);

        if (vehicle.RouteIndex + 1 >= vehicle.CurrentRoute!.Edges.Count)
        {
            vehicle.MarkArrived();
            return null;
        }

        vehicle.MoveToNextEdge();
        var enteredEdgeId = vehicle.CurrentEdge;
        trafficState.VehicleEntered(enteredEdgeId);
        return enteredEdgeId;
    }

    public void CreateVehicle(VehicleId id, NodeId source, NodeId destination, IRoutePlanner routePlanner, RoadGraph graph, TrafficState trafficState)
    {
        var vehicle = new Vehicle(id, source, destination);
        var route = routePlanner.FindRoute(source, destination, graph);

        if (route.Edges.Count == 0)
        {
            vehicle.MarkArrived();
        }
        else
        {
            vehicle.AssignRoute(route);
            trafficState.VehicleEntered(route.Edges[0]);
        }

        Add(vehicle);
    }
}
