using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Aurora.Devices.RGBNet.Config;
using Aurora.Settings;
using RGB.NET.Core;
using Color = System.Drawing.Color;

namespace Aurora.Devices.RGBNet;

public abstract class RgbNetDevice : DefaultDevice
{
    public bool Disabled { get; set; }
    protected abstract IRGBDeviceProvider Provider { get; }
    public ConcurrentDictionary<IRGBDevice, Dictionary<LedId, DeviceKeys>> DeviceKeyRemap { get; } = new();
    protected string? ErrorMessage { get; set; }
    protected override string DeviceInfo => ErrorMessage ?? GetDeviceStatus();

    protected readonly IDictionary<string, int> DeviceCountMap = new Dictionary<string, int>();

    private readonly List<IRGBDevice> _deviceList = new();

    public IEnumerable<IRGBDevice> Devices { get; }

    protected RgbNetDevice()
    {
        Devices = _deviceList.AsReadOnly();
    }

    private string GetDeviceStatus()
    {
        if (!IsInitialized)
            return "";

        if (!Provider.Devices.Any())
            return "Initialized: No devices connected";

        return "Initialized: " + string.Join(", ", DeviceCountMap.Select(pair => pair.Value > 1 ? pair.Key + " x" + pair.Value : pair.Key));
    }

    public override IEnumerable<string> GetDevices()
    {
        return Devices.Select(CalibrationName);
    }

    protected override async Task<bool> DoInitialize()
    {
        Global.logger.Information("Initializing {DeviceName}", DeviceName);
        List<Exception> providerExceptions = new();
        List<Exception> criticalExceptions = new();

        void DeviceProviderOnException(object? sender, ExceptionEventArgs e)
        {
            Global.logger.Error(e.Exception, "Device provider {DeviceProvider} threw exception", DeviceName);
            ErrorMessage = e.Exception.Message;
            providerExceptions.Add(e.Exception);

            if (e.IsCritical)
            {
                criticalExceptions.Add(e.Exception);
            }
        }

        try
        {
            await ConfigureProvider();

            Provider.Exception += DeviceProviderOnException;
            Provider.DevicesChanged += ProviderOnDevicesChanged;
            var connectSleepTimeSeconds =
                Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_connect_sleep_time");
            var remainingMillis = connectSleepTimeSeconds * 1000;

            do
            {
                providerExceptions.Clear();
                criticalExceptions.Clear();
                Provider.Initialize();
                if (criticalExceptions.Count != 0)
                {
                    break;
                }

                if (providerExceptions.Count == 0)
                {
                    IsInitialized = true;
                    break;
                }

                await Task.Delay(1000);
                remainingMillis -= 1000;
            } while (remainingMillis > 0);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }

        Provider.Exception -= DeviceProviderOnException;

        if (!IsInitialized)
        {
            Provider.DevicesChanged -= ProviderOnDevicesChanged;
            return false;
        }

        ErrorMessage = null;
        OnInitialized();
        return true;
    }

    private void ProviderOnDevicesChanged(object? sender, DevicesChangedEventArgs e)
    {
        switch (e.Action)
        {
            case DevicesChangedEventArgs.DevicesChangedAction.Added:
                ProviderOnDeviceAdded(e.Device);
                break;
            case DevicesChangedEventArgs.DevicesChangedAction.Removed:
                ProviderOnDeviceRemoved(e.Device);
                break;
        }
    }

    private void ProviderOnDeviceRemoved(IRGBDevice device)
    {
        _deviceList.Remove(device);

        var deviceName = device.DeviceInfo.Manufacturer + " " + device.DeviceInfo.Model;
        DeviceCountMap.TryGetValue(deviceName, out var count);
        if (--count == 0)
        {
            DeviceCountMap.Remove(deviceName);
        }
        else
        {
            DeviceCountMap[deviceName] = count;
        }
    }

    private void ProviderOnDeviceAdded(IRGBDevice device)
    {
        _deviceList.Add(device);

        var deviceName = device.DeviceInfo.Manufacturer + " " + device.DeviceInfo.Model;
        if (DeviceCountMap.TryGetValue(deviceName, out var count))
        {
            DeviceCountMap[deviceName] = count + 1;
        }
        else
        {
            DeviceCountMap.Add(deviceName, 1);
        }

        Task.Run(() =>
        {
            var rgbNetConfigDevices =
                DeviceMappingConfig.Config.Devices.ToDictionary(device1 => device1.Name, device2 => device2);
            RemapDeviceKeys(rgbNetConfigDevices, device);
        });
    }

    private void RemapDeviceKeys(IReadOnlyDictionary<string, RgbNetConfigDevice> rgbNetConfigDevices, IRGBDevice rgbDevice)
    {
        if (rgbNetConfigDevices.TryGetValue(rgbDevice.DeviceInfo.DeviceName, out var configDevice))
        {
            DeviceKeyRemap.TryAdd(rgbDevice, configDevice.KeyMapper);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected override Task Shutdown()
    {
        if (!OnShutdown())
        {
            return Task.CompletedTask;
        }

        if (Provider.IsInitialized)
        {
            Provider.Dispose();
        }

        Provider.DevicesChanged -= ProviderOnDevicesChanged;

        IsInitialized = false;
        return Task.CompletedTask;
    }

    protected virtual bool IsReversed()
    {
        return false;
    }

    protected virtual Task ConfigureProvider()
    {
        return Task.CompletedTask;
    }

    protected virtual void OnInitialized()
    {
    }

    /// <summary>
    /// Do shutdown tasks
    /// </summary>
    /// <returns>Whether shutdown should continue</returns>
    protected virtual bool OnShutdown()
    {
        return true;
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e,
        bool forced = false)
    {
        if (Disabled) return Task.FromResult(false);
        foreach (var device in Devices)
        {
            UpdateDevice(keyColors, device);
        }

        return Task.FromResult(true);
    }

    private void UpdateDevice(IReadOnlyDictionary<DeviceKeys, Color> keyColors, IRGBDevice device)
    {
        if (IsReversed())
        {
            UpdateReverse(keyColors, device);
        }
        else
        {
            UpdateStraight(keyColors, device);
        }

        device.Update();
    }

    private static void UpdateReverse(IReadOnlyDictionary<DeviceKeys, Color> keyColors, IRGBDevice device)
    {
        var calibrationName = CalibrationName(device);
        var calibrated = Global.Configuration.DeviceCalibrations.TryGetValue(calibrationName, out var calibration);
        foreach (var (key, color) in keyColors)
        {
            Led? led;
            if (!RgbNetKeyMappings.AuroraToRgbNet.TryGetValue(key, out var rgbNetLedId) || (led = device[rgbNetLedId]) == null)
                continue;
            if (calibrated)
            {
                UpdateLedCalibrated(led, color, calibration);
            }
            else
            {
                UpdateLed(led, color);
            }
        }
    }

    private void UpdateStraight(IReadOnlyDictionary<DeviceKeys, Color> keyColors, IRGBDevice device)
    {
        var calibrationName = CalibrationName(device);
        var calibrated = Global.Configuration.DeviceCalibrations.TryGetValue(calibrationName, out var calibration);
        foreach (var led in device)
        {
            DeviceKeyRemap.TryGetValue(device, out var keyRemap);
            if (!(keyRemap != null &&
                  keyRemap.TryGetValue(led.Id, out var dk)) && //get remapped key if device if remapped
                !RgbNetKeyMappings.KeyNames.TryGetValue(led.Id, out dk)) continue;
            if (!keyColors.TryGetValue(dk, out var color)) continue;

            if (calibrated)
            {
                UpdateLedCalibrated(led, color, calibration);
            }
            else
            {
                UpdateLed(led, color);
            }
        }
    }

    private static void UpdateLed(Led led, Color color)
    {
        led.Color = new RGB.NET.Core.Color(
            color.A,
            color.R,
            color.G,
            color.B
        );
    }

    private static void UpdateLedCalibrated(Led led, Color color, Color calibration)
    {
        led.Color = new RGB.NET.Core.Color(
            (byte)(color.A * calibration.A / 255),
            (byte)(color.R * calibration.R / 255),
            (byte)(color.G * calibration.G / 255),
            (byte)(color.B * calibration.B / 255)
        );
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);

        variableRegistry.Register($"{DeviceName}_connect_sleep_time", 40, "Connection timeout seconds");
    }

    private static string CalibrationName(IRGBDevice device)
    {
        return device.DeviceInfo.Model;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (IsInitialized)
        {
            ShutdownDevice();
        }
    }
}