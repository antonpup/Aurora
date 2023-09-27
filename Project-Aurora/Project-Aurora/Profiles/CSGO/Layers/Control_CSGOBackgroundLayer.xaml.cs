using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Utils;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOBackgroundLayer.xaml
/// </summary>
public partial class Control_CSGOBackgroundLayer
{
    private bool settingsset;

    public Control_CSGOBackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOBackgroundLayer(CSGOBackgroundLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOBackgroundLayerHandler && !settingsset)
        {
            ColorPicker_CT.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOBackgroundLayerHandler).Properties._CTColor ?? System.Drawing.Color.Empty);
            ColorPicker_T.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOBackgroundLayerHandler).Properties._TColor ?? System.Drawing.Color.Empty);
            ColorPicker_Default.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOBackgroundLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);
            Checkbox_DimEnabled.IsChecked = (DataContext as CSGOBackgroundLayerHandler).Properties._DimEnabled;
            TextBox_DimValue.Content = (int)(DataContext as CSGOBackgroundLayerHandler).Properties._DimDelay + "s";
            Slider_DimSelector.Value = (DataContext as CSGOBackgroundLayerHandler).Properties._DimDelay.Value;
            IntegerUpDown_DimAmount.Value = (DataContext as CSGOBackgroundLayerHandler).Properties._DimAmount.Value;

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

    private void ColorPicker_CT_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOBackgroundLayerHandler).Properties._CTColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_T_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOBackgroundLayerHandler).Properties._TColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Default_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOBackgroundLayerHandler).Properties._DefaultColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void Checkbox_DimEnabled_enabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            (DataContext as CSGOBackgroundLayerHandler).Properties._DimEnabled  = (sender as CheckBox).IsChecked.Value;
    }

    private void Slider_DimSelector_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler && sender is Slider)
        {
            (DataContext as CSGOBackgroundLayerHandler).Properties._DimDelay = (sender as Slider).Value;

            TextBox_DimValue.Content = (int)(sender as Slider).Value + "s";
        }
    }

    private void IntegerUpDown_DimAmount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler && sender is IntegerUpDown)
        {
            (DataContext as CSGOBackgroundLayerHandler).Properties._DimAmount = (sender as IntegerUpDown).Value;
        }
    }
}