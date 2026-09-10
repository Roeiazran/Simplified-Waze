using Waze.Core.Domain;

namespace Waze.Simulation.Vehicles;

public sealed class Vehicle
{
    public VehicleId Id { get; }
    public NodeId Source { get; }
    public NodeId Destination { get; }
    public Route? CurrentRoute { get; private set; }
    public VehicleState State { get; private set; }

    public Vehicle(VehicleId id, NodeId source, NodeId destination)
    {
        Id = id;
        Source = source;
        Destination = destination;
        State = VehicleState.WaitingForRoute;
    }

    public void AssignRoute(Route route)
    {
        CurrentRoute = route;
        State = VehicleState.Driving;
    }

    public void MarkArrived()
    {
        State = VehicleState.Arrived;
    }
}
