using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Devices;

namespace Aurora.Controls;

public partial class Control_DeviceCalibration
{
    private readonly Task<DeviceManager> _deviceManager;
    
    public Control_DeviceCalibration(Task<DeviceManager> deviceManager)
    {
        _deviceManager = deviceManager;
        
        InitializeComponent();
    }

    private async Task RefreshLists()
    {
        var deviceContainers = (await _deviceManager).DeviceContainers;
        var devices = deviceContainers
            .Where(c => c.Device.IsInitialized)
            .Select(c => c.Device);

        ComboBox.ItemsSource = devices
            .SelectMany(a => a.GetDevices())
            .Except(Global.Configuration.DeviceCalibrations.Keys);
        DeviceList.Items.Refresh();
    }

    private async void ButtonBase_OnClick(object? sender, RoutedEventArgs e)
    {
        var value = (string)ComboBox.SelectionBoxItem;
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        Global.Configuration.DeviceCalibrations.Add(value, Color.White);
        await RefreshLists();
    }

    private async void Control_DeviceCalibrationItem_OnItemRemoved(object? sender, KeyValuePair<string, Color> e)
    {
        Global.Configuration.DeviceCalibrations.Remove(e.Key);
        Global.Configuration.DeviceCalibrations.TrimExcess();
        await RefreshLists();
    }

    private async void Control_DeviceCalibration_OnLoaded(object sender, RoutedEventArgs e)
    {
        await RefreshLists();
    }
}