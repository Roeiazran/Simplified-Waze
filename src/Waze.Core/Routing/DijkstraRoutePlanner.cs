using Waze.Core.Domain;
using Waze.Core.Graph;

namespace Waze.Core.Routing;

public sealed class DijkstraRoutePlanner : IRoutePlanner
{
    public Route FindRoute(NodeId source, NodeId destination, RoadGraph graph)
    {
        var distances = new Dictionary<NodeId, double> { [source] = 0 };
        var predecessorEdge = new Dictionary<NodeId, EdgeId>();
        var visited = new HashSet<NodeId>();
        var queue = new PriorityQueue<NodeId, double>();
        queue.Enqueue(source, 0);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
                continue;

            if (current.Equals(destination))
                break;

            foreach (var edge in graph.GetOutgoingEdges(current))
            {
                var newDistance = distances[current] + edge.Length;
                if (!distances.TryGetValue(edge.To, out var existing) || newDistance < existing)
                {
                    distances[edge.To] = newDistance;
                    predecessorEdge[edge.To] = edge.Id;
                    queue.Enqueue(edge.To, newDistance);
                }
            }
        }

        if (!distances.ContainsKey(destination))
            throw new InvalidOperationException($"No route found from {source} to {destination}.");

        var edges = new List<EdgeId>();
        var node = destination;
        while (!node.Equals(source))
        {
            var edgeId = predecessorEdge[node];
            edges.Add(edgeId);
            node = graph.GetEdge(edgeId).From;
        }
        edges.Reverse();

        return new Route(source, destination, edges, distances[destination]);
    }
}
