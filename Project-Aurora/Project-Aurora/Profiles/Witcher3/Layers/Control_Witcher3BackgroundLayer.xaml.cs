using System.Windows;
using System.Windows.Media;
using Aurora.Utils;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.Witcher3.Layers;

/// <summary>
/// Interaction logic for Control_Witcher3BackgroundLayer.xaml
/// </summary>
public partial class Control_Witcher3BackgroundLayer
{
    private bool settingsset;

    public Control_Witcher3BackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_Witcher3BackgroundLayer(Witcher3BackgroundLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    private void SetSettings()
    {
        if (DataContext is not Witcher3BackgroundLayerHandler || settingsset) return;
        ColorPicker_Aard.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext)
            .Properties._AardColor ?? System.Drawing.Color.Empty);
        ColorPicker_Igni.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext)
            .Properties._IgniColor ?? System.Drawing.Color.Empty);
        ColorPicker_Quen.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext)
            .Properties._QuenColor ?? System.Drawing.Color.Empty);
        ColorPicker_Yrden.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext)
            .Properties._YrdenColor ?? System.Drawing.Color.Empty);
        ColorPicker_Axii.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext)
            .Properties._AxiiColor ?? System.Drawing.Color.Empty);
        ColorPicker_Default.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext)
            .Properties._DefaultColor ?? System.Drawing.Color.Empty);

        settingsset = true;
    }

    internal void SetProfile(Application profile)
    {
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_Aard_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as Witcher3BackgroundLayerHandler).Properties._AardColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Igni_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as Witcher3BackgroundLayerHandler).Properties._IgniColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Quen_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as Witcher3BackgroundLayerHandler).Properties._QuenColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Yrden_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as Witcher3BackgroundLayerHandler).Properties._YrdenColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Axii_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as Witcher3BackgroundLayerHandler).Properties._AxiiColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_Default_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            (DataContext as Witcher3BackgroundLayerHandler).Properties._DefaultColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
    }
}