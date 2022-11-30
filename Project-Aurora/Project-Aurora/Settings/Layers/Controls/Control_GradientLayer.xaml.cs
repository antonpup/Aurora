using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.EffectsEngine;
using ColorBox;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Layers.Controls
{
    /// <summary>
    /// Interaction logic for Control_GradientLayer.xaml
    /// </summary>
    public partial class Control_GradientLayer
    {
        private bool settingsset;

        public Control_GradientLayer()
        {
            InitializeComponent();
        }

        public Control_GradientLayer(GradientLayerHandler datacontext)
        {
            InitializeComponent();

            DataContext = datacontext;
        }

        public void SetSettings()
        {
            if(DataContext is GradientLayerHandler && !settingsset)
            {
                wave_size_slider.Value = (DataContext as GradientLayerHandler).Properties.GradientConfig.GradientSize;
                wave_size_label.Text = (DataContext as GradientLayerHandler).Properties.GradientConfig.GradientSize + " %";
                effect_speed_slider.Value = (DataContext as GradientLayerHandler).Properties._GradientConfig.Speed;
                effect_speed_label.Text = "x " + (DataContext as GradientLayerHandler).Properties._GradientConfig.Speed;
                effect_angle.Text = (DataContext as GradientLayerHandler).Properties._GradientConfig.Angle.ToString();
                effect_animation_type.SelectedValue = (DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationType;
                effect_animation_reversed.IsChecked = (DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationReverse;
                Brush brush = (DataContext as GradientLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
                try
                {
                    gradient_editor.Brush = brush;
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Could not set brush, exception: " + exc);
                }

                KeySequence_keys.Sequence = (DataContext as GradientLayerHandler).Properties._Sequence;

                settingsset = true;
            }
        }

        private void Gradient_editor_BrushChanged(object sender, BrushChangedEventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GradientLayerHandler && sender is ColorBox.ColorBox)
                (DataContext as GradientLayerHandler).Properties._GradientConfig.Brush = new EffectBrush((sender as ColorBox.ColorBox).Brush);
        }

        private void Button_SetGradientRainbow_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as GradientLayerHandler).Properties._GradientConfig.Brush = new EffectBrush(ColorSpectrum.Rainbow);

            Brush brush = (DataContext as GradientLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
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
            (DataContext as GradientLayerHandler).Properties._GradientConfig.Brush = new EffectBrush(ColorSpectrum.RainbowLoop);

            Brush brush = (DataContext as GradientLayerHandler).Properties._GradientConfig.Brush.GetMediaBrush();
            try
            {
                gradient_editor.Brush = brush;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Could not set brush, exception: " + exc);
            }
        }
        private void effect_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && DataContext is GradientLayerHandler && sender is Slider)
            {
                (DataContext as GradientLayerHandler).Properties._GradientConfig.Speed = (float)(sender as Slider).Value;

                if (effect_speed_label is TextBlock)
                    effect_speed_label.Text = "x " + (sender as Slider).Value;
            }
        }

        private void wave_size_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && DataContext is GradientLayerHandler && sender is Slider)
            {
                (DataContext as GradientLayerHandler).Properties.GradientConfig.GradientSize = (float)(sender as Slider).Value;
                
                if (wave_size_label is TextBlock)
                    
                {
                    wave_size_label.Text = (sender as Slider).Value + " %";
                    if ((sender as Slider).Value == 0)
                    {
                        wave_size_label.Text = "Stop";
                    }
                }
                TriggerPropertyChanged();
            }
        }

        private void effect_angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && DataContext is GradientLayerHandler && sender is IntegerUpDown)
            {
                float outval;
                if (float.TryParse((sender as IntegerUpDown).Text, out outval))
                {
                    (sender as IntegerUpDown).Background = new SolidColorBrush(Color.FromArgb(255, 24, 24, 24));

                    (DataContext as GradientLayerHandler).Properties._GradientConfig.Angle = outval;
                }
                else
                {
                    (sender as IntegerUpDown).Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    (sender as IntegerUpDown).ToolTip = "Entered value is not a number";
                }
                TriggerPropertyChanged();
            }
        }

        private void effect_animation_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GradientLayerHandler && sender is ComboBox comboBox)
            {
                (DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationType = (AnimationType)comboBox.SelectedValue;
                TriggerPropertyChanged();
            }
        }

        private void effect_animation_reversed_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GradientLayerHandler && sender is CheckBox checkBox)
            {
                (DataContext as GradientLayerHandler).Properties._GradientConfig.AnimationReverse = checkBox.IsChecked.HasValue ? checkBox.IsChecked.Value : false;
                TriggerPropertyChanged();
            }
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GradientLayerHandler && sender is Aurora.Controls.KeySequence sequence)
            {
                (DataContext as GradientLayerHandler).Properties._Sequence = sequence.Sequence;
                TriggerPropertyChanged();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            Loaded -= UserControl_Loaded;
        }

        protected void TriggerPropertyChanged()
        {
            var layerHandler = (GradientLayerHandler) DataContext;
            layerHandler.Properties.OnPropertiesChanged(this);
        }
    }
}
