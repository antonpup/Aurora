using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Common.Devices;
using Common.Devices.RGBNet;

namespace Aurora.Devices.RGBNet.Config;

/// <summary>
/// Interaction logic for AsusConfigWindow.xaml
/// </summary>
public partial class DeviceMapping
{
    private readonly Task<DeviceManager> _deviceManager;
    private readonly List<RemappableDevice> _devices = new();
    private readonly List<RgbNetKeyToDeviceKeyControl> _keys = new();

    private DeviceMappingConfig _config;

    public DeviceMapping(Task<DeviceManager> deviceManager)
    {
        _deviceManager = deviceManager;
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnClosed;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        LoadConfigFile();
        await LoadDevices();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _config.SaveConfig();
    }

    private void LoadConfigFile()
    {
        _config = DeviceMappingConfig.Config;
    }

    private async Task ReloadDevices()
    {
        await LoadDevices();
    }

    private async Task LoadDevices()
    {
        await (await _deviceManager).RequestRemappableDevices();
        Thread.Sleep(400);

        // clear current devices
        _devices.Clear();

        var remappableDevices = ReadDevices();

        if (remappableDevices == null)
        {
            return;
        }
        foreach (var remappableDevicesDevice in remappableDevices.Devices)
        {
            _devices.Add(remappableDevicesDevice);
        }

        Dispatcher.Invoke(() =>
        {
            AsusDeviceList.Children.Clear();
            foreach (var device in _devices)
            {
                // create a new button for the ui
                var button = new Button();
                button.Content = device.DeviceSummary;

                button.Click += (_,_) =>
                {
                    for (var i = 0; i < AsusDeviceList.Children.Count; i++)
                    {
                        if (AsusDeviceList.Children[i] is Button dButton) 
                            dButton.IsEnabled = true;
                    }

                    button.IsEnabled = false;
                    DeviceSelect(device);
                };

                AsusDeviceList.Children.Add(button);
            }
        });
    }

    private static CurrentDevices? ReadDevices()
    { 
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "CurrentDevices.json");

        if (!File.Exists(filePath))
        {
            return null;
        }
        
        using var stream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<CurrentDevices>(stream);
    }

    private void DeviceSelect(RemappableDevice remappableDevice)
    {
        _keys.Clear();

        // Rebuild the key area
        AsusDeviceKeys.Children.Clear();

        var deviceRemap = GetDeviceRemap(remappableDevice);

        foreach (var led in remappableDevice.RgbNetLeds)
        {
            var keyControl = new RgbNetKeyToDeviceKeyControl(deviceRemap, led);
                
            keyControl.BlinkCallback += () =>
            {
                _deviceManager.Result.BlinkRemappableKey(remappableDevice, led);
            };

            keyControl.DeviceKeyChanged += async (_, newKey) =>
            {
                if (newKey != null)
                {
                    deviceRemap.KeyMapper[led] = newKey.Value;
                }
                else
                {
                    deviceRemap.KeyMapper.Remove(led);
                }

                var deviceManager = await _deviceManager;
                await deviceManager.RemapKey(remappableDevice.DeviceId, led, newKey);
            };
                
            _keys.Add(keyControl);
                
            AsusDeviceKeys.Children.Add(keyControl);
        }
    }

    private DeviceRemap GetDeviceRemap(RemappableDevice device)
    {
        foreach (var netConfigDevice in DeviceMappingConfig.Config.Devices)
        {
            if (netConfigDevice.Name.Equals(device.DeviceId))
            {
                return netConfigDevice;
            }
        }

        var rgbNetConfigDevice = new DeviceRemap(device);
        DeviceMappingConfig.Config.Devices.Add(rgbNetConfigDevice);
        return rgbNetConfigDevice;
    }

    #region UI

    private async void ReloadButton_Click(object? sender, RoutedEventArgs e)
    {
        await ReloadDevices();
    }
        
    private void SetAllNone_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var key in _keys)
            key.DeviceKey = DeviceKeys.NONE;
    }
        
    private void SetAllLogo_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var key in _keys)
            key.DeviceKey = DeviceKeys.Peripheral_Logo;
    }
    #endregion
}