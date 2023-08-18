using System;
using System.Windows;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using ColorBox;

namespace Aurora.Settings.Layers.Controls;

/// <summary>
/// Interaction logic for Control_GradientFillLayer.xaml
/// </summary>
public partial class Control_GradientFillLayer
{
    private bool settingsset;

    public Control_GradientFillLayer()
    {
        InitializeComponent();
    }

    public Control_GradientFillLayer(GradientFillLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not GradientFillLayerHandler || settingsset) return;
        effect_speed_slider.Value = (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Speed;
        effect_speed_label.Text = "x " + (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Speed;
        CheckBox_FillEntire.IsChecked = (DataContext as GradientFillLayerHandler).Properties._FillEntireKeyboard;
        var brush = (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }

        KeySequence_keys.Sequence = (DataContext as GradientFillLayerHandler).Properties._Sequence;

        settingsset = true;
    }

    private void Gradient_editor_BrushChanged(object? sender, BrushChangedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is GradientFillLayerHandler && sender is ColorBox.ColorBox colorBox)
            (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Brush = new EffectBrush(colorBox.Brush);
    }

    private void Button_SetGradientRainbow_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Brush = new EffectBrush(ColorSpectrum.Rainbow);

        var brush = (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }
    }

    private void Button_SetGradientRainbowLoop_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Brush = new EffectBrush(ColorSpectrum.RainbowLoop);

        var brush = (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }
    }

    private void effect_speed_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || !settingsset || DataContext is not GradientFillLayerHandler || sender is not Slider slider) return;
        (DataContext as GradientFillLayerHandler).Properties._GradientConfig.Speed = (float)slider.Value;

        if (effect_speed_label is TextBlock)
            effect_speed_label.Text = "x " + slider.Value;
    }

    private void CheckBox_FillEntire_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is GradientFillLayerHandler && sender is CheckBox)
            (DataContext as GradientFillLayerHandler).Properties._FillEntireKeyboard = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, EventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is GradientFillLayerHandler && sender is Aurora.Controls.KeySequence)
            (DataContext as GradientFillLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }
}