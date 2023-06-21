using System;
using Aurora.Settings;
using Aurora.Utils;
using RGB.NET.Devices.Corsair;

namespace Aurora.Devices.RGBNet;

public class CorsairRgbNetDevice : RgbNetDevice
{
    protected override CorsairDeviceProvider Provider => CorsairDeviceProvider.Instance;

    public override string DeviceName => "Corsair (RGB.NET)";

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);
        
        variableRegistry.Register($"{DeviceName}_exclusive", false, "Request exclusive control");
    }

    protected override void ConfigureProvider()
    {
        base.ConfigureProvider();

        DesktopUtils.WaitSessionUnlock();

        var exclusive = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_exclusive");

        CorsairDeviceProvider.ExclusiveAccess = exclusive;
        CorsairDeviceProvider.ConnectionTimeout = new TimeSpan(0, 0,5);
        
        Provider.SessionStateChanged += SessionStateChanged;
    }

    private void SessionStateChanged(object? sender, CorsairSessionState e)
    {
        Provider.SessionStateChanged -= SessionStateChanged;

        IsInitialized = false;
    }
}