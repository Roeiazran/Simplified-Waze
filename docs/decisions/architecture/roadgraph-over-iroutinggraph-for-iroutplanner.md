# ADR: Use RoadGraph Directly in IRoutePlanner Instead of IRoutingGraph

## Status

Accepted

## Date

2026-09-10

## Context

Architecture 8.2 shows `IRoutePlanner.FindRoute` taking an `IRoutingGraph`
parameter — a routing-facing view meant to combine graph topology with a
traffic snapshot.

At the point of implementing the Dijkstra baseline, only static topology
exists - No traffic system has been built yet — that is a later stage in the incremental development order.

## Options Considered

1. Introduce `IRoutingGraph` now, with a single pass-through implementation
   wrapping `RoadGraph`, even though there is no traffic state to combine
   it with yet.
2. Have `IRoutePlanner.FindRoute` depend directly on `RoadGraph` for now,
   and introduce `IRoutingGraph` later, once traffic state exists.

## Decision

Option 2. `IRoutePlanner.FindRoute` takes a `RoadGraph` directly:

```csharp
public interface IRoutePlanner
{
    Route FindRoute(NodeId source, NodeId destination, RoadGraph graph);
}
```

## Reasoning
1. Depending on RoadGraph directly
keeps the routing baseline minimal and lets it be verified correct, without speculative design for a capability that doesn't exist yet.

## Consequences

1. `DijkstraRoutePlanner` currently uses `RoadEdge.Length` as routing cost directly — consistent with there being no dynamic cost to consult yet.
1. `IRoutePlanner`'s signature will need to change when Traffic State is introduced.
1. Any code written against `IRoutePlanner` during this phase will need a small update (parameter type change) at that point.

