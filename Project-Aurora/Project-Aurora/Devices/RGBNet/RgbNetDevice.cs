using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using RGB.NET.Core;
using RGB.NET.Devices.Logitech;
using Color = System.Drawing.Color;

namespace Aurora.Devices.RGBNet;

public class RgbNetDevice : DefaultDevice, IDisposable
{
    private bool _suspended;

    public override string DeviceName => "Logitech RGB.NET";
    protected override string DeviceInfo => string.Join(", ", LogitechDeviceProvider.Instance.Devices.Select(d => d.DeviceInfo.DeviceName));
    public override bool Initialize()
    {
        var deviceProvider = LogitechDeviceProvider.Instance;
        List<Exception> providerExceptions = new();
        void DeviceProviderOnException(object? sender, ExceptionEventArgs e)
        {
            if (e.IsCritical)
            {
                providerExceptions.Add(e.Exception);
                Global.logger.Error(e.Exception, "Device provider {deviceProvider} threw critical exception", deviceProvider.GetType().Name);
            }
            else
                Global.logger.Warn(e.Exception, "Device provider {deviceProvider} threw non-critical exception", deviceProvider.GetType().Name);
        }
        
        deviceProvider.Exception += DeviceProviderOnException;
        deviceProvider.Initialize();
        deviceProvider.Exception -= DeviceProviderOnException;
        IsInitialized = deviceProvider.Initialize();
        if (providerExceptions.Count >= 1)
        {
            return false;
        }

        if (!IsInitialized)
        {
            return false;
        }

        SystemEvents.PowerModeChanged += SystemEventsPowerModeChanged;
        SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
        return true;
    }

    public override void Shutdown()
    {
        LogitechDeviceProvider.Instance.Dispose();
        IsInitialized = false;

        SystemEvents.PowerModeChanged -= SystemEventsPowerModeChanged;
    }

    protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        foreach (var device in LogitechDeviceProvider.Instance.Devices)
        {
            foreach (var key in keyColors)
            {
                if (!RgbNetKeyMappings.KeyNames.TryGetValue(key.Key, out var rgbNetLedId)) continue;
                var led = device[rgbNetLedId];
                if (led != null)
                {
                    led.Color = new RGB.NET.Core.Color(
                        key.Value.A,
                        key.Value.R,
                        key.Value.G,
                        key.Value.B
                    );
                }
            }
            device.Update();
        }

        return true;
    }
    #region Event handlers

    private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (!IsInitialized)
            return;

        if (e.Reason == SessionSwitchReason.SessionUnlock && _suspended)
            Task.Run(() =>
            {
                // Give LGS a moment to think about its sins
                Thread.Sleep(5000);
                _suspended = false;
                Initialize();
            });
    }

    private void SystemEventsPowerModeChanged(object sender, PowerModeChangedEventArgs e)
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