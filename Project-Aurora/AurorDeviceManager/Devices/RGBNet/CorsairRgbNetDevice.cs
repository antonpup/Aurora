using Common.Utils;
using AurorDeviceManager.Settings;
using AurorDeviceManager.Utils;
using Common.Devices;
using RGB.NET.Core;
using RGB.NET.Devices.CorsairLegacy;

namespace AurorDeviceManager.Devices.RGBNet;

public class CorsairRgbNetDevice : RgbNetDevice
{
    protected override CorsairLegacyDeviceProvider Provider => CorsairLegacyDeviceProvider.Instance;

    public override string DeviceName => "Corsair (RGB.NET)";

    public CorsairRgbNetDevice()
    {
        const string sdkLink = "https://www.corsair.com/ww/en/s/downloads";
        _tooltips = new DeviceTooltips(false, false, null, sdkLink);
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);

        variableRegistry.Register($"{DeviceName}_exclusive", false, "Request exclusive control");
    }

    protected override bool OnShutdown()
    {
        base.OnShutdown();

        return !App.Closing;
    }

    protected override async Task ConfigureProvider()
    {
        await base.ConfigureProvider();

        var waitSessionUnlock = await DesktopUtils.WaitSessionUnlock();
        if (waitSessionUnlock)
        {
            Global.Logger.Information("Waiting for Corsair to load after unlock");
            await Task.Delay(5000);
        }
        
        var isIcueRunning = ProcessUtils.IsProcessRunning("icue");
        if (!isIcueRunning)
        {
            throw new DeviceProviderException(new ApplicationException("iCUE is not running!"), false);
        }

        var exclusive = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_exclusive");

        //Provider.ExclusiveAccess = exclusive;
        //Provider.ConnectionTimeout = new TimeSpan(0, 0, 5);

        //Provider.SessionStateChanged += SessionStateChanged;
    }

    /*
    private void SessionStateChanged(object? sender, CorsairSessionState e)
    {
        if (e != CorsairSessionState.Closed) return;
        Provider.SessionStateChanged -= SessionStateChanged;

        IsInitialized = false;
    }
    */
}