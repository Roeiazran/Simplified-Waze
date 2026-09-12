# Design Decision: Vehicle.Reroute Splices the Current Edge

## Date

2026-09-12

## Context

Rerouting must replace a vehicle's route while it is physically
mid-edge. `IRoutePlanner.FindRoute` only computes paths between nodes, not
from a continuous position, so a candidate route can only be calculated
starting from the node the vehicle is currently heading toward
(`currentEdge.To`), not from its exact position.

## Options Considered

1. Call the existing `AssignRoute` with the candidate route as-is, resetting
   `RouteIndex` and `PositionOnEdge` to `0`.
2. Splice the vehicle's current edge onto the front of the candidate route,
   and use a separate `Reroute` method that resets `RouteIndex` but leaves
   `PositionOnEdge` untouched.

## Decision

Option 2.

## Reasoning

Option 1 is not not precise: the candidate route's `Edges[0]` is some edge leaving
`currentEdge.To`, unrelated to the edge the vehicle is actually on.
Setting `RouteIndex = 0` against that route means index `0` refers to a
different edge than the vehicle's physical location, and resetting
`PositionOnEdge` to `0` places the vehicle at the start of it. The result
is a vehicle instantly relocating from wherever it was on its original
edge to the start of an unrelated edge elsewhere in the graph — a visible
teleport.

The vehicle is already committed to finishing its current edge regardless
of which route it ends up on afterward. Splicing that edge onto the front
of the candidate (`[currentEdgeId, ...candidateTail.Edges]`) makes
`Edges[0]` of the new route equal to the vehicle's actual current edge, so
`RouteIndex = 0` is correct. Because nothing about the vehicle's physical
position changed, `PositionOnEdge` must not be reset — it remains valid
against the same edge it already described.

This also means `Reroute` cannot reuse `AssignRoute`: the two operations
have genuinely different reset semantics. `AssignRoute` (first assignment
from `WaitingForRoute`) correctly resets both `RouteIndex` and
`PositionOnEdge`, since the vehicle has not moved yet. `Reroute` must reset
only `RouteIndex`, since the vehicle's position within its current edge is
unaffected by which route it follows afterward.

## Consequences

A vehicle sitting at `PositionOnEdge == 0` on the very edge that becomes
  congested will still traverse that one edge before a new route takes
  effect, since splicing always includes the current edge regardless of
  how much of it remains. Avoiding the edge entirely in that case is a
  separate, unimplemented refinement.

## Future Reconsideration

Revisit if movement is ever modeled with true sub-edge granularity (e.g.
mid-edge lane changes or partial-edge routing), which would require
`Reroute` to handle a route that doesn't begin exactly at the vehicle's
current edge.
