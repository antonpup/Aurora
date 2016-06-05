using Aurora.Settings;
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

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for EffectSettingsWindow.xaml
    /// </summary>
    public partial class EffectSettingsWindow : Window
    {
        public LayerEffectConfig EffectConfig;
        public PreviewType preview = PreviewType.GenericApplication;
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
            FireEffectConfigUpdated();
            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Global.geh.SetPreview(preview, preview_key);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Global.geh.SetPreview(PreviewType.None);
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
    }
}
