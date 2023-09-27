using System.Windows;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_WinningTeamLayer.xaml
/// </summary>
public partial class Control_CSGOWinningTeamLayer
{
    private bool settingsset;

    public Control_CSGOWinningTeamLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOWinningTeamLayer(CSGOWinningTeamLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOWinningTeamLayerHandler && !settingsset)
        {
            ColorPicker_CT.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOWinningTeamLayerHandler).Properties._CTColor ?? System.Drawing.Color.Empty);
            ColorPicker_T.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOWinningTeamLayerHandler).Properties._TColor ?? System.Drawing.Color.Empty);

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
        if (IsLoaded && settingsset && DataContext is CSGOWinningTeamLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOWinningTeamLayerHandler).Properties._CTColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_T_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOWinningTeamLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOWinningTeamLayerHandler).Properties._TColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }
}