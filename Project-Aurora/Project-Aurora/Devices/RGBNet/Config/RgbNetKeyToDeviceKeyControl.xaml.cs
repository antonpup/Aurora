using System;
using System.Windows;

namespace Aurora.Devices.RGBNet.Config;

/// <summary>
/// Interaction logic for AsusKeyToDeviceKeyControl.xaml
/// </summary>
public partial class RgbNetKeyToDeviceKeyControl
{
    public Action BlinkCallback { get; set; }
    public event EventHandler<DeviceKeys?> DeviceKeyChanged;

    public RgbNetKeyToDeviceKeyControl()
    {
        InitializeComponent();
        DeviceKey.SelectionChanged += OnDeviceKeyOnDropDownClosed;
    }

    private void OnDeviceKeyOnDropDownClosed(object sender, EventArgs args)
    {
        DeviceKeyChanged?.Invoke(this, DeviceKey.SelectedValue as DeviceKeys?);
    }

    private void TestBlink(object sender, RoutedEventArgs e)
    {
        BlinkCallback?.Invoke();
    }

    private void Clear(object sender, RoutedEventArgs e)
    {
        DeviceKey.SelectedItem = null;
    }
}