## Dijkstra Routing

### Purpose

Baseline shortest-path algorithm for the routing subsystem. Provides a
correct, simple reference implementation that future routing algorithms
(Dynamic Shortest Paths, Contraction Hierarchies, ...) are compared against.

### Input

- `RoadGraph` — the road network to search over
- `NodeId source`
- `NodeId destination`

### Output

A `Route` containing:
- the ordered list of `EdgeId`s from source to destination
- the total cost of the path

### Data Structures

```csharp
var distances = new Dictionary<NodeId, double> { [source] = 0 };
var predecessorEdge = new Dictionary<NodeId, EdgeId>();
var visited = new HashSet<NodeId>();
var queue = new PriorityQueue<NodeId, double>();
```

- `distances` — cheapest known cost from `source` to each node reached so far.
- `predecessorEdge` — the edge used to reach each node on its cheapest known path; used to reconstruct the route at the end.
- `visited` — nodes whose shortest distance is finalized (lazy-deletion set, since `PriorityQueue<T>` has no decrease-key operation).
- `queue` — candidate nodes ordered by tentative distance.

### Priority Queue Implementation

`PriorityQueue<NodeId, double>` is a binary min-heap: a complete binary
tree stored as a flat array, maintaining the invariant that every parent's
priority is less than or equal to its children's. This gives `Enqueue` and
`Dequeue` both O(log n) — insert at the end and "bubble up," or replace the
root with the last element and "bubble down."

The .NET implementation has one relevant limitation: it does not support
decrease-key (updating the priority of an entry already in the queue).

### Lazy Deletion

Because decrease-key isn't available, a cheaper distance to a node is
never applied to an existing queue entry — a new entry is enqueued instead:

```csharp
queue.Enqueue(edge.To, newDistance);
```

A single node can therefore exist in the queue multiple times
simultaneously, with different (stale and current) priorities.

This is resolved on the dequeue side rather than the enqueue side:

```csharp
var current = queue.Dequeue();
if (!visited.Add(current))
    continue;
```

The heap always yields the smallest-priority entry first, so the first
time a node is dequeued it carries its true shortest distance and is
processed. Any later, stale duplicate of the same node is discarded by
the `visited` check without affecting correctness — it is simply never
acted on. This avoids needing an indexed/decrease-key-capable heap at
the cost of the queue occasionally holding a small number of dead entries.


### Algorithm

```csharp
while (queue.Count > 0)
{
    var current = queue.Dequeue();
    if (!visited.Add(current))
        continue;

    if (current.Equals(destination))
        break;

    foreach (var edge in graph.GetOutgoingEdges(current))
    {
        var newDistance = distances[current] + edge.Length;
        if (!distances.TryGetValue(edge.To, out var existing) || newDistance < existing)
        {
            distances[edge.To] = newDistance;
            predecessorEdge[edge.To] = edge.Id;
            queue.Enqueue(edge.To, newDistance);
        }
    }
}
```

1. Dequeue the cheapest unvisited node.
2. Skip it if already visited — a stale, more expensive queue entry from earlier.
3. Stop once the destination itself is dequeued; its distance is now final.
4. Relax every outgoing edge: if reaching `edge.To` via `current` beats any previously known distance, record the new distance and predecessor edge, and enqueue it.

```csharp
var edges = new List<EdgeId>();
var node = destination;
while (!node.Equals(source))
{
    var edgeId = predecessorEdge[node];
    edges.Add(edgeId);
    node = graph.GetEdge(edgeId).From;
}
edges.Reverse();
```

5. Walk backward from `destination` to `source` via `predecessorEdge`, collecting edges.
6. Reverse the collected edges into source→destination order.
