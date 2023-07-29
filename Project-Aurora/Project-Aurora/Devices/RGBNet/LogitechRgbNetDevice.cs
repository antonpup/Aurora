using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using RGB.NET.Core;
using RGB.NET.Devices.Logitech;

namespace Aurora.Devices.RGBNet;

public sealed class LogitechRgbNetDevice : RgbNetDevice, IDisposable
{
    private bool _suspended;

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
        SystemEvents.PowerModeChanged += SystemEventsPowerModeChanged;
        SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
    }

    protected override bool OnShutdown()
    {
        SystemEvents.PowerModeChanged -= SystemEventsPowerModeChanged;

        return true;
    }

    #region Event handlers

    private void SystemEventsOnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (!IsInitialized)
            return;

        if (e.Reason == SessionSwitchReason.SessionUnlock && _suspended)
            Task.Run(async () =>
            {
                // Give LGS a moment to think about its sins
                await Task.Delay(5000);
                _suspended = false;
                await Initialize();
            });
    }

    private async void SystemEventsPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (!IsInitialized)
            return;

        if (e.Mode != PowerModes.Suspend || _suspended) return;
        _suspended = true;
        Shutdown();
    }

    #endregion

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;
    }
}