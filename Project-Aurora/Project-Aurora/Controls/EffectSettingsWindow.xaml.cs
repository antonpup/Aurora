using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Utils;
using ColorBox;
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
            primary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(EffectConfig.Primary);
            secondary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(EffectConfig.Secondary);
            effect_speed_slider.Value = EffectConfig.Speed;
            effect_speed_label.Text = "x " + EffectConfig.Speed;
            effect_angle.Text = EffectConfig.Angle.ToString();
            Brush brush = EffectConfig.Brush.GetMediaBrush();
            gradient_editor.Brush = brush;

            gradient_editor.BrushChanged += Gradient_editor_BrushChanged;
        }

        public EffectSettingsWindow(LayerEffectConfig EffectConfig)
        {
            InitializeComponent();

            this.EffectConfig = EffectConfig;
            primary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(EffectConfig.Primary);
            secondary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(EffectConfig.Secondary);
            effect_speed_slider.Value = EffectConfig.Speed;
            effect_speed_label.Text = "x " + EffectConfig.Speed;
            effect_angle.Text = EffectConfig.Angle.ToString();
            effect_animation_type.SelectedValue = EffectConfig.AnimationType;
            effect_animation_reversed.IsChecked = EffectConfig.AnimationReverse;
            Brush brush = EffectConfig.Brush.GetMediaBrush();
            try
            {
                gradient_editor.Brush = brush;
            }
            catch(Exception exc)
            {
                Global.logger.Error("Could not set brush:", exc);
            }

            gradient_editor.BrushChanged += Gradient_editor_BrushChanged;
        }

        private void Gradient_editor_BrushChanged(object? sender, BrushChangedEventArgs e)
        {
            EffectConfig.Brush = new EffectBrush(gradient_editor.Brush);
        }

        private void FireEffectConfigUpdated()
        {
            if (EffectConfigUpdated != null)
                EffectConfigUpdated(this, new EventArgs());
        }


        private void effect_speed_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                if (effect_speed_label is TextBlock)
                {
                    effect_speed_label.Text = "x " + (sender as Slider).Value;
                }

                EffectConfig.Speed = (float)(sender as Slider).Value;
                FireEffectConfigUpdated();
            }
        }

        private void primary_color_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && (sender as ColorPicker).SelectedColor.HasValue)
            {
                EffectConfig.Primary = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                FireEffectConfigUpdated();
            }
        }

        private void secondary_color_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && (sender as ColorPicker).SelectedColor.HasValue)
            {
                EffectConfig.Secondary = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                FireEffectConfigUpdated();
            }
        }

        private void accept_button_Click(object? sender, RoutedEventArgs e)
        {
            EffectConfig.Brush = new EffectBrush(gradient_editor.Brush);
            FireEffectConfigUpdated();
            Close();
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = preview_key;
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = null;
        }

        private void effect_angle_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded)
            {
                float outval;
                if (float.TryParse((sender as IntegerUpDown).Text, out outval))
                {
                    (sender as IntegerUpDown).Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                    EffectConfig.Angle = outval;
                    FireEffectConfigUpdated();
                }
                else
                {
                    (sender as IntegerUpDown).Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    (sender as IntegerUpDown).ToolTip = "Entered value is not a number";
                }
            }
        }

        private void effect_animation_type_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                EffectConfig.AnimationType = (AnimationType)effect_animation_type.SelectedValue;
            }
        }

        private void effect_animation_reversed_Checked(object? sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                EffectConfig.AnimationReverse = (effect_animation_reversed.IsChecked.HasValue ? effect_animation_reversed.IsChecked.Value : false);
            }
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member