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
        public static GPUUpdater GPU => _gpu ?? (_gpu = new GPUUpdater(_hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.GpuAmd|| hw.HardwareType == HardwareType.GpuNvidia)));

        private static CPUUpdater _cpu;
        public static CPUUpdater CPU => _cpu ?? (_cpu = new CPUUpdater(_hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Cpu)));

        private static RAMUpdater _ram;
        public static RAMUpdater RAM => _ram ?? (_ram = new RAMUpdater(_hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Memory)));

        static HardwareMonitor()
        {
            _computer = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true
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
            return Array.Find(hardware.Sensors, s => s.Identifier.ToString().Contains(identifier));
        }

        public abstract class HardwareUpdater
        {
            protected IHardware hw;
            protected bool inUse;
            private readonly Timer _useTimer = new Timer(5000);
            private readonly Timer _updateTimer = new Timer(200);

            protected HardwareUpdater()
            {
                _useTimer.Elapsed += (a, b) =>
                {
                    inUse = false;
                    _useTimer.Stop();
                };
                _updateTimer.Elapsed += (a, b) =>
                {
                    if (inUse)
                        hw.Update();
                };
                _updateTimer.Start();
                _useTimer.Start();
            }

            protected void ResetTimer()
            {
                inUse = true;
                _useTimer.Stop();
                _useTimer.Start();
            }
        }

        public class GPUUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _GPUCoreTemp;
            public float GPUCoreTemp { get { inUse = true; return _GPUCoreTemp.Value ?? 0; } }

            private readonly ISensor _GPUFan;
            public float GPUFan { get { inUse = true; return _GPUFan.Value ?? 0; } }

            private readonly ISensor _GPUCoreClock;
            public float GPUCoreClock { get { inUse = true; return _GPUCoreClock.Value ?? 0; } }

            private readonly ISensor _GPUMemoryClock;
            public float GPUMemoryClock { get { inUse = true; return _GPUMemoryClock.Value ?? 0; } }

            private readonly ISensor _GPUShaderClock;
            public float GPUShaderClock { get { inUse = true; return _GPUShaderClock.Value ?? 0; } }

            private readonly ISensor _GPUCoreLoad;
            public float GPUCoreLoad { get { inUse = true; return _GPUCoreLoad.Value ?? 0; } }

            private readonly ISensor _GPUMemoryCLoad;
            public float GPUMemoryCLoad { get { inUse = true; return _GPUMemoryCLoad.Value ?? 0; } }

            private readonly ISensor _GPUVideoEngineLoad;
            public float GPUVideoEngineLoad { get { inUse = true; return _GPUVideoEngineLoad.Value ?? 0; } }

            private readonly ISensor _GPUMemoryTotal;
            public float GPUMemoryTotal { get { inUse = true; return _GPUMemoryTotal.Value ?? 0; } }

            private readonly ISensor _GPUMemoryUsed;
            public float GPUMemoryUsed { get { inUse = true; return _GPUMemoryUsed.Value ?? 0; } }

            private readonly ISensor _GPUPower;
            public float GPUPower { get { inUse = true; return _GPUPower.Value ?? 0; } }
            #endregion

            public GPUUpdater(IHardware hardware)
            {
                hw = hardware;
                if (hw != null)
                {
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
        }

        public class CPUUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _CPUDieTemp;
            public float CPUDieTemp { get { ResetTimer(); return _CPUDieTemp.Value ?? 0; } }

            private readonly ISensor _CPUTotalLoad;
            public float CPUTotalLoad { get { ResetTimer(); return _CPUTotalLoad.Value ?? 0; } }

            private readonly ISensor _CPUPower;
            public float CPUPower { get { ResetTimer(); return _CPUPower.Value ?? 0; } }
            #endregion

            public CPUUpdater(IHardware hardware)
            {
                hw = hardware;
                if (hw != null)
                {
                    _CPUDieTemp = hw.FindSensor("temperature/0");
                    _CPUTotalLoad = hw.FindSensor("load/0");
                    _CPUPower = hw.FindSensor("power/0");
                }
            }
        }

        public class RAMUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly ISensor _RAMUsed;
            public float RAMUsed { get { inUse = true; return _RAMUsed.Value ?? 0; } }

            private readonly ISensor _RAMFree;
            public float RAMFree { get { inUse = true; return _RAMFree.Value ?? 0; } }
            #endregion

            public RAMUpdater(IHardware hardware)
            {
                hw = hardware;
                if (hw != null)
                {
                    _RAMUsed = hw.FindSensor("data/0");
                    _RAMFree = hw.FindSensor("data/1");
                }
            }
        }
    }
}
