using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Aurora.Devices;

namespace Aurora.Settings.Layers.Controls;

public partial class Control_RazerLayer
{
    private bool _settingsSet;
    private RazerLayerHandler? Context => DataContext as RazerLayerHandler;

    public Control_RazerLayer()
    {
        InitializeComponent();
    }

    public Control_RazerLayer(RazerLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (Context == null || _settingsSet) return;

        ColorPostProcessCheckBox.IsChecked = Context.Properties.ColorPostProcessEnabled;
        BrightnessSlider.Value = Context.Properties.BrightnessChange;
        SaturationSlider.Value = Context.Properties.SaturationChange;
        HueSlider.Value = Context.Properties.HueShift;
        CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
        _settingsSet = true;
    }

    private void OnUserControlLoaded(object sender, RoutedEventArgs e)
    {
        SetSettings();
        Loaded -= OnUserControlLoaded;
    }

    private void OnAddKeyCloneButtonClick(object sender, RoutedEventArgs e)
    {
        if (KeyCloneSourceButtonComboBox.SelectedValue == null || KeyCloneDestinationButtonComboBox.SelectedValue == null)
            return;

        var sourceKey = (DeviceKeys)KeyCloneSourceButtonComboBox.SelectedValue;
        var destKey = (DeviceKeys)KeyCloneDestinationButtonComboBox.SelectedValue;

        if (sourceKey == destKey)
            return;

        var cloneMap = Context.Properties.KeyCloneMap;
        if (cloneMap.ContainsKey(destKey))
            return;

        cloneMap.Add(destKey, sourceKey);

        KeyCloneDestinationButtonComboBox.SelectedValue = null;
        CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
    }

    private void OnDeleteKeyCloneButtonClick(object sender, RoutedEventArgs e)
    {
        var cloneMap = Context.Properties.KeyCloneMap;
        foreach (var o in KeyCloneListBox.SelectedItems)
        {
            if (o is not KeyValuePair<DeviceKeys, DeviceKeys> item) continue;
            if (!cloneMap.ContainsKey(item.Key) || cloneMap[item.Key] != item.Value)
                continue;

            cloneMap.Remove(item.Key);
        }

        CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
    }
}