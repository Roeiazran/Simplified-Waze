namespace Waze.Core.Domain;

public sealed class Route
{
    public NodeId Source { get; }
    public NodeId Destination { get; }
    public IReadOnlyList<EdgeId> Edges { get; }
    public double TotalCost { get; }

    public Route(NodeId source, NodeId destination, IReadOnlyList<EdgeId> edges, double totalCost)
    {
        Source = source;
        Destination = destination;
        Edges = edges;
        TotalCost = totalCost;
    }
}
