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

### Run 1 — no cache (2026-09-17, commit `8e9a916`)

| Metric | Value |
|---|---:|
| Wall time | 104,973ms |
| Ticks/sec | 2 |
| Arrived | 3,391 |
| Still driving | 1,609 |
| Congestion events | 24,467 |
| Reroute attempts | 312,668 |
| Successful reroutes | 0 |

### Run 2 — with RouteCache/CachingRoutePlanner (2026-09-18, commit `<fill in after committing>`)

Same configuration, only change: `ReroutingManager` now receives
`CachingRoutePlanner(DijkstraRoutePlanner, RouteCache)` instead of a plain
`DijkstraRoutePlanner`.

| Metric | Value |
|---|---:|
| Wall time | 19,499ms |
| Ticks/sec | 10 |
| Arrived | 3,391 |
| Still driving | 1,609 |
| Congestion events | 24,467 |
| Reroute attempts | 312,668 |
| Successful reroutes | 0 |
| Cache hits | 260,051 |
| Cache misses | 52,617 |

## Observations

**Zero reroutes despite 312,668 attempts is expected, not a bug.** Every
vehicle's route was computed by real Dijkstra via `CreateVehicle`, so each
one is already optimal for the only cost function that exists —
`RoadEdge.Length`. Congestion changes vehicle *counts* on an edge but does
not change that edge's *cost*, so `TryReroute`'s recomputation of a
vehicle's remaining path is mathematically guaranteed to reproduce the
exact same continuation every time.

**The cache produced a ~5.4x speedup (104,973ms → 19,499ms) while leaving
every simulation outcome identical** — arrived/still-driving/congestion/
reroute-attempts/reroutes are exactly the same between both runs, exactly
as predicted: caching changes how often a route gets *recomputed*, not
which routes exist or what decisions get made from them.

**83.2% hit rate** (260,051 / 312,668) confirms the reasoning behind
targeting `ReroutingManager` specifically: a vehicle's destination stays
fixed for its whole trip, and its current position repeats identically
across every congestion check that lands while it's resident on the same
edge (5 ticks per edge), so the same `(source, destination)` pair
genuinely does recur often at this call site — unlike `CreateVehicle`'s
one-shot random pairs, where a cache was deliberately not applied.

## Conclusion

The rerouting mechanism is functioning exactly as designed given its
current inputs — it correctly finds no improvement, because none exists
yet under a traffic-blind cost function. The cache closes the
*performance* gap (redundant recomputation) but not the *behavioral* one
(reroutes never succeeding) — those are independent problems, and this
run confirms they were independent in practice, not just in theory.

## Next Experiment

Implement traffic-aware cost and re-run — expect `reroutes` to
  become nonzero, since congested edges would finally cost more than
  their static length.
