using System;
using System.Windows;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.Dota_2.Layers;

/// <summary>
/// Interaction logic for Control_Dota2RespawnLayer.xaml
/// </summary>
public partial class Control_Dota2RespawnLayer
{
    private bool settingsset;

    public Control_Dota2RespawnLayer()
    {
        InitializeComponent();
    }

    public Control_Dota2RespawnLayer(Dota2RespawnLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is Dota2RespawnLayerHandler && !settingsset)
        {
            ColorPicker_background.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as Dota2RespawnLayerHandler).Properties._BackgroundColor ?? System.Drawing.Color.Empty);
            ColorPicker_respawn.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as Dota2RespawnLayerHandler).Properties._RespawnColor ?? System.Drawing.Color.Empty);
            ColorPicker_respawning.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as Dota2RespawnLayerHandler).Properties._RespawningColor ?? System.Drawing.Color.Empty);
            KeySequence_sequence.Sequence = (DataContext as Dota2RespawnLayerHandler).Properties._Sequence;

            settingsset = true;
        }
    }

    internal void SetProfile(Application profile)
    {
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_background_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2RespawnLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as Dota2RespawnLayerHandler).Properties._BackgroundColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);

    }

    private void ColorPicker_respawn_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2RespawnLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as Dota2RespawnLayerHandler).Properties._RespawnColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_respawning_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2RespawnLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as Dota2RespawnLayerHandler).Properties._RespawningColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void KeySequence_sequence_SequenceUpdated(object? sender, EventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2RespawnLayerHandler && sender is Controls.KeySequence)
            (DataContext as Dota2RespawnLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
    }
}