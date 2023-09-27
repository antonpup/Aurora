using System.Windows;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.EliteDangerous.Layers;

/// <summary>
/// Interaction logic for Control_EliteDangerousBackgroundLayer.xaml
/// </summary>
public partial class Control_EliteDangerousBackgroundLayer
{
    private bool settingsset;

    public Control_EliteDangerousBackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_EliteDangerousBackgroundLayer(EliteDangerousBackgroundLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is EliteDangerousBackgroundLayerHandler && !settingsset)
        {
            ColorPicker_CombatMode.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousBackgroundLayerHandler).Properties._CombatModeColor ?? System.Drawing.Color.Empty);
            ColorPicker_DiscoveryMode.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousBackgroundLayerHandler).Properties._DiscoveryModeColor ?? System.Drawing.Color.Empty);

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

    private void ColorPicker_CombatMode_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is EliteDangerousBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as EliteDangerousBackgroundLayerHandler).Properties._CombatModeColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_DiscoveryMode_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is EliteDangerousBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as EliteDangerousBackgroundLayerHandler).Properties._DiscoveryModeColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }
}