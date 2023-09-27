using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOBurningLayer.xaml
/// </summary>
public partial class Control_CSGOBurningLayer
{
    private bool settingsset;

    public Control_CSGOBurningLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOBurningLayer(CSGOBurningLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOBurningLayerHandler && !settingsset)
        {
            ColorPicker_Burning.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOBurningLayerHandler).Properties._BurningColor ?? System.Drawing.Color.Empty);
            checkBox_Animated.IsChecked = (DataContext as CSGOBurningLayerHandler).Properties._Animated;

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

    private void ColorPicker_Burning_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBurningLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOBurningLayerHandler).Properties._BurningColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void checkBox_Animated_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBurningLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            (DataContext as CSGOBurningLayerHandler).Properties._Animated = (sender as CheckBox).IsChecked.Value;
    }
}