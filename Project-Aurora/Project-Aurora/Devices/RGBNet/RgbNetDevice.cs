using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Aurora.Devices.RGBNet.Config;
using RGB.NET.Core;
using RGB.NET.Devices.Logitech;
using Color = System.Drawing.Color;

namespace Aurora.Devices.RGBNet;

public abstract class RgbNetDevice : DefaultDevice
{
    protected abstract IRGBDeviceProvider Provider { get; }
    public Dictionary<IRGBDevice, Dictionary<LedId, DeviceKeys>> DeviceKeyRemap { get; } = new();

    public IEnumerable<IRGBDevice> Devices()
    {
        return Provider.Devices;
    }

    public override bool Initialize()
    {
        List<Exception> providerExceptions = new();
        void DeviceProviderOnException(object sender, ExceptionEventArgs e)
        {
            if (e.IsCritical)
            {
                providerExceptions.Add(e.Exception);
                Global.logger.Error(e.Exception, "Device provider {DeviceProvider} threw critical exception", Provider.GetType().Name);
            }
            else
                Global.logger.Warn(e.Exception, "Device provider {DeviceProvider} threw non-critical exception", Provider.GetType().Name);
        }
        
        Provider.Exception += DeviceProviderOnException;
        IsInitialized = Provider.Initialize();
        Provider.Exception -= DeviceProviderOnException;
        if (providerExceptions.Count >= 1)
        {
            return false;
        }

        if (!IsInitialized)
        {
            return false;
        }

        var rgbNetConfigDevices = DeviceMappingConfig.Config.Devices.ToDictionary(device => device.Name, device => device);
        foreach (var rgbDevice in Devices())
        {
            if (rgbNetConfigDevices.TryGetValue(rgbDevice.DeviceInfo.DeviceName, out var configDevice))
            {
                DeviceKeyRemap.Add(rgbDevice, configDevice.KeyMapper);
            }
        }
        
        OnInitialized();
        return true;
    }

    public override void Shutdown()
    {
        OnShutdown();
        LogitechDeviceProvider.Instance.Dispose();
        IsInitialized = false;
    }
    protected virtual void OnInitialized()
    {
    }

    protected virtual void OnShutdown()
    {
    }

    protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        foreach (var device in Provider.Devices)
        {
            foreach (var led in device)
            {
                DeviceKeyRemap.TryGetValue(device, out var keyRemap);
                if (!(keyRemap != null && keyRemap.TryGetValue(led.Id, out var dk)) &&  //get remapped key if device if remapped
                    !RgbNetKeyMappings.KeyNames.TryGetValue(led.Id, out dk)) continue;
                if (!keyColors.TryGetValue(dk, out var color)) continue;
                led.Color = new RGB.NET.Core.Color(
                    color.A,
                    color.R,
                    color.G,
                    color.B
                );
            }
            device.Update();
        }

        return true;
    }
}