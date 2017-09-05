using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Aurora.Controls
{
    public partial class EffectSettingsWindow : Window
    {
        public LayerEffectConfig EffectConfig;
        //public PreviewType preview = PreviewType.GenericApplication;
        public string preview_key = "";

        public event EventHandler EffectConfigUpdated;


        public EffectSettingsWindow()
        {
            InitializeComponent();

            EffectConfig = new LayerEffectConfig();
            this.primary_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(EffectConfig.primary);
            this.secondary_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(EffectConfig.secondary);
            this.effect_speed_slider.Value = EffectConfig.speed;
            this.effect_speed_label.Text = "x " + EffectConfig.speed;
            this.effect_angle.Text = EffectConfig.angle.ToString();
            Brush brush = EffectConfig.brush.GetMediaBrush();
            try
            {
                this.gradient_editor.Brush = brush;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Could not set brush, exception: " + exc);

                //this.gradient_editor.Brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            }

            this.gradient_editor.BrushChanged += Gradient_editor_BrushChanged;
        }

        public EffectSettingsWindow(LayerEffectConfig EffectConfig)
        {
            InitializeComponent();

            this.EffectConfig = EffectConfig;
            this.primary_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(EffectConfig.primary);
            this.secondary_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(EffectConfig.secondary);
            this.effect_speed_slider.Value = EffectConfig.speed;
            this.effect_speed_label.Text = "x " + EffectConfig.speed;
            this.effect_angle.Text = EffectConfig.angle.ToString();
            this.effect_animation_type.SelectedIndex = (int)EffectConfig.animation_type;
            this.effect_animation_reversed.IsChecked = EffectConfig.animation_reverse;
            Brush brush = EffectConfig.brush.GetMediaBrush();
            try
            {
                this.gradient_editor.Brush = brush;
            }
            catch(Exception exc)
            {
                Global.logger.Error("Could not set brush, exception: " + exc);

                //this.gradient_editor.Brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            }

            this.gradient_editor.BrushChanged += Gradient_editor_BrushChanged;
        }

        private void Gradient_editor_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e)
        {
            EffectConfig.brush = new EffectsEngine.EffectBrush(this.gradient_editor.Brush);
        }

        private void FireEffectConfigUpdated()
        {
            if (EffectConfigUpdated != null)
                EffectConfigUpdated(this, new EventArgs());
        }


        private void effect_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
            {
                if (this.effect_speed_label is TextBlock)
                {
                    this.effect_speed_label.Text = "x " + (sender as Slider).Value;
                }

                EffectConfig.speed = (float)(sender as Slider).Value;
                FireEffectConfigUpdated();
            }
        }

        private void primary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (this.IsLoaded && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                EffectConfig.primary = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
                FireEffectConfigUpdated();
            }
        }

        private void secondary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (this.IsLoaded && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                EffectConfig.secondary = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
                FireEffectConfigUpdated();
            }
        }

        private void accept_button_Click(object sender, RoutedEventArgs e)
        {
            EffectConfig.brush = new EffectsEngine.EffectBrush(this.gradient_editor.Brush);
            FireEffectConfigUpdated();
            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = preview_key;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = null;
        }

        private void effect_angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.IsLoaded)
            {
                float outval;
                if (float.TryParse((sender as IntegerUpDown).Text, out outval))
                {
                    (sender as IntegerUpDown).Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                    EffectConfig.angle = outval;
                    FireEffectConfigUpdated();
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
            if (IsLoaded)
            {
                EffectConfig.animation_type = (AnimationType)Enum.Parse(typeof(AnimationType),effect_animation_type.SelectedIndex.ToString());
            }
        }

        private void effect_animation_reversed_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                EffectConfig.animation_reverse = (effect_animation_reversed.IsChecked.HasValue ? effect_animation_reversed.IsChecked.Value : false);
            }
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member