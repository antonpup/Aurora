using Common.Devices;
using RGB.NET.Core;
using RGB.NET.Devices.Razer;

namespace AurorDeviceManager.Devices.RGBNet;

public class RazerRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => RazerDeviceProvider.Instance;

    public override string DeviceName => "Razer (RGB.NET)";

    public RazerRgbNetDevice()
    {
        const string info = "You can install chroma inside \"Devices & Wrappers\" -> Chroma and use this";
        _tooltips = new DeviceTooltips(false, true, info, null);
    }
}