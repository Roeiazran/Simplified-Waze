using System.Diagnostics;
using Waze.Core.Routing;
using Waze.Simulation.Routing;
using Waze.Simulation.Traffic;

namespace Waze.SimulationBenchmarks;

public static class RoutingThroughputBenchmark
{
    public static void Run(int gridSize, int requestCount, int seed)
    {
        var graph = GraphGenerator.BuildGridGraph(gridSize);
        var requests = GraphGenerator.GenerateRandomRequests(gridSize, requestCount, seed);
        var planner = new DijkstraRoutePlanner();
        var routingGraph = new RoutingGraphView(graph, new TrafficState());
        Console.WriteLine($"Graph: {gridSize * gridSize} nodes, {requestCount} requests, seed {seed}");
        Console.WriteLine($"Processor count: {Environment.ProcessorCount}");
        Console.WriteLine();

        var sw = Stopwatch.StartNew();
        foreach (var (source, destination) in requests)
            planner.FindRoute(source, destination, routingGraph);
        sw.Stop();
        Console.WriteLine($"Sequential:            {sw.ElapsedMilliseconds,6}ms  ({requestCount / sw.Elapsed.TotalSeconds:F0} routes/sec)");

        foreach (var workerCount in new[] { 1, 2, 4, 8, 16 })
        {
            var pool = new RoutingWorkerPool(planner, workerCount);
            sw.Restart();
            pool.FindRoutes(requests, routingGraph);
            sw.Stop();
            Console.WriteLine($"Parallel (workers={workerCount,2}): {sw.ElapsedMilliseconds,6}ms  ({requestCount / sw.Elapsed.TotalSeconds:F0} routes/sec)");
        }
    }
}
