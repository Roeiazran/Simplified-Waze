# Full Simulation Scale Benchmark

Date: 2026-09-17

Git Commit: `8e9a916`

## Goal

Run the fully wired simulation (vehicle creation, movement, traffic
tracking, congestion detection, rerouting), and measure whether congestion/rerouting behave as expected under real load.

## Environment

CPU: 8 physical cores, 8 logical

## Configuration

`Graph:` 50×50 grid, 2,500 nodes (~10,000 directed edges)\
`Edge length:` 50, speed limit: 10 (5 ticks to cross one edge)\
`Edge capacity:` 3\
`Vehicles:` 5,000 (random source/destination pairs)\
`Ticks:` 200\
`Random seed:` 481516\
`Routing:` Dijkstra.
`Cache:` None.

## Procedure

Single run, Release build, no warmup.

## Results

| Metric | Value |
|---|---:|
| Wall time | 104,973ms |
| Ticks/sec | 2 |
| Arrived | 3,391 |
| Still driving | 1,609 |
| Congestion events | 24,467 |
| Reroute attempts | 312,668 |
| Successful reroutes | 0 |

## Observations

**Zero reroutes despite 312,668 attempts is expected, not a bug.** Every
vehicle's route was computed by real Dijkstra via `CreateVehicle`, so each
one is already optimal for the only cost function that exists —
`RoadEdge.Length`. Congestion changes vehicle *counts* on an edge but does
not change that edge's *cost*, so `TryReroute`'s recomputation of a
vehicle's remaining path is mathematically guaranteed to reproduce the
exact same continuation every time.

## Conclusion

The rerouting mechanism is functioning exactly as designed given its
current inputs — it correctly finds no improvement.

## Next Experiments

1. Implement `RouteCache`/`CachingRoutePlanner` and re-run this exact
  configuration — expect wall time to drop substantially with reroute
  attempts and successes unchanged (cost function hasn't changed).
1. Implement traffic-aware cost and re-run — expect `reroutes` to
  become nonzero, since congested edges would finally cost more than
  their static length.
