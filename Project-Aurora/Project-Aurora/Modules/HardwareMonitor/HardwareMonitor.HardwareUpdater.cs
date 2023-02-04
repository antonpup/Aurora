using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using LibreHardwareMonitor.Hardware;

namespace Aurora.Modules.HardwareMonitor;

public partial class HardwareMonitor
{
    public abstract class HardwareUpdater : INotifyPropertyChanged
    {
        protected int maxQueue = 0;
        protected IHardware hw;
        protected bool inUse;

        private Stopwatch _lastUsage = Stopwatch.StartNew();
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
                if (_lastUsage.Elapsed.Seconds <= 5) return;
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

            if (!inUse)
            {
                _useTimer.Start();
            }
            inUse = true;
            _lastUsage.Reset();

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
}