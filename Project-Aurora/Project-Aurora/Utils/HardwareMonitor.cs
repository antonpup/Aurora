using System;
using System.Collections.Generic;
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

        private static ISensor FindSensor(this IHardware hardware, string identifier)
        {
            var result = Array.Find(hardware.Sensors, s => s.Identifier.ToString().Contains(identifier));
            if (result is null)
            {
                Global.logger.Error(
                    $"[HardwareMonitor] Failed to find sensor \"{identifier}\" in {hardware.Name} of type {hardware.HardwareType}.");
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
                        hw.Update();
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

            public void SetUpdateTimer(int interval)
            {
                _updateTimer.Interval = interval;
                _updateTimer.Stop();
                _updateTimer.Start();
            }
        }

        public sealed class GPUUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _GPUCoreTemp;
            public float GPUCoreTemp => GetValue(_GPUCoreTemp);

            private readonly ISensor _GPUFan;
            public float GPUFan => GetValue(_GPUFan);

            private readonly ISensor _GPUCoreClock;
            public float GPUCoreClock => GetValue(_GPUCoreClock);

            private readonly ISensor _GPUMemoryClock;
            public float GPUMemoryClock => GetValue(_GPUMemoryClock);

            private readonly ISensor _GPUShaderClock;
            public float GPUShaderClock => GetValue(_GPUShaderClock);

            private readonly ISensor _GPUCoreLoad;
            public float GPUCoreLoad => GetValue(_GPUCoreLoad);

            private readonly ISensor _GPUMemoryCLoad;
            public float GPUMemoryCLoad => GetValue(_GPUMemoryCLoad);

            private readonly ISensor _GPUVideoEngineLoad;
            public float GPUVideoEngineLoad => GetValue(_GPUVideoEngineLoad);

            private readonly ISensor _GPUMemoryTotal;
            public float GPUMemoryTotal => GetValue(_GPUMemoryTotal);

            private readonly ISensor _GPUMemoryUsed;
            public float GPUMemoryUsed => GetValue(_GPUMemoryUsed);

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
                _GPUCoreLoad = hw.FindSensor("load/0");
                _GPUMemoryCLoad = hw.FindSensor("load/1");
                _GPUVideoEngineLoad = hw.FindSensor("load/2");

                _GPUCoreClock = hw.FindSensor("clock/0");
                _GPUMemoryClock = hw.FindSensor("clock/1");
                _GPUShaderClock = hw.FindSensor("clock/2");

                _GPUCoreTemp = hw.FindSensor("temperature/0");
                _GPUFan = hw.FindSensor("fan/0");
                _GPUPower = hw.FindSensor("power/0");

                _GPUMemoryTotal = hw.FindSensor("smalldata/3");
                _GPUMemoryUsed = hw.FindSensor("smalldata/2");
            }
        }

        public sealed class CPUUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _CPUDieTemp;
            public float CPUDieTemp => GetValue(_CPUDieTemp);

            private readonly ISensor _CPUTotalLoad;
            public float CPUTotalLoad => GetValue(_CPUTotalLoad);

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
                _CPUDieTemp = hw.FindSensor("temperature/0");
                _CPUTotalLoad = hw.FindSensor("load/0");
                _CPUPower = hw.FindSensor("power/0");
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
                _BandwidthUsed = hw.FindSensor("load");
                _UploadSpeed = hw.FindSensor("throughput/7");
                _DownloadSpeed = hw.FindSensor("throughput/8");
            }
        }
    }
}
