using Waze.Core.Domain;

namespace Waze.Core.Routing;

public sealed class CachingRoutePlanner : IRoutePlanner
{
    private readonly IRoutePlanner _inner;
    private readonly RouteCache _cache;

    public int Hits { get; private set; }
    public int Misses { get; private set; }

    public CachingRoutePlanner(IRoutePlanner inner, RouteCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Route FindRoute(NodeId source, NodeId destination, IRoutingGraph graph)
    {
        if (_cache.TryGet(source, destination, out var cached))
        {
            Hits++;
            return cached;
        }

        Misses++;
        var route = _inner.FindRoute(source, destination, graph);
        _cache.Store(source, destination, route);
        return route;
    }
}
