using Waze.Core.Domain;

namespace Waze.Core.Routing;

public sealed class RouteCache
{
    private readonly Dictionary<(NodeId Source, NodeId Destination), Route> _cache = new();

    public bool TryGet(NodeId source, NodeId destination, out Route route) =>
        _cache.TryGetValue((source, destination), out route!);

    public void Store(NodeId source, NodeId destination, Route route)
    {
        _cache[(source, destination)] = route;
    }
}
