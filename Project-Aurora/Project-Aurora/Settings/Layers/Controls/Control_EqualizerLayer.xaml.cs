using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.EffectsEngine;
using Aurora.Utils;
using ColorBox;
using Xceed.Wpf.Toolkit;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;

namespace Aurora.Settings.Layers.Controls;

/// <summary>
/// Interaction logic for Control_EqualizerLayer.xaml
/// </summary>
public partial class ControlEqualizerLayer
{
    private bool _settingsset;

    private Window _previewWindow;
    private readonly Image _previewImage = new();
    private static bool _previewWindowOpen;

    public ControlEqualizerLayer()
    {
        InitializeComponent();
    }

    public ControlEqualizerLayer(EqualizerLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    private void SetSettings()
    {
        if (DataContext is not EqualizerLayerHandler || _settingsset) return;
        affectedKeys.Sequence = ((EqualizerLayerHandler)DataContext).Properties._Sequence;

        eq_type.SelectedValue = ((EqualizerLayerHandler)DataContext).Properties._EQType;
        eq_view_type.SelectedValue = ((EqualizerLayerHandler)DataContext).Properties.ViewType;
        eq_background_mode.SelectedValue = ((EqualizerLayerHandler)DataContext).Properties._BackgroundMode;
        Clr_primary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(
            ((EqualizerLayerHandler)DataContext).Properties._PrimaryColor ?? System.Drawing.Color.Empty);
        Clr_secondary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(((EqualizerLayerHandler)DataContext).Properties.SecondaryColor);

        Brush brush = ((EqualizerLayerHandler)DataContext).Properties.Gradient.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error("Could not set brush, exception: " + exc);
        }

        updown_max_amplitude_value.Value = (int)((EqualizerLayerHandler)DataContext).Properties.MaxAmplitude;
        Clr_dim_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(
            ((EqualizerLayerHandler)DataContext).Properties._DimColor ?? System.Drawing.Color.Empty);
        lstbx_frequencies.ItemsSource = ((EqualizerLayerHandler)DataContext).Properties.Frequencies;
        chkbox_scale_with_system_volume.IsChecked = ((EqualizerLayerHandler)DataContext).Properties._ScaleWithSystemVolume;

        _settingsset = true;
    }

    private void eq_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && sender is ComboBox)
            ((EqualizerLayerHandler)DataContext).Properties._EQType = (EqualizerType)(sender as ComboBox).SelectedValue;
    }

    private void eq_view_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && sender is ComboBox)
            ((EqualizerLayerHandler)DataContext).Properties.ViewType = (EqualizerPresentationType)(sender as ComboBox).SelectedValue;
    }

    private void eq_background_mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && sender is ComboBox)
            ((EqualizerLayerHandler)DataContext).Properties._BackgroundMode = (EqualizerBackgroundMode)(sender as ComboBox).SelectedValue;
    }

    private void Clr_primary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler &&
            sender is ColorPicker { SelectedColor: { } } picker)
            ((EqualizerLayerHandler)DataContext).Properties._PrimaryColor =
                ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void Clr_secondary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler &&
            (sender as ColorPicker)?.SelectedColor != null)
            ((EqualizerLayerHandler)DataContext).Properties.SecondaryColor =
                ColorUtils.MediaColorToDrawingColor(((ColorPicker)sender).SelectedColor.Value);
    }

    private void Gradient_editor_BrushChanged(object sender, BrushChangedEventArgs e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && sender is ColorBox.ColorBox box)
            ((EqualizerLayerHandler)DataContext).Properties.Gradient = new EffectBrush(box.Brush);
    }

    private void Button_SetGradientRainbow_Click(object sender, RoutedEventArgs e)
    {
        ((EqualizerLayerHandler)DataContext).Properties.Gradient = new EffectBrush(ColorSpectrum.Rainbow);

        Brush brush = ((EqualizerLayerHandler)DataContext).Properties.Gradient.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error("Could not set brush, exception: " + exc);
        }
    }

    private void Button_SetGradientRainbowLoop_Click(object sender, RoutedEventArgs e)
    {
        ((EqualizerLayerHandler)DataContext).Properties.Gradient = new EffectBrush(ColorSpectrum.RainbowLoop);

        Brush brush = ((EqualizerLayerHandler)DataContext).Properties.Gradient.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error("Could not set brush, exception: " + exc);
        }
    }

    private void updown_max_amplitude_value_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && (sender as IntegerUpDown)?.Value != null)
            ((EqualizerLayerHandler)DataContext).Properties.MaxAmplitude = ((IntegerUpDown)sender).Value.Value;
    }

    private void chkbox_scale_with_system_sound_Checked(object sender, RoutedEventArgs e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && (sender as CheckBox)?.IsChecked != null)
            ((EqualizerLayerHandler)DataContext).Properties._ScaleWithSystemVolume = ((CheckBox)sender).IsChecked.Value;
    }

    private void Clr_dim_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && (sender as ColorPicker)?.SelectedColor != null)
            ((EqualizerLayerHandler)DataContext).Properties._DimColor = ColorUtils.MediaColorToDrawingColor(((ColorPicker)sender).SelectedColor.Value);
    }

    private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
    {
        if (IsLoaded && _settingsset && DataContext is EqualizerLayerHandler && sender is Aurora.Controls.KeySequence sequence)
        {
            ((EqualizerLayerHandler)DataContext).Properties._Sequence = sequence.Sequence;
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void btn_AddFreq_Click(object sender, RoutedEventArgs e)
    {
        float value;

        if (float.TryParse(txtBox_newFreqValue.Text, out value))
        {
            if (value >= 0.0f && value <= 16000.0f)
            {
                ((EqualizerLayerHandler)DataContext).Properties.Frequencies.Add(value);

                lstbx_frequencies.Items.Refresh();
            }
            else
                MessageBox.Show("Frequency must be in-between 0 Hz and 16000 Hz");
        }
        else
            MessageBox.Show("Entered value is not a number!");
    }

    private void btn_DeleteFreq_Click(object sender, RoutedEventArgs e)
    {
        if (lstbx_frequencies.SelectedItem != null)
        {
            ((EqualizerLayerHandler)DataContext).Properties.Frequencies.Remove((float)lstbx_frequencies.SelectedItem);

            lstbx_frequencies.Items.Refresh();
        }
    }

    private void btn_ShowPreviewWindow_Click(object sender, RoutedEventArgs e)
    {
        if(_previewWindow == null)
        {
            if(_previewWindowOpen)
            {
                MessageBox.Show("Equalizer preview already open for another layer.\r\nPlease close it.");
                return;
            }

            _previewWindow = new Window();
            _previewWindow.Closed += Preview_window_Closed;
            _previewWindow.ResizeMode = ResizeMode.NoResize;
            _previewWindow.SizeToContent = SizeToContent.WidthAndHeight;

            _previewWindow.Title = "Equalizer preview";
            _previewWindow.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            ((EqualizerLayerHandler)DataContext).NewLayerRender += Control_EqualizerLayer_NewLayerRender;

            _previewImage.SnapsToDevicePixels = true;
            _previewImage.HorizontalAlignment = HorizontalAlignment.Stretch;
            _previewImage.VerticalAlignment = VerticalAlignment.Stretch;
            _previewImage.MinWidth = Effects.CanvasWidth;
            _previewImage.MinHeight = Effects.CanvasHeight;
            _previewImage.Width = Effects.CanvasWidth * 4;
            _previewImage.Height = Effects.CanvasHeight * 4;

            _previewWindow.Content = _previewImage;

            _previewWindow.UpdateLayout();
            _previewWindow.Show();
        }
        else
        {
            _previewWindow.BringIntoView();
        }

        _previewWindowOpen = true;
    }

    private void Preview_window_Closed(object sender, EventArgs e)
    {
        _previewWindow = null;
        ((EqualizerLayerHandler)DataContext).NewLayerRender -= Control_EqualizerLayer_NewLayerRender;
        _previewWindowOpen = false;
    }

    private void Control_EqualizerLayer_NewLayerRender(Bitmap bitmap)
    {
        try
        {
            Dispatcher.Invoke(
                () =>
                {
                    using MemoryStream memory = new MemoryStream();
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    _previewImage.Source = bitmapimage;
                });
        }
        catch (Exception ex)
        {
            Global.logger.Warning(ex.ToString());
        }
    }
}