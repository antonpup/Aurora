using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LibreHardwareMonitor.Hardware;
using Computer = LibreHardwareMonitor.Hardware.Computer;

namespace Aurora.Utils
{
    public static class HardwareMonitor
    {
        private static readonly Computer _computer;
        private static readonly IEnumerable<IHardware> _hardware;

        private static GPUUpdater _gpu;
        public static GPUUpdater GPU => _gpu ?? (_gpu = new GPUUpdater(_hardware));

        private static CPUUpdater _cpu;
        public static CPUUpdater CPU => _cpu ?? (_cpu = new CPUUpdater(_hardware));

        private static RAMUpdater _ram;
        public static RAMUpdater RAM => _ram ?? (_ram = new RAMUpdater(_hardware));

        private static NETUpdater _net;
        public static NETUpdater NET => _net ?? (_net = new NETUpdater(_hardware));

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static HardwareMonitor()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
            _computer = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsNetworkEnabled = true
            };
            try
            {
                _computer.Open();
                _hardware = _computer.Hardware;
            }
            catch (Exception e)
            {
                Global.logger.Error(e);
            }
        }

        public static bool TryDump()
        {
            var lines = new List<string>();
            foreach (var hw in _hardware)
            {
                lines.Add("-----");
                lines.Add(hw.Name);
                lines.Add("Sensors:");
                foreach (var sensor in hw.Sensors.OrderBy(s => s.SensorType))
                {
                    lines.Add($"Name: {sensor.Name}, Id: {sensor.Identifier}, Type: {sensor.SensorType}");
                }
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

        private static ISensor FindSensor(this IHardware hardware, string identifier)
        {
            var result = hardware.Sensors.OrderBy(s => s.Identifier).FirstOrDefault(s => s.Identifier.ToString().Contains(identifier));
            if (result is null)
            {
                Global.logger.Error(
                    $"[HardwareMonitor] Failed to find sensor \"{identifier}\" in {hardware.Name} of type {hardware.HardwareType}.");
            }
            return result;
        }

        private static ISensor FindSensor(this IHardware hardware, SensorType type)
        {
            var result = hardware.Sensors.OrderBy(s => s.Identifier).FirstOrDefault(s => s.SensorType == type);
            if (result is null)
            {
                Global.logger.Error(
                    $"[HardwareMonitor] Failed to find sensor of type \"{type}\" in {hardware.Name} of type {hardware.HardwareType}.");
            }
            return result;
        }

        public abstract class HardwareUpdater
        {
            protected IHardware hw;
            protected bool inUse;

            private readonly Timer _useTimer;
            private readonly Timer _updateTimer;

            protected HardwareUpdater()
            {
                _useTimer = new Timer(5000);
                _useTimer.Elapsed += (a, b) =>
                {
                    inUse = false;
                    _useTimer.Stop();
                };
                _useTimer.Start();

                _updateTimer = new Timer(Global.Configuration.HardwareMonitorUpdateRate);
                _updateTimer.Elapsed += (a, b) =>
                {
                    if (inUse)
                        hw?.Update();
                    if (_updateTimer.Interval != Global.Configuration.HardwareMonitorUpdateRate)
                        _updateTimer.Interval = Global.Configuration.HardwareMonitorUpdateRate;
                };
                _updateTimer.Start();
            }

            protected float GetValue(ISensor sensor)
            {
                inUse = true;
                _useTimer.Stop();
                _useTimer.Start();

                return sensor?.Value ?? 0;
            }
        }

        public sealed class GPUUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _GPUTemp;
            public float GPUCoreTemp => GetValue(_GPUTemp);

            private readonly ISensor _GPUFan;
            public float GPUFan => GetValue(_GPUFan);

            private readonly ISensor _GPULoad;
            public float GPULoad => GetValue(_GPULoad);

            private readonly ISensor _GPUPower;
            public float GPUPower => GetValue(_GPUPower);
            #endregion

            public GPUUpdater(IEnumerable<IHardware> hardware)
            {
                hw = hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.GpuAmd ||
                                                   hw.HardwareType == HardwareType.GpuNvidia);
                if (hw is null)
                {
                    Global.logger.Error("[HardwareMonitor] Could not find hardware of type GPU");
                    return;
                }
                _GPULoad = hw.FindSensor(SensorType.Load);
                _GPUTemp = hw.FindSensor(SensorType.Temperature);
                _GPUFan = hw.FindSensor(SensorType.Fan);
                _GPUPower = hw.FindSensor(SensorType.Power);
            }
        }

        public sealed class CPUUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _CPUTemp;
            public float CPUTemp => GetValue(_CPUTemp);

            private readonly ISensor _CPULoad;
            public float CPULoad => GetValue(_CPULoad);

            private readonly ISensor _CPUPower;
            public float CPUPower => GetValue(_CPUPower);
            #endregion

            public CPUUpdater(IEnumerable<IHardware> hardware)
            {
                hw = hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Cpu);
                if (hw is null)
                {
                    Global.logger.Error("[HardwareMonitor] Could not find hardware of type CPU");
                    return;
                }
                _CPUTemp = hw.FindSensor(SensorType.Temperature);
                _CPULoad = hw.FindSensor(SensorType.Load);
                _CPUPower = hw.FindSensor(SensorType.Power);
            }
        }

        public sealed class RAMUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _RAMUsed;
            public float RAMUsed => GetValue(_RAMUsed);

            private readonly ISensor _RAMFree;
            public float RAMFree => GetValue(_RAMFree);
            #endregion

            public RAMUpdater(IEnumerable<IHardware> hws)
            {
                hw = hws.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
                if (hw is null)
                {
                    Global.logger.Error("[HardwareMonitor] Could not find hardware of type RAM");
                    return;
                }
                _RAMUsed = hw.FindSensor("data/0");
                _RAMFree = hw.FindSensor("data/1");
            }
        }

        public sealed class NETUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _BandwidthUsed;
            public float BandwidthUsed => GetValue(_BandwidthUsed);

            private readonly ISensor _UploadSpeed;
            public float UploadSpeedBytes => GetValue(_UploadSpeed);

            private readonly ISensor _DownloadSpeed;
            public float DownloadSpeedBytes => GetValue(_DownloadSpeed);
            #endregion

            public NETUpdater(IEnumerable<IHardware> hws)
            {
                hw = hws.FirstOrDefault(h => h.HardwareType == HardwareType.Network);
                if (hw is null)
                {
                    Global.logger.Error("[HardwareMonitor] Could not find hardware of type Network");
                    return;
                }
                _BandwidthUsed = hw.FindSensor(SensorType.Load);
                _UploadSpeed = hw.FindSensor("throughput/7");
                _DownloadSpeed = hw.FindSensor("throughput/8");
            }
        }
    }
}
