namespace Aurora.Nodes;

public class RAMInfo : Node
{
    /// <summary>
    /// Used system memory in megabytes
    /// </summary>
    public long Used => (long)(LocalPcInformation.HardwareMonitor.Ram.RAMUsed * 1024f);

    /// <summary>
    /// Free system memory in megabytes
    /// </summary>
    public long Free => (long)(LocalPcInformation.HardwareMonitor.Ram.RAMFree * 1024f);

    /// <summary>
    /// Total system memory in megabytes
    /// </summary>
    public long Total => Free + Used;
}