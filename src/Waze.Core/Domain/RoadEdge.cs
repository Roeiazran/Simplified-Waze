namespace Waze.Core.Domain;

public sealed class RoadEdge
{
    public EdgeId Id { get; }
    public NodeId From { get; }
    public NodeId To { get; }
    public double Length { get; }
    public double SpeedLimit { get; }
    public int Capacity { get; }

    public RoadEdge(EdgeId id, NodeId from, NodeId to, double length, double speedLimit, int capacity)
    {
        Id = id;
        From = from;
        To = to;
        Length = length;
        SpeedLimit = speedLimit;
        Capacity = capacity;
    }
}
