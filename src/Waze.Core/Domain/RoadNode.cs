namespace Waze.Core.Domain;

public sealed class RoadNode
{
    public NodeId Id { get; }

    public RoadNode(NodeId id)
    {
        Id = id;
    }
}
