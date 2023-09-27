using Common.Devices;
using Microsoft.Win32;
using RGB.NET.Core;
using RGB.NET.Devices.Logitech;

namespace AurorDeviceManager.Devices.RGBNet;

public sealed class LogitechRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => LogitechDeviceProvider.Instance;

    public override string DeviceName => "Logitech (RGB.NET)";

    public LogitechRgbNetDevice()
    {
        const string info = "LGS can also work";
        const string sdkLink = "https://www.logitechg.com/en-us/innovation/g-hub.html";
        _tooltips = new DeviceTooltips(false, true, info, sdkLink);
    }

    protected override void OnInitialized()
    {
        SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
    }

    protected override bool OnShutdown()
    {
        SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;

        return true;
    }

    protected override bool IsReversed()
    {
        return true;
    }

    #region Event handlers

    private async void SystemEventsOnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (!IsInitialized)
            return;
        
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
            case SessionSwitchReason.SessionLogoff:
                await Shutdown();
                SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
                break;
            case SessionSwitchReason.SessionLogon:
            case SessionSwitchReason.SessionUnlock:
                await Task.Delay(TimeSpan.FromSeconds(4));
                await Initialize();
                break;
        }
    }

    #endregion
}