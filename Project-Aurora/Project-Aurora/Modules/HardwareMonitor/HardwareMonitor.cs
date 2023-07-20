using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LibreHardwareMonitor.Hardware;

namespace Aurora.Modules.HardwareMonitor;

public interface IHardwareMonitor
{
    HardwareMonitor.GPUUpdater Gpu { get; }
    HardwareMonitor.CPUUpdater Cpu { get; }
    HardwareMonitor.RAMUpdater Ram { get; }
    HardwareMonitor.NETUpdater Net { get; }
}

public class NoopHardwareMonitor : IHardwareMonitor
{
    private readonly Lazy<HardwareMonitor.GPUUpdater> _gpu = new(() => new (new List<IHardware>()));
    private readonly Lazy<HardwareMonitor.CPUUpdater> _cpu = new(() => new (new List<IHardware>()));
    private readonly Lazy<HardwareMonitor.NETUpdater> _net = new(() => new (new List<IHardware>()));
    private readonly Lazy<HardwareMonitor.RAMUpdater> _ram = new(() => new (new List<IHardware>()));

    public HardwareMonitor.GPUUpdater Gpu => _gpu.Value;

    public HardwareMonitor.CPUUpdater Cpu => _cpu.Value;

    public HardwareMonitor.RAMUpdater Ram => _ram.Value;

    public HardwareMonitor.NETUpdater Net => _net.Value;
}

public partial class HardwareMonitor: IHardwareMonitor
{
    private static readonly IEnumerable<IHardware> Hardware;

    private static readonly Lazy<GPUUpdater> _gpu = new(() => new GPUUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    public GPUUpdater Gpu => _gpu.Value;

    private static readonly Lazy<CPUUpdater> _cpu = new(() => new CPUUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    public CPUUpdater Cpu => _cpu.Value;

    private static readonly Lazy<RAMUpdater> _ram = new(() => new RAMUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    public RAMUpdater Ram => _ram.Value;

    private static readonly Lazy<NETUpdater> _net = new(() => new NETUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    public NETUpdater Net => _net.Value;

#pragma warning disable CA1810 // Initialize reference type static fields inline
    static HardwareMonitor()
#pragma warning restore CA1810 // Initialize reference type static fields inline
    {
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsNetworkEnabled = true
        };
        try
        {
            computer.Open();
            Hardware = computer.Hardware;
        }
        catch (Exception e)
        {
            Global.logger.Error("Error instantiating hardware monitor:", e);
        }
    }

    public static bool TryDump()
    {
        var lines = new List<string>();
        foreach (var hw in Hardware)
        {
            lines.Add("-----");
            lines.Add(hw.Name);
            lines.Add("Sensors:");
            lines.AddRange(
                hw.Sensors.OrderBy(s => s.Identifier)
                    .Select(sensor => $"Name: {sensor.Name}, Id: {sensor.Identifier}, Type: {sensor.SensorType}"));
            lines.Add("-----");
        }
        try
        {
            File.WriteAllLines(Path.Combine(Global.LogsDirectory, "sensors.txt"), lines);
            return true;
        }
        catch (IOException e)
        {
            Global.logger.Error("Failed to write sensors dump: " + e);
            return false;
        }
    }

    public sealed class Sensor
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }
}