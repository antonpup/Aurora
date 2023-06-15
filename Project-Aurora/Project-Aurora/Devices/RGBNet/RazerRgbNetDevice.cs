using RGB.NET.Core;
using RGB.NET.Devices.Razer;

namespace Aurora.Devices.RGBNet;

public class RazerRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => RazerDeviceProvider.Instance;

    public override string DeviceName => "Razer (RGB.NET)";
}