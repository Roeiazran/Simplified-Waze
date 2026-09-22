# Design Decision: SimulationEngine and VehicleManager Build RoutingGraphView Internally

## Status

Accepted

## Date

2026-09-20

## Context

Introducing `IRoutingGraph`/`RoutingGraphView` (combining `RoadGraph` with
`TrafficState` for traffic-aware routing cost) raised a question: should
`SimulationEngine` and `VehicleManager.CreateVehicle` accept an
`IRoutingGraph` as a constructor/method parameter, or build one internally
from the `RoadGraph`/`TrafficState` they already hold?

## Options Considered

**1. Caller constructs and passes the routing view directly:**
```csharp
var trafficManager = new TrafficManager(new TrafficState(), new CongestionDetector());
var routingGraph = new RoutingGraphView(graph, trafficManager.State);
var engine = new SimulationEngine(graph, routingGraph, vehicleManager, trafficManager, reroutingManager);
```

**2. Build it internally, from state the class already owns:**
```csharp
public SimulationEngine(RoadGraph graph, VehicleManager vehicleManager, TrafficManager trafficManager, ReroutingManager reroutingManager)
{
    _graph = graph;
    _vehicleManager = vehicleManager;
    _trafficManager = trafficManager;
    _reroutingManager = reroutingManager;
    _routingGraph = new RoutingGraphView(graph, trafficManager.State);
}
```

## Decision

Option 2 (implemented), for both `SimulationEngine` and
`VehicleManager.CreateVehicle`.

## Reasoning

Option 1 compiles fine but enforces its critical invariant — that the
supplied `IRoutingGraph` is built from the *same* `TrafficState` instance
`TrafficManager` is actually mutating each tick — by convention only.
Nothing stops a caller from passing a `RoutingGraphView` built from an
unrelated `TrafficState`:
```csharp
var routingGraph = new RoutingGraphView(graph, new TrafficState()); // compiles, silently wrong
```
This would compile, run, and produce no error — rerouting would compute
costs against a permanently-empty phantom traffic state, disconnected
from the real one `VehicleManager`/`TrafficManager` are updating, and the
entire point of traffic-aware routing would be silently defeated.

Building `RoutingGraphView` internally, from `trafficManager.State` — the
same reference already used elsewhere in `Tick()` — makes this mismatch
structurally impossible rather than merely unlikely: the caller is never
given the opportunity to supply a `RoutingGraphView` at all, so there is
no code path left where it could be built from the wrong `TrafficState`.

Note this bug would not announce itself. If a mismatched `TrafficState`
were ever passed under Option 1, every other output would still look
correct — movement, tick counts, and `CongestionDetector`'s congestion
counts all read the real `TrafficState` directly and are unaffected. Only
`ReroutingManager`'s cost calculations would silently use the empty state, making every edge's `VehicleCount` read as `0`
and cost collapse back to pure free-flow time.


## Consequences

- `SimulationEngine`'s and `VehicleManager.CreateVehicle`'s public
  signatures stay unchanged from before `IRoutingGraph` existed —
  `SimulationEngineTests` and most of `VehicleManagerTests` required no
  changes when this step landed.
- `SimulationEngine` cannot have its routing view substituted for a fake
  `IRoutingGraph` in isolation — testing it requires a real `RoadGraph`
  and `TrafficState`, since the concrete `RoutingGraphView` is constructed
  internally rather than accepted as an injectable dependency.