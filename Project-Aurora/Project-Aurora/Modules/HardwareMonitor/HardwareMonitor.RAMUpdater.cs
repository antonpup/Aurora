using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Aurora.Modules.HardwareMonitor;

public partial class HardwareMonitor
{
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
                Global.logger.Error("[HardwareMonitor] Could not find hardware of type RAM or hardware monitoring is disabled");
                return;
            }
            _RAMUsed = FindSensor("data/0");
            _RAMFree = FindSensor("data/1");
        }
    }
}