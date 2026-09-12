namespace Waze.Simulation.Traffic;

public sealed class EdgeTrafficState
{
    public int VehicleCount { get; private set; }
    public bool IsOpen { get; private set; } = true;

    public void IncrementVehicleCount()
    {
        VehicleCount++;
    }

    public void DecrementVehicleCount()
    {
        VehicleCount = Math.Max(0, VehicleCount - 1);
    }
}
