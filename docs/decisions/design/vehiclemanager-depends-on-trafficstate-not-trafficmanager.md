# Design Decision: VehicleManager Depends on TrafficState, Not TrafficManager

## Status

Accepted

## Date

2026-09-15

## Context

Wiring vehicle movement into traffic tracking requires
`VehicleManager` to record when a vehicle enters or leaves an edge. The
question is what it should depend on to do that: the raw `TrafficState`
data, or the full `TrafficManager` that also owns congestion detection.

## Options Considered

1. `VehicleManager.AdvanceAll` takes a `TrafficManager`, and reaches into
   it (e.g. `trafficManager.State.VehicleEntered(...)`, or a forwarding
   method added to `TrafficManager` for this purpose).
2. `VehicleManager.AdvanceAll` takes a `TrafficState` directly.

## Decision

Option 2 (implemented).

## Reasoning

`VehicleManager` only ever needs to call `VehicleEntered`/`VehicleLeft` —
recording that a crossing happened. It has no legitimate use for
`TrafficManager.DetectCongestedEdges` or anything `CongestionDetector`
does; deciding whether an edge is congested is a separate concern,
belonging entirely to `TrafficManager` and `SimulationEngine`'s
coordination of it.

Depending on the full `TrafficManager` would hand `VehicleManager` access
to capabilities it never uses, which breaks the rule "separation of concerns", by coupling  `VehicleManager` and `TrafficManager`.

Adding a forwarding method on `TrafficManager` (e.g.
`RecordVehicleEntered(edgeId)`) was also considered and rejected, because `TrafficState` is already a properly encapsulated
class and a pass-through method would add indirection
without adding any actual safety or clarity.

## Consequences

- `VehicleManager` and `TrafficManager` remain decoupled from each other;
  `SimulationEngine` is the only component that holds both and coordinates
  between them.