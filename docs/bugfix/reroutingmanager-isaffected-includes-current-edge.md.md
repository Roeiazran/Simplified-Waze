# Bug: ReroutingManager.IsAffected Includes the Vehicle's Unavoidable Current Edge

## Introduced In

Commit hash: `3b1acf7`

File path: `src/Waze.Simulation/Rerouting/ReroutingManager.cs`

## Fixed In

`791ed1c`

## Summary

`IsAffected` flags a vehicle as affected by a congested edge using
`Skip(vehicle.RouteIndex)`, which includes the vehicle's current edge.
`TryReroute`'s own cost comparison is built from
`Skip(vehicle.RouteIndex + 1)` onward — excluding the current edge. A
vehicle can therefore be flagged as affected purely because its own
current edge is congested, triggering a `TryReroute` call that never uses that edge.

## Fix

```csharp
// Before
return vehicle.CurrentRoute.Edges.Skip(vehicle.RouteIndex).Contains(changedEdge);

// After
return vehicle.CurrentRoute.Edges.Skip(vehicle.RouteIndex + 1).Contains(changedEdge);
```
