namespace Aurora.Nodes;

public class GPUInfo : Node
{
    public float Usage => LocalPcInformation.HardwareMonitor.Gpu.GPULoad;
    public float Temperature => LocalPcInformation.HardwareMonitor.Gpu.GPUCoreTemp;
    public float PowerUsage => LocalPcInformation.HardwareMonitor.Gpu.GPUPower;
    public float FanRPM => LocalPcInformation.HardwareMonitor.Gpu.GPUFan;
}