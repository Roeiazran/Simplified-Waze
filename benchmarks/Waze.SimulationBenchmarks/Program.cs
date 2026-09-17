using Waze.SimulationBenchmarks;

Console.WriteLine("=== Routing Throughput Benchmark ===");
RoutingThroughputBenchmark.Run(gridSize: 50, requestCount: 2000, seed: 481516);

Console.WriteLine();
Console.WriteLine("=== Full Simulation Scale Test ===");
SimulationScaleBenchmark.Run(gridSize: 50, vehicleCount: 5000, tickCount: 200, seed: 481516);
