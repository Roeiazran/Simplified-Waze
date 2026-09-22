using Waze.Core.Domain;

namespace Waze.Core.Routing;

public sealed class RoutingWorkerPool
{
    private readonly IRoutePlanner _routePlanner;
    private readonly int _workerCount;

    public RoutingWorkerPool(IRoutePlanner routePlanner, int workerCount)
    {
        _routePlanner = routePlanner;
        _workerCount = workerCount;
    }

    public IReadOnlyList<Route> FindRoutes(IReadOnlyList<(NodeId Source, NodeId Destination)> requests, IRoutingGraph graph)
    {
        var results = new Route[requests.Count];

        Parallel.For(0, requests.Count, new ParallelOptions { MaxDegreeOfParallelism = _workerCount }, i =>
        {
            var (source, destination) = requests[i];
            results[i] = _routePlanner.FindRoute(source, destination, graph);
        });

        return results;
    }
}
