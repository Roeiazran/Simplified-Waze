# Design Decision: VehicleManager Exposes a Read-Only Vehicle List

## Status

Accepted

## Date

2026-09-11

## Context

Outside consumers (future server queries, metrics) need to iterate
`VehicleManager`'s vehicle collection. They must be able to read it, but
must never be able to change it.

## Why iteration must be allowed, but mutation must not

`VehicleManager` is the only component allowed to decide which vehicles
exist. If an outside reader could add or remove vehicles directly,
components whose only job is to report simulation state could silently
alter it instead — breaking the rule that shared state has exactly one
owner.

## Why a private setter doesn't solve this

A private setter only stops the property from being *reassigned* to a
different list. It does nothing to stop `.Add()`/`.Remove()` being called
on the same list object the getter already hands out.

## Decision

```csharp
private readonly List<Vehicle> _vehicles = new();
public IReadOnlyList<Vehicle> Vehicles => _vehicles;
public void Add(Vehicle vehicle) => _vehicles.Add(vehicle);
```

`_vehicles` and `Vehicles` are two differently-typed references to the same
object: one mutable, kept private for `VehicleManager`'s own use; one
read-only, exposed to every outside caller. `IReadOnlyList<Vehicle>` has no
mutating members at all, so no external call can compile against it —
regardless of what a private setter would or wouldn't have blocked.
