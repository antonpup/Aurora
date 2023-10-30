namespace Aurora.Nodes;

public class CPUInfo : Node
{
    /// <summary>
    /// Represents the CPU usage from 0 to 100
    /// </summary>
    public float Usage => LocalPcInformation.HardwareMonitor.Cpu.CPULoad;

    /// <summary>
    /// Represents the temperature of the cpu die in celsius
    /// </summary>
    public float Temperature => LocalPcInformation.HardwareMonitor.Cpu.CPUTemp;

    /// <summary>
    /// Represents the CPU power draw in watts
    /// </summary>
    public float PowerUsage => LocalPcInformation.HardwareMonitor.Cpu.CPUPower;
}