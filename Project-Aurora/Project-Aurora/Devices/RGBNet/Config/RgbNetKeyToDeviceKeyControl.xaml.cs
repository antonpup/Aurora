using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using RGB.NET.Core;

namespace Aurora.Devices.RGBNet.Config;

/// <summary>
/// Interaction logic for AsusKeyToDeviceKeyControl.xaml
/// </summary>
public partial class RgbNetKeyToDeviceKeyControl
{
    public Action? BlinkCallback { get; set; }

    public DeviceKeys? DeviceKey
    {
        set
        {
            DeviceKeyButton.Content = value;
            DeviceKeyChanged?.Invoke(this, value);
        }
    }

    private readonly RgbNetConfigDevice _configDevice;
    private readonly Led _led;

    public event EventHandler<DeviceKeys?>? DeviceKeyChanged;

    public RgbNetKeyToDeviceKeyControl(RgbNetConfigDevice configDevice, Led led)
    {
        _configDevice = configDevice;
        _led = led;
        
        InitializeComponent();

        KeyIdValue.Text = led.Id.ToString();
        UpdateMappedLedId();
    }

    private void UpdateMappedLedId()
    {
        if (_configDevice.KeyMapper.TryGetValue(_led.Id, out var deviceKey))
        {
            DeviceKeyButton.Content = deviceKey;
            ButtonBorder.BorderBrush = Brushes.Blue;
        }
        else
        {
            if (RgbNetKeyMappings.KeyNames.TryGetValue(_led.Id, out var defaultKey))
            {
                DeviceKeyButton.Content = defaultKey;
                ButtonBorder.BorderBrush = Brushes.Black;
            }
            else
            {
                DeviceKeyButton.Content = DeviceKeys.NONE;
                ButtonBorder.BorderBrush = Brushes.Red;
            }
        }
    }

    private void TestBlink(object? sender, RoutedEventArgs e)
    {
        BlinkCallback?.Invoke();
    }

    private void Clear(object? sender, RoutedEventArgs e)
    {
        DeviceKeyChanged?.Invoke(this, null);
        UpdateMappedLedId();
    }

    private void DeviceKeyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Global.key_recorder.FinishedRecording += KeyRemapped;
        Global.key_recorder.StartRecording("DeviceRemap", true);
    }

    private void KeyRemapped(DeviceKeys[] keys)
    {
        Global.key_recorder.FinishedRecording -= KeyRemapped;
        var assignedKey = keys.First();
        DeviceKeyChanged?.Invoke(this, assignedKey);
        UpdateMappedLedId();
        Global.key_recorder.Reset();
    }
}