using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Aurora.Modules.HardwareMonitor;

public partial class HardwareMonitor
{
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
                Global.logger.Error("[HardwareMonitor] Could not find hardware of type Network or hardware monitoring is disabled");
                return;
            }
            _BandwidthUsed = FindSensor(SensorType.Load);
            _UploadSpeed = FindSensor("throughput/7");
            _DownloadSpeed = FindSensor("throughput/8");
        }
    }
}