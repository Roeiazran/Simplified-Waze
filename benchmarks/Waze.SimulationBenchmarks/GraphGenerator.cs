using Waze.Core.Domain;
using Waze.Core.Graph;

namespace Waze.SimulationBenchmarks;

public static class GraphGenerator
{
    public static RoadGraph BuildGridGraph(int size)
    {
        var graph = new RoadGraph();
        var nodes = new NodeId[size, size];

        for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                nodes[x, y] = new NodeId(y * size + x);
                graph.AddNode(new RoadNode(nodes[x, y]));
            }

        var edgeId = 0;
        for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                if (x + 1 < size)
                {
                    graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x, y], nodes[x + 1, y], length: 50, speedLimit: 10, capacity: 3));
                    graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x + 1, y], nodes[x, y], length: 50, speedLimit: 10, capacity: 3));
                }
                if (y + 1 < size)
                {
                    graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x, y], nodes[x, y + 1], length: 50, speedLimit: 10, capacity: 3));
                    graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x, y + 1], nodes[x, y], length: 50, speedLimit: 10, capacity: 3));
                }
            }

        return graph;
    }

    public static List<(NodeId, NodeId)> GenerateRandomRequests(int gridSize, int count, int seed)
    {
        var random = new Random(seed);
        var maxNode = gridSize * gridSize - 1;
        var requests = new List<(NodeId, NodeId)>();

        for (var i = 0; i < count; i++)
            requests.Add((new NodeId(random.Next(0, maxNode + 1)), new NodeId(random.Next(0, maxNode + 1))));

        return requests;
    }
}
