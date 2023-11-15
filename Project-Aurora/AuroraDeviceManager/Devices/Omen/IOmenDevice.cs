using System.Drawing;
using Common.Devices;

namespace AuroraDeviceManager.Devices.Omen
{
    interface IOmenDevice
    {
        public void Shutdown();
        public void SetLights(Dictionary<DeviceKeys, Color> keyColors);
        public string GetDeviceName();
    };
}
