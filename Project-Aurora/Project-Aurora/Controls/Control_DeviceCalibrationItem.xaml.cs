using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Aurora.Utils;

namespace Aurora.Controls;

public partial class Control_DeviceCalibrationItem
{
    public event EventHandler<KeyValuePair<string, Color>> ItemRemoved;
    private KeyValuePair<string, Color> DeviceColor => (KeyValuePair<string, Color>) DataContext;

    public Control_DeviceCalibrationItem()
    {
        InitializeComponent();
    }

    private void ColorPicker_OnSelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
    {
        if (e.NewValue.HasValue && Global.DeviceConfigration.DeviceCalibrations.ContainsKey(DeviceColor.Key))
        {
            Global.DeviceConfigration.DeviceCalibrations[DeviceColor.Key] = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ItemRemoved?.Invoke(this, DeviceColor);
    }
}