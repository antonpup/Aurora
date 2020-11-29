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
                this.wave_size_slider.Value = (this.DataContext as GradientLayerHandler).Properties.GradientConfig.GradientSize;
                this.wave_size_label.Text = (this.DataContext as GradientLayerHandler).Properties.GradientConfig.GradientSize + " %";
                this.effect_speed_slider.Value = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Speed;
                this.effect_speed_label.Text = "x " + (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Speed;
                this.effect_angle.Text = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Angle.ToString();
                this.effect_animation_type.SelectedValue = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationType;
                this.effect_animation_reversed.IsChecked = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationReverse;
                Brush brush = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
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
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Brush = new EffectsEngine.EffectBrush((sender as ColorBox.ColorBox).Brush);
        }

        private void Button_SetGradientRainbow_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Brush = new EffectBrush(ColorSpectrum.Rainbow);

            Brush brush = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
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
            (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Brush = new EffectBrush(ColorSpectrum.RainbowLoop);

            Brush brush = (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
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
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Speed = (float)(sender as Slider).Value;

                if (this.effect_speed_label is TextBlock)
                    this.effect_speed_label.Text = "x " + (sender as Slider).Value;
            }
        }

        private void wave_size_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is Slider)
            {
                (this.DataContext as GradientLayerHandler).Properties.GradientConfig.GradientSize = (float)(sender as Slider).Value;
                
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

                    (this.DataContext as GradientLayerHandler).Properties._GradientConfig.Angle = outval;
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
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationType = (AnimationType)(sender as ComboBox).SelectedValue;
        }

        private void effect_animation_reversed_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientLayerHandler && sender is CheckBox)
                (this.DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationReverse = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
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
