using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AurorDeviceManager.Devices.RGBNet.Config;
using Common.Devices;
using Common.Devices.RGBNet;
using RGB.NET.Core;

namespace Aurora.Devices.RGBNet.Config;

/// <summary>
/// Interaction logic for AsusConfigWindow.xaml
/// </summary>
public partial class DeviceMapping
{
    private readonly List<RemappableDevice> _devices = new();
    private readonly List<RgbNetKeyToDeviceKeyControl> _keys = new();

    private DeviceMappingConfig _config;

    public DeviceMapping()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnClosed;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        LoadConfigFile();
        LoadDevices();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _config.SaveConfig();
    }

    private void LoadConfigFile()
    {
        _config = DeviceMappingConfig.Config;
    }

    private void ReloadDevices()
    {
        LoadDevices();
    }

    private void LoadDevices()
    {
        // clear current devices
        AsusDeviceList.Children.Clear();
        _devices.Clear();
        //TODO load CurrentDevices object

        var remappableDevices = ReadDevices();

        if (remappableDevices == null)
        {
            return;
        }
        foreach (var remappableDevicesDevice in remappableDevices.Devices)
        {
            _devices.Add(remappableDevicesDevice);
        }

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
    }

    private static CurrentDevices? ReadDevices()
    { 
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "CurrentDevices.json");
        using var stream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<CurrentDevices>(stream);
    }

    private void DeviceSelect(RemappableDevice remappableDevice)
    {
        _keys.Clear();

        // Rebuild the key area
        AsusDeviceKeys.Children.Clear();

        var configDevice = GetAsusConfigDevice(device);

        foreach (var led in remappableDevice.RgbNetLeds)
        {
            var keyControl = new RgbNetKeyToDeviceKeyControl(configDevice, led);
                
            keyControl.BlinkCallback += () =>
            {
                _tokenSource?.Cancel();
                rgbNetDevice.Disabled = true;
                BlinkKey(device, led)
                    .ContinueWith((a) =>
                        {
                            rgbNetDevice.Disabled = false;
                            return Task.CompletedTask;
                        }
                    );
            };

            keyControl.DeviceKeyChanged += (_, newKey) =>
            {
                if (!rgbNetDevice.DeviceKeyRemap.TryGetValue(device, out var deviceKeyMap))
                {
                    deviceKeyMap = new Dictionary<LedId, DeviceKeys>();
                    rgbNetDevice.DeviceKeyRemap.TryAdd(device, deviceKeyMap);
                }

                if (newKey == null)
                {
                    deviceKeyMap.Remove(led.Id);
                    configDevice.KeyMapper.Remove(led.Id);
                }
                else
                {
                    deviceKeyMap[led.Id] = (DeviceKeys)newKey;
                    configDevice.KeyMapper[led.Id] = (DeviceKeys)newKey;
                }
            };
                
            _keys.Add(keyControl);
                
            AsusDeviceKeys.Children.Add(keyControl);
        }
    }
        
    private CancellationTokenSource? _tokenSource;
    private const int BlinkCount = 7;

    private async Task BlinkKey(IRGBDevice device, Led led)
    {
        if (_tokenSource is {Token.IsCancellationRequested: false})
            _tokenSource.Cancel();

        _tokenSource = new CancellationTokenSource();
        var token = _tokenSource.Token;
        try
        {
            for (var i = 0; i < BlinkCount; i++)
            {
                if (token.IsCancellationRequested || device == null)
                    return;

                // set everything to black
                foreach (var light in device)
                    light.Color = new Color(0, 0, 0);

                // set this one key to white
                if (i % 2 == 1)
                {
                    led.Color = new Color(255, 255, 255);
                }

                device.Update();
                await Task.Delay(200, token); // ms
            }
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error while blinking device led");
        }
    }

    private RgbNetConfigDevice GetAsusConfigDevice(IRGBDevice rgbDevice)
    {
        foreach (var netConfigDevice in DeviceMappingConfig.Config.Devices)
        {
            if (netConfigDevice.Name.Equals(rgbDevice.DeviceInfo.DeviceName))
            {
                return netConfigDevice;
            }
        }

        var rgbNetConfigDevice = new RgbNetConfigDevice(rgbDevice);
        DeviceMappingConfig.Config.Devices.Add(rgbNetConfigDevice);
        return rgbNetConfigDevice;
    }

    #region UI

    private void ReloadButton_Click(object? sender, RoutedEventArgs e)
    {
        ReloadDevices();
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