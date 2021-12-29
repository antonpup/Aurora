using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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
                foreach (var sensor in hw.Sensors.OrderBy(s => s.Identifier))
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

        public sealed class Sensor
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }

        public abstract class HardwareUpdater : INotifyPropertyChanged
        {
            protected int maxQueue = 0;
            protected IHardware hw;
            protected bool inUse;

            protected readonly Timer _useTimer; // Check if hw is used
            protected readonly Timer _updateTimer; // Update sensor value
            protected readonly Dictionary<Identifier, Queue<float>> _queues;

            protected HardwareUpdater()
            {
                maxQueue = Global.Configuration.HardwareMonitorMaxQueue;

                _queues = new Dictionary<Identifier, Queue<float>>();

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
                if (sensor is null)
                    return 0;

                inUse = true;

                _useTimer.Stop();
                _useTimer.Start();

                float value = sensor?.Value ?? 0;

                if (!_queues.TryGetValue(sensor.Identifier, out var values))
                    return value;

                // Prevent collection from being modified while enumerating
                lock (values)
                {
                    // Try to fix invalid reading
                    if (value == 0)
                        value = values.LastOrDefault();

                    // Update queue capacity
                    if (maxQueue != Global.Configuration.HardwareMonitorMaxQueue)
                    {
                        maxQueue = Global.Configuration.HardwareMonitorMaxQueue;
                        _queues[sensor.Identifier] = new Queue<float>(maxQueue);
                    }

                    if (values.Count == maxQueue)
                        values.Dequeue();

                    values.Enqueue(value);

                    return Global.Configuration.HardwareMonitorUseAverageValues
                                ? values.Average()
                                : value;
                }
            }

            protected ISensor FindSensor(string identifier)
            {
                var result = hw.Sensors.OrderBy(s => s.Identifier).FirstOrDefault(s => s.Identifier.ToString().Contains(identifier));
                if (result is null)
                {
                    Global.logger.Error(
                        $"[HardwareMonitor] Failed to find sensor \"{identifier}\" in {hw.Name} of type {hw.HardwareType}.");
                    return null;
                }
                result.ValuesTimeWindow = TimeSpan.Zero;
                _queues.Add(result.Identifier, new Queue<float>(maxQueue));
                return result;
            }

            protected ISensor FindSensor(SensorType type)
            {
                var result = hw.Sensors.OrderBy(s => s.Identifier).FirstOrDefault(s => s.SensorType == type);
                if (result is null)
                {
                    Global.logger.Error(
                        $"[HardwareMonitor] Failed to find sensor of type \"{type}\" in {hw.Name} of type {hw.HardwareType}.");
                    return null;
                }
                result.ValuesTimeWindow = TimeSpan.Zero;
                _queues.Add(result.Identifier, new Queue<float>(maxQueue));
                return result;
            }

            protected List<ISensor> FindSensors(SensorType type)
            {
                var result = new List<ISensor>();

                foreach (var sensor in hw.Sensors.Where(s => s.SensorType == type).OrderBy(s => s.Identifier))
                {
                    sensor.ValuesTimeWindow = TimeSpan.Zero;
                    _queues.Add(sensor.Identifier, new Queue<float>(maxQueue));
                    result.Add(sensor);
                }

                if (result.Count == 0)
                {
                    Global.logger.Error(
                        $"[HardwareMonitor] Failed to find sensor of type \"{type}\" in {hw.Name} of type {hw.HardwareType}.");
                    return null;
                }

                return result;
            }

            #region PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;
            protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            #endregion
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
                _GPULoad = FindSensor(SensorType.Load);
                _GPUTemp = FindSensor(SensorType.Temperature);
                _GPUFan = FindSensor(SensorType.Fan);
                _GPUPower = FindSensor(SensorType.Power);
            }
        }

        public sealed class CPUUpdater : HardwareUpdater
        {
            #region Sensors
            private readonly List<ISensor> _CPUTemp;
            public List<Sensor> GetSensorsTemp() => _CPUTemp.ConvertAll(x => new Sensor() { Name = x.Name, Index = x.Index });
            public float CPUTemp => GetValue(_CPUTemp.FirstOrDefault(x => x.Index == Global.Configuration.HardwareMonitorCPUTemperature));

            private readonly List<ISensor> _CPULoad;
            public List<Sensor> GetSensorsLoad() => _CPULoad.ConvertAll(x => new Sensor() { Name = x.Name, Index = x.Index });
            public float CPULoad => GetValue(_CPULoad.FirstOrDefault(x => x.Index == Global.Configuration.HardwareMonitorCPULoad));

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

                _CPUTemp = FindSensors(SensorType.Temperature);
                _CPULoad = FindSensors(SensorType.Load);
                _CPUPower = FindSensor(SensorType.Power);

                _updateTimer.Elapsed += (a, b) =>
                {
                    // To update Aurora GUI In Hardware Monitor tab
                    NotifyPropertyChanged(nameof(CPUTemp));
                    NotifyPropertyChanged(nameof(CPULoad));
                };
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
                _RAMUsed = FindSensor("data/0");
                _RAMFree = FindSensor("data/1");
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

            public NETUpdater(IEnumerable<IHardware> hardware)
            {
                hw = hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Network);
                if (hw is null)
                {
                    Global.logger.Error("[HardwareMonitor] Could not find hardware of type Network");
                    return;
                }
                _BandwidthUsed = FindSensor(SensorType.Load);
                _UploadSpeed = FindSensor("throughput/7");
                _DownloadSpeed = FindSensor("throughput/8");
            }
        }
    }
}
