using System.Diagnostics;
using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;

const int gridSize = 50;
const int requestCount = 2000;
const int randomSeed = 481516;

var graph = BuildGridGraph(gridSize);
var requests = GenerateRandomRequests(gridSize, requestCount, randomSeed);
var planner = new DijkstraRoutePlanner();

Console.WriteLine($"Graph: {gridSize * gridSize} nodes, {requestCount} requests, seed {randomSeed}");
Console.WriteLine($"Processor count: {Environment.ProcessorCount}");
Console.WriteLine();

var sw = Stopwatch.StartNew();
foreach (var workerCount in new[] { 1, 2, 4, 8, 16 })
{
    var pool = new RoutingWorkerPool(planner, workerCount);
    sw.Restart();
    pool.FindRoutes(requests, graph);
    sw.Stop();
    Console.WriteLine($"Parallel (workers={workerCount}): {sw.ElapsedMilliseconds,6}ms  ({requestCount / sw.Elapsed.TotalSeconds:F0} routes/sec)");
}

static RoadGraph BuildGridGraph(int size)
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
                graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x, y], nodes[x + 1, y], length: 1, speedLimit: 50, capacity: 10));
                graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x + 1, y], nodes[x, y], length: 1, speedLimit: 50, capacity: 10));
            }
            if (y + 1 < size)
            {
                graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x, y], nodes[x, y + 1], length: 1, speedLimit: 50, capacity: 10));
                graph.AddEdge(new RoadEdge(new EdgeId(edgeId++), nodes[x, y + 1], nodes[x, y], length: 1, speedLimit: 50, capacity: 10));
            }
        }

    return graph;
}

static List<(NodeId, NodeId)> GenerateRandomRequests(int gridSize, int count, int seed)
{
    var random = new Random(seed);
    var maxNode = gridSize * gridSize - 1;
    var requests = new List<(NodeId, NodeId)>();

    for (var i = 0; i < count; i++)
        requests.Add((new NodeId(random.Next(0, maxNode + 1)), new NodeId(random.Next(0, maxNode + 1))));

    return requests;
}
