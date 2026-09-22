# ADR: Traffic-Aware Routing via IRoutingGraph (Dependency Inversion)

## Status

Accepted

## Date

2026-09-20

## Context

The original [roadgraph-over-iroutinggraph](./roadgraph-over-iroutinggraph-for-iroutplanner.md) ADR chose
`RoadGraph` directly for `IRoutePlanner.FindRoute`, explicitly deferring
`IRoutingGraph` until traffic-aware cost existed. This ADR
implements the deferred change and supersedes that earlier one.

`DijkstraRoutePlanner` lives in `Waze.Core` and needs traffic-aware cost.
Traffic (`TrafficState`) lives in `Waze.Simulation`. Per section 2.2 of the architecture: `Waze.Core` can never depend on `Waze.Simulation`.

## Options Considered

**1. Define `IRoutingGraph` and its implementation both in `Waze.Core`.**
Rejected — the implementation needs `TrafficState`, a `Waze.Simulation`
type. Core would have to reference Simulation, the forbidden direction.

**2. Define `IRoutingGraph` and its implementation both in `Waze.Simulation`.**
Rejected — it doesn't compile.
`DijkstraRoutePlanner`'s method signature needs to reference
`IRoutingGraph`, which would require `Waze.Core.csproj` to reference
`Waze.Simulation.csproj`. But `Waze.Simulation.csproj` already references
`Waze.Core.csproj`. That is a circular project reference, which MSBuild
refuses to build at all.

**3. Split: define `IRoutingGraph` in Core, implement it (`RoutingGraphView`)
in Simulation.**

## Decision

Option 3 — Dependency Inversion.

## Reasoning

The classic statement of the principle (Robert C. Martin):

1. High-level modules should not depend on low-level modules. Both should
   depend on abstractions.
2. Abstractions should not depend on details. Details should depend on
   abstractions.

Mapped onto this codebase:

| DIP term | This codebase |
|---|---|
| High-level module (the policy) | `DijkstraRoutePlanner` |
| Low-level module (the detail) | `RoutingGraphView` |
| The shared abstraction | `IRoutingGraph` |


This principle is realized in practice through Method (or parameter) Injection. `DijkstraRoutePlanner`
has no constructor parameters at all; `IRoutingGraph` arrives as a
parameter to `FindRoute` itself, rather than the constructor injection (used in the classic DI).

At compile time, the dependency arrow points
the normal way — Core knows nothing about Simulation. At runtime, the
algorithm defined in the lower layer ends up consuming data from the
higher layer, entirely through the abstraction it owns.

## What Changed

`IRoutePlanner.FindRoute`'s signature, before and after:
```csharp
// Before
Route FindRoute(NodeId source, NodeId destination, RoadGraph graph);

// After
Route FindRoute(NodeId source, NodeId destination, IRoutingGraph graph);
```

New: `src/Waze.Core/Routing/IRoutingGraph.cs` — the abstraction, owned by
the consumer's project, knowing nothing about traffic:
```csharp
public interface IRoutingGraph
{
    IReadOnlyList<RoadEdge> GetOutgoingEdges(NodeId node);
    RoadEdge GetEdge(EdgeId id);
    double GetCost(EdgeId id);
}
```

New: `src/Waze.Simulation/Routing/RoutingGraphView.cs` — the detail,
combining `RoadGraph` and `TrafficState`, conforming to Core's interface:
```csharp
public sealed class RoutingGraphView : IRoutingGraph
{
    private readonly RoadGraph _graph;
    private readonly TrafficState _trafficState;

    public RoutingGraphView(RoadGraph graph, TrafficState trafficState)
    {
        _graph = graph;
        _trafficState = trafficState;
    }

    public IReadOnlyList<RoadEdge> GetOutgoingEdges(NodeId node) => _graph.GetOutgoingEdges(node);

    public RoadEdge GetEdge(EdgeId id) => _graph.GetEdge(id);

    public double GetCost(EdgeId id)
    {
        var edge = _graph.GetEdge(id);
        var state = _trafficState.GetOrCreateState(id);
        var freeFlowTime = edge.Length / edge.SpeedLimit;
        var loadRatio = (double)state.VehicleCount / edge.Capacity;
        return freeFlowTime * (1 + loadRatio);
    }
}
```

At runtime, `DijkstraRoutePlanner.FindRoute` calls `graph.GetCost(edge.Id)`
against a reference declared only as `IRoutingGraph` — but the concrete
object behind that reference (constructed once in `SimulationEngine`) is a
`RoutingGraphView` reading the simulation's actual, live `TrafficState`.
`DijkstraRoutePlanner.cs` never imports `Waze.Simulation` and was compiled
with no knowledge it exists.

## Consequences

- `RoutingWorkerPool`, `CachingRoutePlanner`, and `ReroutingManager` all
  take `IRoutingGraph` now instead of `RoadGraph`.
- `Waze.Core.Tests` cannot reference the real `RoutingGraphView` (it would
  mean Core's own tests depending on Simulation) — a minimal
  `SimpleRoutingGraph` test helper (cost = `Length`, matching the old
  behavior) preserves existing test assertions unchanged.
- `Waze.Simulation.Tests` and the benchmark project already reference both
  projects, so they use the real `RoutingGraphView` directly.
