using Aurora.EffectsEngine;
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
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_GradientLayer.xaml
    /// </summary>
    public partial class Control_GradientLayer : UserControl
    {
        private bool settingsset = false;

        public Control_GradientLayer()
        {
            InitializeComponent();
        }

        public Control_GradientLayer(GradientLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if(this.DataContext is GradientLayerHandler && !settingsset)
            {
                this.wave_size_slider.Value = (this.DataContext as GradientLayerHandler).Properties.GradientConfig.gradient_size;
                this.wave_size_label.Text = (this.DataContext as GradientLayerHandler).Properties.GradientConfig.gradient_size + " %";
                this.effect_speed_slider.Value = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.speed;
                this.effect_speed_label.Text = "x " + (this.DataContext as GradientLayerHandler).Properties._GradientConfig.speed;
                this.effect_angle.Text = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.angle.ToString();
                this.effect_animation_type.SelectedIndex = (int)(this.DataContext as GradientLayerHandler).Properties._GradientConfig.animation_type;
                this.effect_animation_reversed.IsChecked = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.animation_reverse;
                Brush brush = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.brush.GetMediaBrush();
                try
                {
                    this.gradient_editor.Brush = brush;
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Could not set brush, exception: " + exc);
                }

                this.KeySequence_keys.Sequence = (this.DataContext as GradientLayerHandler).Properties._Sequence;

                settingsset = true;
            }
        }

        private void Gradient_editor_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is ColorBox.ColorBox)
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.brush = new EffectsEngine.EffectBrush((sender as ColorBox.ColorBox).Brush);
        }

        private void Button_SetGradientRainbow_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as GradientLayerHandler).Properties._GradientConfig.brush = new EffectBrush(ColorSpectrum.Rainbow);

            Brush brush = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.brush.GetMediaBrush();
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
            (this.DataContext as GradientLayerHandler).Properties._GradientConfig.brush = new EffectBrush(ColorSpectrum.RainbowLoop);

            Brush brush = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.brush.GetMediaBrush();
            try
            {
                this.gradient_editor.Brush = brush;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Could not set brush, exception: " + exc);
            }
        }
        private void effect_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is Slider)
            {
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.speed = (float)(sender as Slider).Value;

                if (this.effect_speed_label is TextBlock)
                    this.effect_speed_label.Text = "x " + (sender as Slider).Value;
            }
        }

        private void wave_size_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is Slider)
            {
                (this.DataContext as GradientLayerHandler).Properties.GradientConfig.gradient_size = (float)(sender as Slider).Value;
                
                if (this.wave_size_label is TextBlock)
                    
                {
                    this.wave_size_label.Text = (sender as Slider).Value + " %";
                    if ((sender as Slider).Value == 0)
                    {
                        this.wave_size_label.Text = "Stop";
                    }
                }
            }
        }

        private void effect_angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is IntegerUpDown)
            {
                float outval;
                if (float.TryParse((sender as IntegerUpDown).Text, out outval))
                {
                    (sender as IntegerUpDown).Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24));

                    (this.DataContext as GradientLayerHandler).Properties._GradientConfig.angle = outval;
                }
                else
                {
                    (sender as IntegerUpDown).Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    (sender as IntegerUpDown).ToolTip = "Entered value is not a number";
                }

            }
        }

        private void effect_animation_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is ComboBox)
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.animation_type = (AnimationType)Enum.Parse(typeof(AnimationType), (sender as ComboBox).SelectedIndex.ToString());
        }

        private void effect_animation_reversed_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is CheckBox)
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.animation_reverse = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is Aurora.Controls.KeySequence)
                (this.DataContext as GradientLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }
    }
}
