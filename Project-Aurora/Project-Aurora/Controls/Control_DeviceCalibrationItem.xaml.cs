using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Aurora.Utils;

namespace Aurora.Controls;

public partial class Control_DeviceCalibrationItem : UserControl
{
    public event EventHandler<KeyValuePair<string, Color>> ItemRemoved;
    private KeyValuePair<string, Color> DeviceColor => (KeyValuePair<string, Color>) DataContext;

    public Control_DeviceCalibrationItem()
    {
        InitializeComponent();
    }

    private void ColorPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
    {
        if (e.NewValue.HasValue && Global.Configuration.DeviceCalibrations.ContainsKey(DeviceColor.Key))
        {
            Global.Configuration.DeviceCalibrations[DeviceColor.Key] = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }
    }

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        ItemRemoved?.Invoke(this, DeviceColor);
    }
}