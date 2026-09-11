using Waze.Core.Graph;

namespace Waze.Simulation.Vehicles;

public sealed class VehicleManager
{
    private readonly List<Vehicle> _vehicles = new();

    public IReadOnlyList<Vehicle> Vehicles => _vehicles;

    public void Add(Vehicle vehicle) => _vehicles.Add(vehicle);

    public void AdvanceAll(RoadGraph graph)
    {
        foreach (var vehicle in _vehicles)
            AdvanceVehicle(vehicle, graph);
    }

    private static void AdvanceVehicle(Vehicle vehicle, RoadGraph graph)
    {
        if (vehicle.State != VehicleState.Driving)
            return;

        var edge = graph.GetEdge(vehicle.CurrentEdge);
        vehicle.Advance(edge.SpeedLimit);

        if (vehicle.PositionOnEdge >= edge.Length)
        {
            if (vehicle.RouteIndex + 1 >= vehicle.CurrentRoute!.Edges.Count)
                vehicle.MarkArrived();
            else
                vehicle.MoveToNextEdge();
        }
    }
}
