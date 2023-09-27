using RGB.NET.Core;
using RGB.NET.Devices.Wooting;

namespace AurorDeviceManager.Devices.RGBNet;

public class WootingRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => WootingDeviceProvider.Instance;

    public override string DeviceName => "Wooting (RGB.NET)";
}