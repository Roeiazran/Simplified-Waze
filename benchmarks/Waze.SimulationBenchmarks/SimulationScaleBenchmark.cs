using System.Diagnostics;
using Waze.Core.Domain;
using Waze.Core.Routing;
using Waze.Simulation.Engine;
using Waze.Simulation.Rerouting;
using Waze.Simulation.Traffic;
using Waze.Simulation.Vehicles;

namespace Waze.SimulationBenchmarks;

public static class SimulationScaleBenchmark
{
    public static void Run(int gridSize, int vehicleCount, int tickCount, int seed)
    {
        var graph = GraphGenerator.BuildGridGraph(gridSize);
        var vehicleManager = new VehicleManager();
        var trafficManager = new TrafficManager(new TrafficState(), new CongestionDetector());
        var routeCache = new RouteCache();
        var cachingPlanner = new CachingRoutePlanner(new DijkstraRoutePlanner(), routeCache);
        var reroutingManager = new ReroutingManager(cachingPlanner);
        var engine = new SimulationEngine(graph, vehicleManager, trafficManager, reroutingManager);

        var random = new Random(seed);
        var maxNode = gridSize * gridSize - 1;
        var planner = new DijkstraRoutePlanner();
        const int cacheTtlTicks = 5;

    
        for (var i = 0; i < vehicleCount; i++)
        {
            var source = new NodeId(random.Next(0, maxNode + 1));
            var destination = new NodeId(random.Next(0, maxNode + 1));
            vehicleManager.CreateVehicle(new VehicleId(i), source, destination, planner, graph, trafficManager.State);
        }

        Console.WriteLine($"Created {vehicleCount} vehicles on a {gridSize}x{gridSize} grid ({maxNode + 1} nodes)");

        var sw = Stopwatch.StartNew();
        for (var tick = 0; tick < tickCount; tick++) {
            if (tick % cacheTtlTicks == 0) routeCache.Clear();
            engine.Tick();
        }
        sw.Stop();

        var arrivedCount = vehicleManager.Vehicles.Count(v => v.State == VehicleState.Arrived);
        var drivingCount = vehicleManager.Vehicles.Count(v => v.State == VehicleState.Driving);

        Console.WriteLine($"Ran {tickCount} ticks in {sw.ElapsedMilliseconds}ms ({tickCount / sw.Elapsed.TotalSeconds:F0} ticks/sec)");
        Console.WriteLine($"Arrived: {arrivedCount}, still driving: {drivingCount}");
        Console.WriteLine($"Congestion events: {engine.TotalCongestionEvents}, reroute attempts: {engine.TotalRerouteAttempts}, reroutes: {engine.TotalReroutes}");
        Console.WriteLine($"Route cache: {cachingPlanner.Hits} hits, {cachingPlanner.Misses} misses");
    }
}
