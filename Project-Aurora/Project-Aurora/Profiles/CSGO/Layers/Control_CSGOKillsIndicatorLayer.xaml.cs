using System;
using System.Windows;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOKillIndicatorLayer.xaml
/// </summary>
public partial class Control_CSGOKillIndicatorLayer
{
    private bool settingsset;

    public Control_CSGOKillIndicatorLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOKillIndicatorLayer(CSGOKillIndicatorLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOKillIndicatorLayerHandler && !settingsset)
        {
            ColorPicker_RegularKill.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOKillIndicatorLayerHandler).Properties._RegularKillColor ?? System.Drawing.Color.Empty);
            ColorPicker_HeadshotKill.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOKillIndicatorLayerHandler).Properties._HeadshotKillColor ?? System.Drawing.Color.Empty);
            KeySequence_keys.Sequence = (DataContext as CSGOKillIndicatorLayerHandler).Properties._Sequence;

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

    private void ColorPicker_RegularKill_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOKillIndicatorLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOKillIndicatorLayerHandler).Properties._RegularKillColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_HeadshotKill_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOKillIndicatorLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOKillIndicatorLayerHandler).Properties._HeadshotKillColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, EventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOKillIndicatorLayerHandler && sender is Controls.KeySequence)
        {
            (DataContext as CSGOKillIndicatorLayerHandler).Properties._Sequence = (sender as Controls.KeySequence).Sequence;
        }
    }
}