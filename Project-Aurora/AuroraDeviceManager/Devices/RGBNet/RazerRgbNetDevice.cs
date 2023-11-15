using Common.Devices;
using RGB.NET.Core;
using RGB.NET.Devices.Razer;

namespace AuroraDeviceManager.Devices.RGBNet;

public class RazerRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => RazerDeviceProvider.Instance;

    public override string DeviceName => "Razer (RGB.NET)";
}