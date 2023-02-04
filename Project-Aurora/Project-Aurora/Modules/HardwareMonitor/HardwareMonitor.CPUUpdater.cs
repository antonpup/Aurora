using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Aurora.Modules.HardwareMonitor;

public partial class HardwareMonitor
{
    public sealed class CPUUpdater : HardwareUpdater
    {
        #region Sensors
        private readonly List<ISensor> _CPUTemp;
        public float CPUTemp => GetValue(_CPUTemp.FirstOrDefault(x => x.Index == Global.Configuration.HardwareMonitorCPUTemperature));

        private readonly List<ISensor> _CPULoad;
        public float CPULoad => GetValue(_CPULoad.FirstOrDefault(x => x.Index == Global.Configuration.HardwareMonitorCPULoad));

        private readonly ISensor _CPUPower;
        public float CPUPower => GetValue(_CPUPower);
        #endregion

        public CPUUpdater(IEnumerable<IHardware> hardware)
        {
            hw = hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Cpu);
            if (hw is null)
            {
                Global.logger.Error("[HardwareMonitor] Could not find hardware of type CPU or hardware monitoring is disabled");
                _CPUTemp = new();
                _CPULoad = new();
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
}