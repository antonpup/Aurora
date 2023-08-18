using System;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Layers.Controls;

/// <summary>
/// Interaction logic for Control_PercentGradientLayer.xaml
/// </summary>
public partial class Control_PercentGradientLayer
{
    private bool settingsset;
    private bool profileset = false;

    public Control_PercentGradientLayer()
    {
        InitializeComponent();
    }

    public Control_PercentGradientLayer(PercentGradientLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not PercentGradientLayerHandler || settingsset) return;
        ComboBox_effect_type.SelectedValue = (DataContext as PercentGradientLayerHandler).Properties._PercentType;
        updown_blink_value.Value = (int)((DataContext as PercentGradientLayerHandler).Properties._BlinkThreshold * 100);
        CheckBox_threshold_reverse.IsChecked = (DataContext as PercentGradientLayerHandler).Properties._BlinkDirection;
        KeySequence_keys.Sequence = (DataContext as PercentGradientLayerHandler).Properties._Sequence;

        var brush = (DataContext as PercentGradientLayerHandler).Properties._Gradient.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }

        settingsset = true;
    }

    internal void SetApplication(Profiles.Application profile)
    {
        VariablePath.Application = MaxVariablePath.Application = profile;
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, EventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is PercentGradientLayerHandler && sender is Aurora.Controls.KeySequence)
            (DataContext as PercentGradientLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void Gradient_editor_BrushChanged(object? sender, ColorBox.BrushChangedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is PercentGradientLayerHandler && sender is ColorBox.ColorBox colorBox)
            (DataContext as PercentGradientLayerHandler).Properties._Gradient = new EffectsEngine.EffectBrush(colorBox.Brush);
    }

    private void updown_blink_value_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && settingsset && DataContext is PercentGradientLayerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown down && down.Value.HasValue)
            (DataContext as PercentGradientLayerHandler).Properties._BlinkThreshold = down.Value.Value / 100.0D;
    }

    private void ComboBox_effect_type_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is PercentGradientLayerHandler && sender is ComboBox comboBox)
        {
            (DataContext as PercentGradientLayerHandler).Properties._PercentType = (PercentEffectType)comboBox.SelectedValue;
        }
    }

    private void CheckBox_threshold_reverse_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is PercentGradientLayerHandler && sender is CheckBox checkBox && checkBox.IsChecked.HasValue)
        {
            (DataContext as PercentGradientLayerHandler).Properties._BlinkDirection = checkBox.IsChecked.Value;
        }
    }
}