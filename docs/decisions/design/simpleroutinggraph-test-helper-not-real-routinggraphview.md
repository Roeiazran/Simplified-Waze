# Design Decision: Waze.Core.Tests Uses SimpleRoutingGraph, Not the Real RoutingGraphView

## Status

Accepted

## Date

2026-09-22

## Context

`Waze.Core.Tests` (`DijkstraRoutePlannerTests`, `RoutingWorkerPoolTests`,
`CachingRoutePlannerTests`) needs a concrete `IRoutingGraph` to call
`FindRoute` against — the interface alone has no behavior. The real
implementation, `RoutingGraphView`, lives in `Waze.Simulation`.

## Options Considered

**1. Add a project reference from `Waze.Core.Tests` to `Waze.Simulation`
and use the real `RoutingGraphView`.**

**2. Write a minimal test-only `IRoutingGraph` implementation inside
`Waze.Core.Tests` itself.**

## Decision

Option 2 — `SimpleRoutingGraph`.

```csharp
using Waze.Core.Domain;
using Waze.Core.Graph;
using Waze.Core.Routing;

public sealed class SimpleRoutingGraph : IRoutingGraph
{
    private readonly RoadGraph _graph;

    public SimpleRoutingGraph(RoadGraph graph) => _graph = graph;

    public IReadOnlyList<RoadEdge> GetOutgoingEdges(NodeId node) => _graph.GetOutgoingEdges(node);

    public RoadEdge GetEdge(EdgeId id) => _graph.GetEdge(id);

    public double GetCost(EdgeId id) => GetEdge(id).Length;
}
```

Used as:

```csharp
var route = planner.FindRoute(a, c, new SimpleRoutingGraph(graph));
```

## Reasoning

Unlike the Core/Simulation split in the `IRoutingGraph` ADR, this option
is **not** forced by a compiler constraint. `Waze.Core.Tests` is a leaf in
the dependency graph — nothing depends on it — so
`Waze.Core.Tests → Waze.Simulation → Waze.Core` would be a legal, acyclic
reference chain. Referencing `Waze.Simulation` and using the real
`RoutingGraphView` would compile and run without issue. This was a
deliberate style choice, not a technical necessity.

**Keeping the test's scope honest.** `DijkstraRoutePlannerTests` exists to
verify Dijkstra's *algorithm*, not `RoutingGraphView`'s cost formula or
however Simulation happens to combine traffic and topology today. Using
the real `RoutingGraphView` would mean a future change to Simulation's
traffic-cost formula could silently break Core's tests, even though
nothing about Dijkstra's own correctness changed — a failure at the wrong
layer, for the wrong reason. `SimpleRoutingGraph`'s fixed behavior
(`cost = Length`) keeps these tests asserting exactly what they always
asserted, regardless of what Simulation does with its own cost model.

**Testability isn't only about production code being decoupled**: if Core's own test suite required
Simulation to run at all, Core would not be independently testable in
practice, whatever the production code claimed. This decision keeps that
independence true at the test level, not only the production-code level.

## Consequences

- `Waze.Core.Tests` stays dependency-clean — still only referencing
  `Waze.Core`.
- Existing numeric assertions in `DijkstraRoutePlannerTests` and
  `RoutingWorkerPoolTests` (e.g. `Assert.Equal(6, route.TotalCost)`)
  remained correct unchanged when `IRoutingGraph` was introduced, since
  `SimpleRoutingGraph` reproduces the exact prior cost behavior.
- If `Waze.Simulation`'s cost formula changes in the future, Core's own
  tests are unaffected — by design.