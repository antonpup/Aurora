using AurorDeviceManager.Utils;
using Common.Devices;
using RGB.NET.Core;

namespace AurorDeviceManager.Devices.RGBNet;

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

    protected override Task ConfigureProvider()
    {
        base.ConfigureProvider();
        
        var isAuraRunning = ProcessUtils.IsProcessRunning("lightingservice");
        if (!isAuraRunning)
        {
            throw new DeviceProviderException(new ApplicationException("Aura or Armory Crate is not running! (LightingService.exe)"), false);
        }

        return Task.CompletedTask;
    }
}