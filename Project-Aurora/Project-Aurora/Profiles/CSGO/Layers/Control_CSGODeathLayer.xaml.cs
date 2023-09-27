using System.Windows;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGODeathLayer.xaml
/// </summary>
public partial class Control_CSGODeathLayer
{
    private bool settingsset;

    public Control_CSGODeathLayer()
    {
        InitializeComponent();
    }

    public Control_CSGODeathLayer(CSGODeathLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGODeathLayerHandler && !settingsset)
        {
            CSGODeathLayerHandlerProperties properties = (DataContext as CSGODeathLayerHandler).Properties;

            ColorPicker_DeathColor.SelectedColor = ColorUtils.DrawingColorToMediaColor(properties._DeathColor ?? System.Drawing.Color.Empty);
            IntegerUpDown_FadeOutAfter.Value = properties.FadeOutAfter;

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

    private void ColorPicker_DeathColor_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGODeathLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGODeathLayerHandler).Properties._DeathColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void IntegerUpDown_FadeOutAfter_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGODeathLayerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown && (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue)
            (DataContext as CSGODeathLayerHandler).Properties._FadeOutAfter = (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value;
    }
}