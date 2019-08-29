using Aurora.EffectsEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_EqualizerLayer.xaml
    /// </summary>
    public partial class Control_EqualizerLayer : UserControl
    {
        private bool settingsset = false;

        private Window preview_window = null;
        private Image preview_image = new Image();
        private static bool preview_window_open;

        public Control_EqualizerLayer()
        {
            InitializeComponent();
        }

        public Control_EqualizerLayer(EqualizerLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is EqualizerLayerHandler && !settingsset)
            {
                //this.ColorPicker_primaryColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as EqualizerLayerHandler).PrimaryColor);
                this.affectedKeys.Sequence = (this.DataContext as EqualizerLayerHandler).Properties._Sequence;

                this.eq_type.SelectedItem = (this.DataContext as EqualizerLayerHandler).Properties._EQType;
                this.eq_view_type.SelectedItem = (this.DataContext as EqualizerLayerHandler).Properties._ViewType;
                this.eq_background_mode.SelectedItem = (this.DataContext as EqualizerLayerHandler).Properties._BackgroundMode;
                this.Clr_primary_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as EqualizerLayerHandler).Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                this.Clr_secondary_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as EqualizerLayerHandler).Properties._SecondaryColor ?? System.Drawing.Color.Empty);

                Brush brush = (this.DataContext as EqualizerLayerHandler).Properties._Gradient.GetMediaBrush();
                try
                {
                    this.gradient_editor.Brush = brush;
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Could not set brush, exception: " + exc);
                }

                this.updown_max_amplitude_value.Value = (int)(this.DataContext as EqualizerLayerHandler).Properties._MaxAmplitude;
                this.Clr_dim_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as EqualizerLayerHandler).Properties._DimColor ?? System.Drawing.Color.Empty);
                this.lstbx_frequencies.ItemsSource = (this.DataContext as EqualizerLayerHandler).Properties._Frequencies;
                this.chkbox_scale_with_system_volume.IsChecked = (this.DataContext as EqualizerLayerHandler).Properties._ScaleWithSystemVolume;

                settingsset = true;
            }
        }

        private void eq_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is ComboBox)
                (this.DataContext as EqualizerLayerHandler).Properties._EQType = (EqualizerType)Enum.Parse(typeof(EqualizerType), (sender as ComboBox).SelectedItem.ToString());
        }

        private void eq_view_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is ComboBox)
                (this.DataContext as EqualizerLayerHandler).Properties._ViewType = (EqualizerPresentationType)Enum.Parse(typeof(EqualizerPresentationType), (sender as ComboBox).SelectedItem.ToString());
        }

        private void eq_background_mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is ComboBox)
                (this.DataContext as EqualizerLayerHandler).Properties._BackgroundMode = (EqualizerBackgroundMode)Enum.Parse(typeof(EqualizerBackgroundMode), (sender as ComboBox).SelectedItem.ToString());
        }

        private void Clr_primary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EqualizerLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Clr_secondary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EqualizerLayerHandler).Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Gradient_editor_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is ColorBox.ColorBox)
                (this.DataContext as EqualizerLayerHandler).Properties._Gradient = new EffectsEngine.EffectBrush((sender as ColorBox.ColorBox).Brush);
        }

        private void Button_SetGradientRainbow_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as EqualizerLayerHandler).Properties._Gradient = new EffectBrush(ColorSpectrum.Rainbow);

            Brush brush = (this.DataContext as EqualizerLayerHandler).Properties._Gradient.GetMediaBrush();
            try
            {
                this.gradient_editor.Brush = brush;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Could not set brush, exception: " + exc);
            }
        }

        private void Button_SetGradientRainbowLoop_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as EqualizerLayerHandler).Properties._Gradient = new EffectBrush(ColorSpectrum.RainbowLoop);

            Brush brush = (this.DataContext as EqualizerLayerHandler).Properties._Gradient.GetMediaBrush();
            try
            {
                this.gradient_editor.Brush = brush;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Could not set brush, exception: " + exc);
            }
        }

        private void updown_max_amplitude_value_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown && (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue)
                (this.DataContext as EqualizerLayerHandler).Properties._MaxAmplitude = (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.Value;
        }

        private void chkbox_scale_with_system_sound_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as EqualizerLayerHandler).Properties._ScaleWithSystemVolume = (sender as CheckBox).IsChecked.Value;
        }

        private void Clr_dim_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EqualizerLayerHandler).Properties._DimColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is EqualizerLayerHandler && sender is Aurora.Controls.KeySequence)
            {
                (this.DataContext as EqualizerLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void btn_AddFreq_Click(object sender, RoutedEventArgs e)
        {
            float value;

            if (float.TryParse(this.txtBox_newFreqValue.Text, out value))
            {
                if (value >= 0.0f && value <= 16000.0f)
                {
                    (this.DataContext as EqualizerLayerHandler).Properties._Frequencies.Add(value);

                    this.lstbx_frequencies.Items.Refresh();
                }
                else
                    MessageBox.Show("Frequency must be in-between 0 Hz and 16000 Hz");
            }
            else
                MessageBox.Show("Entered value is not a number!");
        }

        private void btn_DeleteFreq_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstbx_frequencies.SelectedItem != null)
            {
                (this.DataContext as EqualizerLayerHandler).Properties._Frequencies.Remove((float)this.lstbx_frequencies.SelectedItem);

                this.lstbx_frequencies.Items.Refresh();
            }
        }

        private void btn_ShowPreviewWindow_Click(object sender, RoutedEventArgs e)
        {
            if(preview_window == null)
            {
                if(preview_window_open == true)
                {
                    MessageBox.Show("Equalizer preview already open for another layer.\r\nPlease close it.");
                    return;
                }

                preview_window = new Window();
                preview_window.Closed += Preview_window_Closed;
                preview_window.ResizeMode = ResizeMode.NoResize;
                preview_window.SizeToContent = SizeToContent.WidthAndHeight;

                preview_window.Title = "Equalizer preview";
                preview_window.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                (this.DataContext as EqualizerLayerHandler).NewLayerRender += Control_EqualizerLayer_NewLayerRender;

                preview_image.SnapsToDevicePixels = true;
                preview_image.HorizontalAlignment = HorizontalAlignment.Stretch;
                preview_image.VerticalAlignment = VerticalAlignment.Stretch;
                preview_image.MinWidth = Effects.canvas_width;
                preview_image.MinHeight = Effects.canvas_height;
                preview_image.Width = Effects.canvas_width * 4;
                preview_image.Height = Effects.canvas_height * 4;

                preview_window.Content = preview_image;

                preview_window.UpdateLayout();
                preview_window.Show();
            }
            else
            {
                preview_window.BringIntoView();
            }

            preview_window_open = true;
        }

        private void Preview_window_Closed(object sender, EventArgs e)
        {
            preview_window = null;
            (this.DataContext as EqualizerLayerHandler).NewLayerRender -= Control_EqualizerLayer_NewLayerRender;
            preview_window_open = false;
        }

        private void Control_EqualizerLayer_NewLayerRender(System.Drawing.Bitmap bitmap)
        {
            try
            {
                Dispatcher.Invoke(
                    () =>
                    {
                        using (MemoryStream memory = new MemoryStream())
                        {
                            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                            memory.Position = 0;
                            BitmapImage bitmapimage = new BitmapImage();
                            bitmapimage.BeginInit();
                            bitmapimage.StreamSource = memory;
                            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapimage.EndInit();

                            preview_image.Source = bitmapimage;
                        }
                    });
            }
            catch (Exception ex)
            {
                Global.logger.Warn(ex.ToString());
            }
        }
    }
}
