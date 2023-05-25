using RGB.NET.Core;

namespace Aurora.Devices.RGBNet;

public class AsusDevice : RgbNetDevice
{
    public override string DeviceName => "Asus (RGB.NET)";

    protected override IRGBDeviceProvider Provider => RGB.NET.Devices.Asus.AsusDeviceProvider.Instance;
}