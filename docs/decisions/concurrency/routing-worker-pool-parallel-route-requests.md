# Concurrency Decision: Parallel Route Requests via RoutingWorkerPool

## Status

Accepted

## Date

2026-09-13

## Context

Different vehicles' route calculations don't depend on each
other. This introduces the first genuinely concurrent code in the project.

## Shared State

### RoadGraph

- **Owner:** Setup code that constructs the graph before routing begins.
- **Readers:**
  - **Before:** `DijkstraRoutePlanner`, called sequentially — one `FindRoute` call at a time, from a single thread.
  - **After:** `DijkstraRoutePlanner`, called concurrently by every worker in `RoutingWorkerPool` at once.
- **Writers:** None during routing — `AddNode`/`AddEdge` only run during setup.
- **Consistency requirement:**
  - **Before:** No concurrent-view concern existed, since only one reader could ever be active at a time,.
  - **After:** Every concurrent reader must see the same graph for the duration of routing.
- **Synchronization strategy:** None required, before or after. Concurrent reads of a `Dictionary` that is never concurrently written are safe.

### IRoutePlanner / DijkstraRoutePlanner instance

- **Owner:** Whoever constructs the `RoutingWorkerPool`.
- **Readers:** N/A — not a data structure; called rather than read.
  - **Before:** Called once at a time, from a single thread.
  - **After:** Called from multiple threads simultaneously, **on the same instance**.
- **Writers:** N/A.
- **Consistency requirement:**
  - **Before:** Trivial — no concurrent calls could exist.
  - **After:** Each concurrent call must execute independently, with no observable interaction between them.
- **Synchronization strategy:** None required, before or after. The instance holds no fields of its own — every calculation's state lives entirely in that call's local variables.

## Unshared (Per-Call / Per-Worker) State

- `distances`, `predecessorEdge`, `visited`, `queue` inside
  `DijkstraRoutePlanner.FindRoute` — local variables, freshly created on
  every call, both before and after this step.
- `results[i]` in `RoutingWorkerPool.FindRoutes` — each
  parallel iteration writes to its own array index, no two iterations
  ever touch the same slot.

## What Changed

For illustration only, here is the naive sequential shape this could have
taken, shown purely as a reference point:

```csharp
for (var i = 0; i < requests.Count; i++)
{
    var (source, destination) = requests[i];
    results[i] = _routePlanner.FindRoute(source, destination, graph);
}
```

What was actually written instead is:
```csharp
Parallel.For(0, requests.Count, new ParallelOptions { MaxDegreeOfParallelism = _workerCount }, i =>
{
    var (source, destination) = requests[i];
    results[i] = _routePlanner.FindRoute(source, destination, graph);
});
```

The only difference is execution strategy: the parallel version runs iterations across up to `_workerCount` threads instead of one after
another.

[Benchmark link](../../benchmarks/routing-worker-pool-throughput.md)