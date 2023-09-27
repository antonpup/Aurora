using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOBombLayer.xaml
/// </summary>
public partial class Control_CSGOBombLayer
{
    private bool settingsset = false;

    public Control_CSGOBombLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOBombLayer(CSGOBombLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOBombLayerHandler && !settingsset)
        {
            ColorPicker_Flash.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOBombLayerHandler).Properties._FlashColor ?? System.Drawing.Color.Empty);
            ColorPicker_Primed.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOBombLayerHandler).Properties._PrimedColor ?? System.Drawing.Color.Empty);
            Checkbox_GradualEffect.IsChecked = (DataContext as CSGOBombLayerHandler).Properties._GradualEffect;
            Checkbox_DisplayOnPeripherals.IsChecked = (DataContext as CSGOBombLayerHandler).Properties._PeripheralUse;
            KeySequence_keys.Sequence = (DataContext as CSGOBombLayerHandler).Properties._Sequence;

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

    private void ColorPicker_Flash_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBombLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOBombLayerHandler).Properties._FlashColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Primed_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBombLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOBombLayerHandler).Properties._PrimedColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void Checkbox_GradualEffect_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBombLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            (DataContext as CSGOBombLayerHandler).Properties._GradualEffect = (sender as CheckBox).IsChecked.Value;
    }

    private void Checkbox_DisplayOnPeripherals_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBombLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            (DataContext as CSGOBombLayerHandler).Properties._PeripheralUse = (sender as CheckBox).IsChecked.Value;
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, EventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBombLayerHandler && sender is Controls.KeySequence)
        {
            (DataContext as CSGOBombLayerHandler).Properties._Sequence = (sender as Controls.KeySequence).Sequence;
        }
    }
}