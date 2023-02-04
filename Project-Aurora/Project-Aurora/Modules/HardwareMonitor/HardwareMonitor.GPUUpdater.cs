using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Aurora.Modules.HardwareMonitor;

public partial class HardwareMonitor
{
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
                Global.logger.Error("[HardwareMonitor] Could not find hardware of type GPU or hardware monitoring is disabled");
                return;
            }
            _GPULoad = FindSensor(SensorType.Load);
            _GPUTemp = FindSensor(SensorType.Temperature);
            _GPUFan = FindSensor(SensorType.Fan);
            _GPUPower = FindSensor(SensorType.Power);
        }
    }
}