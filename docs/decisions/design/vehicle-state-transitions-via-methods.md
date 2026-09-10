# Design Decision: Vehicle Exposes State Transitions via Methods, Not Public Setters

## Status

Accepted

## Date

2026-09-10

## Context

`Vehicle` holds a lifecycle `State` and a `CurrentRoute`. Both
change over time as the simulation progresses — a route gets assigned, a
vehicle arrives, and eventually it may be rerouted. The question is how
external code (a future `VehicleManager`/`ReroutingManager`) should be
allowed to change these two fields.

## Options Considered

1. 
    Public settable properties:
    ```csharp
    public Route? CurrentRoute { get; set; }
    public VehicleState State { get; set; }
    ``` 

    Any caller can set either field to any value at any time.

1. 
    Get-only from outside, private set, changed only through named methods (AssignRoute, MarkArrived) that update both related fields together:

    ```csharp
    public Route? CurrentRoute { get; private set; }
    public VehicleState State { get; private set; }

    public void AssignRoute(Route route) { 
        CurrentRoute = route; State = VehicleState.Driving;
    }
    public void MarkArrived() { State = VehicleState.Arrived; }
    ```

## Decision

Option 2.

## Reasoning

Public setters would let calling code set `State` and
`CurrentRoute` independently and inconsistently — e.g. `State = Driving`
with `CurrentRoute` still `null`, or `State = Arrived` with a stale route still attached.

Grouping the two updates inside named methods makes the
only reachable combinations of (State, CurrentRoute) the ones the lifecycle actually allows, without requiring a full state-machine validator at this stage.

This keeps `Vehicle` itself dependency-free and trivially testable in isolation: tests only need to call `AssignRoute/MarkArrived` and
assert on the resulting state, with no need to construct a RoadGraph, IRoutePlanner, or traffic state to do so.

Any such guard should be confined to `Vehicle`'s own `State` property rather than introducing dependencies on `RoadGraph`, `IRoutePlanner`, or `TrafficState`.

The isolation this decision establishes is independent of transition validity and should be preserved regardless of how that validation is eventually implemented.

