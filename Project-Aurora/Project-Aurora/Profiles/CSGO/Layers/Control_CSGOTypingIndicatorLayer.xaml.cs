using System;
using System.Windows;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOTypingIndicatorLayer.xaml
/// </summary>
public partial class Control_CSGOTypingIndicatorLayer
{
    private bool settingsset;

    public Control_CSGOTypingIndicatorLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOTypingIndicatorLayer(CSGOTypingIndicatorLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOTypingIndicatorLayerHandler && !settingsset)
        {
            ColorPicker_TypingKeys.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as CSGOTypingIndicatorLayerHandler).Properties._TypingKeysColor ?? System.Drawing.Color.Empty);
            KeySequence_keys.Sequence = (DataContext as CSGOTypingIndicatorLayerHandler).Properties._Sequence;

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
        if (IsLoaded && settingsset && DataContext is CSGOTypingIndicatorLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            (DataContext as CSGOTypingIndicatorLayerHandler).Properties._TypingKeysColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, EventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOTypingIndicatorLayerHandler && sender is Controls.KeySequence)
        {
            (DataContext as CSGOTypingIndicatorLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }
    }
}