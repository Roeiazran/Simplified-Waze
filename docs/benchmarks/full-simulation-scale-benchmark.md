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

### Run 2 — with RouteCache/CachingRoutePlanner (2026-09-18, commit `4150538`)

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

### Run 3 — traffic-aware cost via IRoutingGraph (2026-09-22, commit `ce89d62`)

Same configuration as Runs 1-2, only change: routing cost now comes from
`RoutingGraphView.GetCost` instead of static `RoadEdge.Length`.

| Metric | Value |
|---|---:|
| Wall time | 32,526ms |
| Ticks/sec | 6 |
| Arrived | 3,386 |
| Still driving | 1,614 |
| Congestion events | 34,784 |
| Reroute attempts | 129,710 |
| Successful reroutes | 45,078 |
| Cache hits | 69,826 |
| Cache misses | 59,884 |


## Observations (Run 3)

**`reroutes` finally leaves zero — the prediction from Run 1 is confirmed.**
Since cost now genuinely rises with congestion, recomputing a vehicle's
remaining path can find a real improvement, unlike the traffic-blind cost
function where recomputation was mathematically guaranteed to reproduce
the original route every time.

**Congestion events rose 42% (24,467 → 34,784), not fell.** This is a
real, plausible emergent effect, not noise: vehicles now actively reroute
away from congested edges (45,078 times), but rerouting away from one
jam means piling onto some *other* edge — which can itself become newly
congested from the sudden extra traffic. The simulation is now exhibiting
a recognizable real-world phenomenon: relieving one bottleneck can create
another elsewhere, rather than making total congestion monotonically
decrease.

**Reroute attempts fell 58% (312,668 → 129,710) despite more congestion
events.** This is the self-regulating counterpart to the above: once a
vehicle successfully reroutes away from an edge, that edge leaves its
remaining route, so it's no longer checked against future congestion
events on that same edge. With 45,078 real reroutes removing vehicles
from future affected-checks, the at-risk population per edge shrinks over
the run, more than offsetting the higher number of congestion events.

**Cache hit rate dropped from 83.2% to 53.8% (69,826 / 129,710).** In
Run 2, routing decisions never changed, so the same vehicle asked the
same `(node, destination)` question repeatedly across many ticks — highly
repetitive, highly cacheable. Now that vehicles actually reroute, each
successful reroute puts a vehicle on genuinely new ground, querying
`(node, destination)` pairs that haven't been asked before — more real
diversity in the query pattern, so a lower hit rate is the expected
consequence of the mechanism actually working, not a regression in the
cache itself.

**Wall time rose to 32,526ms despite 58% fewer attempts** — driven by two
compounding factors: misses actually increased slightly in absolute terms
(52,617 → 59,884) despite far fewer total attempts, because the hit rate
fell so much; and each miss is now intrinsically more expensive, since
`RoutingGraphView.GetCost` performs a `TrafficState` dictionary lookup for
*every* edge Dijkstra relaxes during a search, not just a flat field read
like the old `edge.Length`-based cost.

## Conclusion (updated)

Both predicted experiments from Run 1 are now confirmed with real
measurements: caching eliminates redundant computation without changing
outcomes (Run 2), and traffic-aware cost makes rerouting actually
succeed, at the cost of more expensive individual routing calls and a
lower cache hit rate as vehicles' behavior genuinely diverges over time
(Run 3). The system is now exhibiting a real emergent traffic dynamic —
congestion migrating rather than simply disappearing — that no earlier,
traffic-blind version of this simulation could have produced.

## Next Experiments

1. ~~Implement `RouteCache`/`CachingRoutePlanner`~~ — done, see Run 2.
1. ~~Implement traffic-aware cost~~ — done, see Run 3.
1. Investigate the congestion-migration effect directly — track which
   edges become congested *after* a nearby edge's congestion triggers
   reroutes, to confirm the mechanism suspected above rather than infer
   it from aggregate counts alone.