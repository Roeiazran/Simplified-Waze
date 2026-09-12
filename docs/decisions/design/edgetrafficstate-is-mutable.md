# Design Decision: EdgeTrafficState Is Mutable

## Date

2026-09-12

## Context

`EdgeTrafficState` holds the number of vehicles currently on one
edge, updated continuously as vehicles enter and leave. The question is
whether `VehicleCount` should behave as a value (immutable, replaced
wholesale on each change, like `Route`) or as a live counter (mutated in
place, like `Vehicle.PositionOnEdge`).

## Options Considered

1. Immutable — treat it like `Route`/`RoadEdge`: a description of traffic
   conditions at one instant, replaced wholesale whenever it changes.
2. Mutable — treat it like `Vehicle.PositionOnEdge`/`RouteIndex`: a live,
   owned counter, updated directly.

## Decision

Option 2.

## Reasoning

The codebase distinguishes these two categories by how the data
behaves, not by convention. `Route` is handed to a `Vehicle` and held for
an entire trip; when it changes, the old one is discarded and a new one
takes its place. `Vehicle.PositionOnEdge`/`RouteIndex` are never handed
anywhere else to be held independently, and nothing retains "the value
from three ticks ago" — they are mutated directly by their single owner.

`VehicleCount` fits the second category on its own terms: it exists
specifically to be incremented and decremented as vehicles pass through,
is owned by exactly one `TrafficState`, and nothing in the system holds a
reference to a past `EdgeTrafficState` the way a `Vehicle` holds a
specific `Route`.

## Consequences

- A future consumer needing a stable point-in-time snapshot (e.g. a server
  response) must copy the values it needs rather than hold an
  `EdgeTrafficState` reference, since the object it holds can keep
  changing afterward.
- Concurrent access, if introduced later, will need its own
  synchronization strategy for this mutable state.

## Future Reconsideration

Revisit only if `EdgeTrafficState` starts being handed to and retained by
other objects as a description of a moment in time, rather than queried
fresh each time — at that point it would behave like a value, not a live
counter, and the categorization above would need to be re-applied.