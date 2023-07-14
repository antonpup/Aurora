using RGB.NET.Core;

namespace Aurora.Devices.RGBNet;

public class AsusDevice : RgbNetDevice
{
    public override string DeviceName => "Asus (RGB.NET)";

    protected override IRGBDeviceProvider Provider => RGB.NET.Devices.Asus.AsusDeviceProvider.Instance;

    public AsusDevice()
    {
        const string sdkLink = "https://www.asus.com/supportonly/armoury%20crate/helpdesk_download/";
        const string info = "Using OpenRGB instead of this is HIGHLY recommended";
        _tooltips = new DeviceTooltips(false, true, info, sdkLink);
    }
}