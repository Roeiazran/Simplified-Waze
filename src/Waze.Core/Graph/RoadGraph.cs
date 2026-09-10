using Waze.Core.Domain;

namespace Waze.Core.Graph;

public sealed class RoadGraph
{
    private readonly Dictionary<NodeId, RoadNode> _nodes = new();
    private readonly Dictionary<EdgeId, RoadEdge> _edges = new();
    private readonly Dictionary<NodeId, List<EdgeId>> _outgoing = new();

    public void AddNode(RoadNode node)
    {
        _nodes.Add(node.Id, node);
        _outgoing.Add(node.Id, new List<EdgeId>());
    }

    public void AddEdge(RoadEdge edge)
    {
        _edges.Add(edge.Id, edge);
        _outgoing[edge.From].Add(edge.Id);
    }

    public RoadNode GetNode(NodeId id) => _nodes[id];

    public RoadEdge GetEdge(EdgeId id) => _edges[id];

    public IReadOnlyList<RoadEdge> GetOutgoingEdges(NodeId nodeId) =>
        _outgoing[nodeId].Select(edgeId => _edges[edgeId]).ToList();
}
