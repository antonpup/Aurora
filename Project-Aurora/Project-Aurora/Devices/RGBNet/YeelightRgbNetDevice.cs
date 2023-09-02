using System.Threading.Tasks;
using Aurora.Settings;
using RGB.NET.Core;
using RGB.NET.YeeLightStates;

namespace Aurora.Devices.RGBNet;

public class YeelightRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => YeelightProvider.Instance;

    public override string DeviceName => "Yeelight (RGB.NET)";

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);

        variableRegistry.Register($"{DeviceName}_IP", "", "YeeLight IP(s)",
            null, null, "Comma separated IPv4 or IPv6 addresses.");
        variableRegistry.Register($"{DeviceName}_auto_discovery", false, "Auto-discovery",
            null, null, "Enable this and empty out the IP field to auto-discover lights.");
        variableRegistry.Register($"{DeviceName}_music_mode_only", false, "Music Mode Only",
            null, null, "Only connects as music mode, making sure all devices update fast");
    }

    protected override async Task ConfigureProvider()
    {
        await base.ConfigureProvider();

        var autoDiscovery = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_auto_discovery");
        if (autoDiscovery)
        {
            YeelightProvider.IpAddresses = string.Empty;
        }
        else
        {
            var ipAddresses = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_IP");
            YeelightProvider.IpAddresses = ipAddresses;
        }
        
    }
}