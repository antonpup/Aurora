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
    public static class LibreHardwareMonitor
    {
        private static readonly Computer _computer;
        private static readonly IEnumerable<IHardware> _hardware;
        private static readonly IHardware _CPU;
        private static readonly IHardware _GPU;
        private static readonly IHardware _MB;
        private static readonly IHardware _RAM;

        #region GPU Sensors
        public static readonly ISensor GPUCoreTemp;
        public static readonly ISensor GPUFan;
        public static readonly ISensor GPUCoreClock;
        public static readonly ISensor GPUMemoryClock;
        public static readonly ISensor GPUShaderClock;
        public static readonly ISensor GPUCoreLoad;
        public static readonly ISensor GPUMemoryCLoad;
        public static readonly ISensor GPUVideoEngineLoad;
        public static readonly ISensor GPUMemoryTotal;
        public static readonly ISensor GPUMemoryUsed;
        public static readonly ISensor GPUPower;
        #endregion

        #region CPU Sensors
        public static readonly ISensor CPUDieTemp;
        public static readonly ISensor CPUTotalLoad;
        public static readonly ISensor CPUPower;
        public static readonly ISensor CPUFan;
        #endregion

        #region RAM Sensors
        public static readonly ISensor RAMUsed;
        public static readonly ISensor RAMFree;
        #endregion

        private static int _interval = 200;
        private static readonly Timer _updateTimer;
        public static int Interval
        {
            get => _interval;
            set {
                _interval = value;
                _updateTimer.Interval = _interval;
            }
        }

        static LibreHardwareMonitor()
        {
            try
            {
                _updateTimer = new Timer(_interval);
                _updateTimer.Elapsed += UpdateAll;
                _computer = new Computer()
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsMotherboardEnabled = true
                };
                _computer.Open();
                _hardware = _computer.Hardware;
                UpdateAll(null, null);

                _CPU = _hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Cpu);
                if(_CPU != null)
                {
                    CPUDieTemp = _CPU.FindSensor("temperature/0");
                    CPUTotalLoad = _CPU.FindSensor("load/0");
                    CPUPower = _CPU.FindSensor("power/0");
                }

                _GPU = _hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.GpuAmd || hw.HardwareType == HardwareType.GpuNvidia);
                if(_GPU != null)
                {
                    GPUCoreLoad = _GPU.FindSensor("load/0");
                    GPUMemoryCLoad = _GPU.FindSensor("load/1");
                    GPUVideoEngineLoad = _GPU.FindSensor("load/2");

                    GPUCoreClock = _GPU.FindSensor("clock/0");
                    GPUMemoryClock = _GPU.FindSensor("clock/1");
                    GPUShaderClock = _GPU.FindSensor("clock/2");

                    GPUCoreTemp = _GPU.FindSensor("temperature/0");
                    GPUFan = _GPU.FindSensor("fan/0");
                    GPUPower = _GPU.FindSensor("power/0");

                    GPUMemoryTotal = _GPU.FindSensor("smalldata/3");
                    GPUMemoryUsed = _GPU.FindSensor("smalldata/2");
               }

                _MB = _hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Motherboard)?.SubHardware[0];
                if(_MB != null)
                {
                    CPUFan = _MB.FindSensor("fan/1");
                }

                _RAM = _hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Memory);
                if(_RAM != null)
                {
                    RAMUsed = _RAM.FindSensor("data/0");
                    RAMFree = _RAM.FindSensor("data/1");
                }

                _updateTimer.Start();
            }
            catch (Exception e)
            {
                Global.logger.Error("Error initializing hardware monitor: " + e);
            }
        }

        private static void UpdateAll(object sender, ElapsedEventArgs e)
        {
            foreach (var hw in _hardware)
            {
                hw.Update();
                foreach (var sub in hw.SubHardware)
                    sub.Update();
            }
        }

        private static ISensor FindSensor(this IHardware hardware, string identifier)
        {
            return Array.Find(hardware.Sensors, s => s.Identifier.ToString().Contains(identifier));
        }
    }
}
