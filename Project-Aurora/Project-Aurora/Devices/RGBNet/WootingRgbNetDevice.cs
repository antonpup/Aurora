using RGB.NET.Core;
using RGB.NET.Devices.Razer;
using RGB.NET.Devices.Wooting;

namespace Aurora.Devices.RGBNet;

public class WootingRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => WootingDeviceProvider.Instance;

    public override string DeviceName => "Wooting (RGB.NET)";
}