using RGB.NET.Core;
using RGB.NET.Devices.Wooting;

namespace AuroraDeviceManager.Devices.RGBNet;

public class WootingRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => WootingDeviceProvider.Instance;

    public override string DeviceName => "Wooting (RGB.NET)";
}