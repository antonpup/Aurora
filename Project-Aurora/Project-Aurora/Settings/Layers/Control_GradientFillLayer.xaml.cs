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
    /// Interaction logic for Control_GradientFillLayer.xaml
    /// </summary>
    public partial class Control_GradientFillLayer : UserControl
    {
        private bool settingsset = false;

        public Control_GradientFillLayer()
        {
            InitializeComponent();
        }

        public Control_GradientFillLayer(GradientFillLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if(this.DataContext is GradientFillLayerHandler && !settingsset)
            {
                this.effect_speed_slider.Value = (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.speed;
                this.effect_speed_label.Text = "x " + (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.speed;
                this.CheckBox_FillEntire.IsChecked = (this.DataContext as GradientFillLayerHandler).Properties._FillEntireKeyboard;
                Brush brush = (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.brush.GetMediaBrush();
                try
                {
                    this.gradient_editor.Brush = brush;
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Could not set brush, exception: " + exc);
                }

                this.KeySequence_keys.Sequence = (this.DataContext as GradientFillLayerHandler).Properties._Sequence;

                settingsset = true;
            }
        }

        private void Gradient_editor_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientFillLayerHandler && sender is ColorBox.ColorBox)
                (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.brush = new EffectsEngine.EffectBrush((sender as ColorBox.ColorBox).Brush);
        }

        private void Button_SetGradientRainbow_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.brush = new EffectBrush(ColorSpectrum.Rainbow);

            Brush brush = (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.brush.GetMediaBrush();
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
            (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.brush = new EffectBrush(ColorSpectrum.RainbowLoop);

            Brush brush = (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.brush.GetMediaBrush();
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
            if (IsLoaded && settingsset && this.DataContext is GradientFillLayerHandler && sender is Slider)
            {
                (this.DataContext as GradientFillLayerHandler).Properties._GradientConfig.speed = (float)(sender as Slider).Value;

                if (this.effect_speed_label is TextBlock)
                    this.effect_speed_label.Text = "x " + (sender as Slider).Value;
            }
        }

        private void CheckBox_FillEntire_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientFillLayerHandler && sender is CheckBox)
                (this.DataContext as GradientFillLayerHandler).Properties._FillEntireKeyboard = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GradientFillLayerHandler && sender is Aurora.Controls.KeySequence)
                (this.DataContext as GradientFillLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }
    }
}
