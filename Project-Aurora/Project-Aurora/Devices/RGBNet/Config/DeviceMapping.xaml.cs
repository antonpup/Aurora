using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Aurora.Modules.GameStateListen;
using Common;
using Common.Devices;
using Common.Devices.RGBNet;

namespace Aurora.Devices.RGBNet.Config;

/// <summary>
/// Interaction logic for AsusConfigWindow.xaml
/// </summary>
public partial class DeviceMapping
{
    private readonly Task<DeviceManager> _deviceManager;
    private readonly Task<IpcListener?> _ipcListener;
    
    private readonly List<RemappableDevice> _devices = new();
    private readonly List<RgbNetKeyToDeviceKeyControl> _keys = new();

    private readonly Lazy<DeviceMappingConfig> _config = new(() => DeviceMappingConfig.Config);

    public DeviceMapping(Task<DeviceManager> deviceManager, Task<IpcListener?> ipcListener)
    {
        _deviceManager = deviceManager;
        _ipcListener = ipcListener;
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public async Task Initialize()
    {
        var ipcListener = await _ipcListener;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived += OnAuroraCommandReceived;
        }
    }

    private void OnAuroraCommandReceived(object? sender, string e)
    {
        var words = e.Split(Constants.StringSplit);
        if (words.Length != 2)
        {
            return;
        }

        var command = words[0];
        var json = words[1];
        switch (command)
        {
            case DeviceCommands.RemappableDevices:
                var remappableDevices = ReadDevices(json);
                
                Dispatcher.Invoke(() => LoadDevices(remappableDevices));
                break;
        }
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await (await _deviceManager).RequestRemappableDevices();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        _config.Value.SaveConfig();
    }

    private async Task ReloadDevices()
    {
        await (await _deviceManager).RequestRemappableDevices();
    }

    private void LoadDevices(CurrentDevices remappableDevices)
    {
        // clear current devices
        _devices.Clear();

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

    private static CurrentDevices ReadDevices(string json)
    { 
        return JsonSerializer.Deserialize<CurrentDevices>(json) ?? new CurrentDevices(new List<RemappableDevice>());
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