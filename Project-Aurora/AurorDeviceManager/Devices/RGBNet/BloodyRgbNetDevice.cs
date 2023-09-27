using RGB.NET.Core;
using RGB.NET.Devices.Bloody;

namespace AurorDeviceManager.Devices.RGBNet;

public class BloodyRgbNetDevice: RgbNetDevice
{

    public override string DeviceName => "Bloody (RGB.NET)";
    protected override IRGBDeviceProvider Provider => BloodyDeviceFactory.Instance;
}