using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.Dota_2.Layers;

/// <summary>
/// Interaction logic for Control_Dota2BackgroundLayer.xaml
/// </summary>
public partial class Control_Dota2BackgroundLayer
{
    private bool settingsset;

    public Control_Dota2BackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_Dota2BackgroundLayer(Dota2BackgroundLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is Dota2BackgroundLayerHandler && !settingsset)
        {
            ColorPicker_Dire.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as Dota2BackgroundLayerHandler).Properties._DireColor ?? System.Drawing.Color.Empty);
            ColorPicker_Radiant.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as Dota2BackgroundLayerHandler).Properties._RadiantColor ?? System.Drawing.Color.Empty);
            ColorPicker_Default.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as Dota2BackgroundLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);
            Checkbox_DimEnabled.IsChecked = (DataContext as Dota2BackgroundLayerHandler).Properties._DimEnabled;
            TextBox_DimValue.Content = (int)(DataContext as Dota2BackgroundLayerHandler).Properties._DimDelay + "s";
            Slider_DimSelector.Value = (DataContext as Dota2BackgroundLayerHandler).Properties._DimDelay.Value;

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

    private void ColorPicker_Dire_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as Dota2BackgroundLayerHandler).Properties._DireColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Radiant_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as Dota2BackgroundLayerHandler).Properties._RadiantColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Default_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as Dota2BackgroundLayerHandler).Properties._DefaultColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void Checkbox_DimEnabled_enabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2BackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            (DataContext as Dota2BackgroundLayerHandler).Properties._DimEnabled  = (sender as CheckBox).IsChecked.Value;
    }

    private void Slider_DimSelector_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (IsLoaded && settingsset && DataContext is Dota2BackgroundLayerHandler && sender is Slider)
        {
            (DataContext as Dota2BackgroundLayerHandler).Properties._DimDelay = (sender as Slider).Value;

            if (TextBox_DimValue is Label)
                TextBox_DimValue.Content = (int)(sender as Slider).Value + "s";
        }
    }
}