using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using CSScripting;
using Microsoft.Scripting.Utils;

namespace Aurora.Controls;

public partial class Control_DeviceCalibration : Window
{
    public Control_DeviceCalibration()
    {
        InitializeComponent();

        RefreshLists();
    }

    private void RefreshLists()
    {
        var devices = Global.dev_manager.DeviceContainers.Where(c => c.Device.IsInitialized).Select(c => c.Device);

        ComboBox.ItemsSource = devices.SelectMany(a => a.GetDevices()).Except(Global.Configuration.DeviceCalibrations.Keys);
        DeviceList.Items.Refresh();
        InvalidateVisual();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var value = (string)ComboBox.SelectionBoxItem;
        if (value.IsEmpty())
        {
            return;
        }
        Global.Configuration.DeviceCalibrations.Add(value, Color.White);
        RefreshLists();
    }

    private void Control_DeviceCalibrationItem_OnItemRemoved(object sender, KeyValuePair<string, Color> e)
    {
        Global.Configuration.DeviceCalibrations.Remove(e.Key);
        Global.Configuration.DeviceCalibrations.TrimExcess();
        RefreshLists();
    }
}