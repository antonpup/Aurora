using Aurora.Profiles.Desktop;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for Control_BreathingLayer.xaml
    /// </summary>
    public partial class Control_BreathingLayer : UserControl
    {
        private bool settingsset = false;

        public Control_BreathingLayer()
        {
            InitializeComponent();
        }

        public Control_BreathingLayer(BreathingLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is BreathingLayerHandler && !settingsset)
            {
                //this.ColorPicker_primaryColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as BreathingLayerHandler).PrimaryColor);

                this.breathing_effect_primary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as BreathingLayerHandler).Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                this.breathing_effect_random_primary_color_enabled.IsChecked = (this.DataContext as BreathingLayerHandler).Properties._RandomPrimaryColor;
                this.breathing_effect_secondary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as BreathingLayerHandler).Properties._SecondaryColor ?? System.Drawing.Color.Empty);
                this.breathing_effect_random_secondary_color_enabled.IsChecked = (this.DataContext as BreathingLayerHandler).Properties._RandomSecondaryColor;
                this.breathing_effect_speed_label.Text = "x " + (this.DataContext as BreathingLayerHandler).Properties._EffectSpeed;
                this.breathing_effect_speed_slider.Value = (float)(this.DataContext as BreathingLayerHandler).Properties._EffectSpeed;
                this.KeySequence_keys.Sequence = (this.DataContext as BreathingLayerHandler).Properties._Sequence;

                settingsset = true;
            }
        }

        private void ColorPicker_primaryColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is BreathingLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                (this.DataContext as BreathingLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
            }
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is BreathingLayerHandler && sender is Aurora.Controls.KeySequence)
            {
                (this.DataContext as BreathingLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void breathing_effect_primary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is BreathingLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                (this.DataContext as BreathingLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
            }
        }

        private void breathing_effect_random_primary_color_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is BreathingLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as BreathingLayerHandler).Properties._RandomPrimaryColor = (sender as CheckBox).IsChecked.Value;
            }
        }

        private void breathing_effect_secondary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is BreathingLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                (this.DataContext as BreathingLayerHandler).Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
            }
        }

        private void breathing_effect_random_secondary_color_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is BreathingLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as BreathingLayerHandler).Properties._RandomSecondaryColor = (sender as CheckBox).IsChecked.Value;
            }
        }

        private void breathing_effect_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is BreathingLayerHandler && sender is Slider)
            {
                (this.DataContext as BreathingLayerHandler).Properties._EffectSpeed = (float)(sender as Slider).Value;

                if (this.breathing_effect_speed_label is TextBlock)
                    this.breathing_effect_speed_label.Text = "x " + this.breathing_effect_speed_slider.Value;
            }
        }
    }
}
