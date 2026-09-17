using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;
using Waze.Simulation.Vehicles;

namespace Waze.Simulation.Rerouting;

public sealed class ReroutingManager
{
    private readonly IRoutePlanner _routePlanner;
    private readonly double _minimumImprovementRatio;
    public ReroutingManager(IRoutePlanner routePlanner, double minimumImprovementRatio = 0.1)
    {
        _routePlanner = routePlanner;
        _minimumImprovementRatio = minimumImprovementRatio;
    }

    public (int Attempts, int Reroutes) HandleCongestedEdge(EdgeId congestedEdge, IReadOnlyList<Vehicle> vehicles, RoadGraph graph)
    {
        var rerouted = 0;
        var attempted = 0;

        foreach (var vehicle in vehicles)
        {
            if (!IsAffected(vehicle, congestedEdge))
                continue;
            
            attempted++;
            if (TryReroute(vehicle, graph))
                rerouted++;
        }
        return (attempted, rerouted);
    }

    private static bool IsAffected(Vehicle vehicle, EdgeId changedEdge)
    {
        if (vehicle.State != VehicleState.Driving || vehicle.CurrentRoute is null)
            return false;

        return vehicle.CurrentRoute.Edges.Skip(vehicle.RouteIndex + 1).Contains(changedEdge);
    }

    private bool TryReroute(Vehicle vehicle, RoadGraph graph)
    {
        var currentEdgeId = vehicle.CurrentEdge;
        var currentEdge = graph.GetEdge(currentEdgeId);
        var remainingOnCurrentEdge = currentEdge.Length - vehicle.PositionOnEdge;

        var oldContinuationCost = vehicle.CurrentRoute!.Edges.Skip(vehicle.RouteIndex + 1)
            .Sum(edgeId => graph.GetEdge(edgeId).Length);

        var candidateTail = _routePlanner.FindRoute(currentEdge.To, vehicle.Destination, graph);

        if (candidateTail.TotalCost < oldContinuationCost * (1 - _minimumImprovementRatio))
        {
            var splicedEdges = new List<EdgeId> { currentEdgeId };
            splicedEdges.AddRange(candidateTail.Edges);

            var totalCost = remainingOnCurrentEdge + candidateTail.TotalCost;
            var splicedRoute = new Route(currentEdge.From, vehicle.Destination, splicedEdges, totalCost);
            vehicle.Reroute(splicedRoute);
            return true;
        }
        return false;
    }
}