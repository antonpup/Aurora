using System.Linq;
using Aurora.Devices.RGBNet;
using RGB.NET.Core;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : RgbNetDevice
    {
        /// <inheritdoc />
        public override string DeviceName => "Asus (RGB.NET)";

        /// <inheritdoc />
        public override string DeviceDetails => GetDeviceStatus();

        protected override IRGBDeviceProvider Provider => RGB.NET.Devices.Asus.AsusDeviceProvider.Instance;

        private string GetDeviceStatus()
        {
            if (!IsInitialized)
                return "Not Initialized";

            if (!Provider.Devices.Any())
                return "Initialized: No devices connected";

            return "Initialized: " + Provider.Devices.Select(device => device.DeviceInfo.DeviceName);
        }
    }
}
