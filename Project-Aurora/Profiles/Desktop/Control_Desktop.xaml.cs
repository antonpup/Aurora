using Aurora.Settings;
using Aurora.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Aurora.Profiles.Desktop
{
    /// <summary>
    /// Interaction logic for Control_Desktop.xaml
    /// </summary>
    public partial class Control_Desktop : UserControl
    {
        public Control_Desktop()
        {
            InitializeComponent();

            this.cpu_usage_enabled.IsChecked = Global.Configuration.desktop_settings.cpu_usage_enabled;
            this.cpu_usage_used_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.cpu_used_color);
            this.cpu_usage_free_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.cpu_free_color);
            this.cpu_usage_free_transparent.IsChecked = Global.Configuration.desktop_settings.cpu_free_color_transparent;
            this.cpu_usage_effect_type.SelectedIndex = (int)Global.Configuration.desktop_settings.cpu_usage_effect_type;
            this.ks_cpu.Sequence = Global.Configuration.desktop_settings.cpu_sequence;

            this.ram_usage_enabled.IsChecked = Global.Configuration.desktop_settings.ram_usage_enabled;
            this.ram_usage_used_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.ram_used_color);
            this.ram_usage_free_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.ram_free_color);
            this.ram_usage_free_transparent.IsChecked = Global.Configuration.desktop_settings.ram_free_color_transparent;
            this.ram_usage_effect_type.SelectedIndex = (int)Global.Configuration.desktop_settings.ram_usage_effect_type;
            this.ks_ram.Sequence = Global.Configuration.desktop_settings.ram_sequence;

            this.sc_assistant_enabled.IsChecked = Global.Configuration.desktop_settings.shortcuts_assistant_enabled;
            this.sc_assistant_ctrl_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.ctrl_key_color);
            this.sc_assistant_ctrl_keys.Sequence = Global.Configuration.desktop_settings.ctrl_key_sequence;
            this.sc_assistant_win_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.win_key_color);
            this.sc_assistant_win_keys.Sequence = Global.Configuration.desktop_settings.win_key_sequence;
            this.sc_assistant_alt_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.alt_key_color);
            this.sc_assistant_alt_keys.Sequence = Global.Configuration.desktop_settings.alt_key_sequence;

            this.interactive_effects_usage_enabled.IsChecked = Global.Configuration.desktop_settings.interactive_effects_enabled;
            this.interactive_effects_type.SelectedIndex = (int)Global.Configuration.desktop_settings.interactive_effect_type;
            this.interactive_effects_primary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.interactive_effect_primary_color);
            this.interactive_effects_random_primary_color_enabled.IsChecked = Global.Configuration.desktop_settings.interactive_effects_random_primary_color;
            this.interactive_effects_random_secondary_color_enabled.IsChecked = Global.Configuration.desktop_settings.interactive_effects_random_secondary_color;

            this.interactive_effects_secondary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.desktop_settings.interactive_effect_secondary_color);

            this.interactive_effects_speed_label.Text = "x " + Global.Configuration.desktop_settings.interactive_effect_speed;
            this.interactive_effects_speed_slider.Value = (float)Global.Configuration.desktop_settings.interactive_effect_speed;
            this.interactive_effects_width_label.Text = Global.Configuration.desktop_settings.interactive_effect_width + " px";
            this.interactive_effects_width_slider.Value = (float)Global.Configuration.desktop_settings.interactive_effect_width;

            this.interactive_effects_mouse_interaction_enable.IsChecked = Global.Configuration.desktop_settings.interactive_effects_mouse_clicking;

            this.desktop_cz.ColorZonesList = Global.Configuration.desktop_settings.lighting_areas;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        ////CPU/RAM Settings
        private void cpu_usage_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.cpu_usage_enabled = (this.cpu_usage_enabled.IsChecked.HasValue) ? this.cpu_usage_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cpu_usage_used_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.cpu_usage_used_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.cpu_used_color = Utils.ColorUtils.MediaColorToDrawingColor(this.cpu_usage_used_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cpu_usage_free_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.cpu_usage_free_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.cpu_free_color = Utils.ColorUtils.MediaColorToDrawingColor(this.cpu_usage_free_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cpu_usage_free_transparent_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.cpu_free_color_transparent = (this.cpu_usage_free_transparent.IsChecked.HasValue) ? this.cpu_usage_free_transparent.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }

            this.cpu_usage_free_colorpicker.IsEnabled = !Global.Configuration.desktop_settings.cpu_free_color_transparent;
        }

        private void cpu_usage_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.cpu_usage_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.cpu_usage_effect_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ram_usage_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.ram_usage_enabled = (this.ram_usage_enabled.IsChecked.HasValue) ? this.ram_usage_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ram_usage_used_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ram_usage_used_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.ram_used_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ram_usage_used_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ram_usage_free_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ram_usage_free_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.ram_free_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ram_usage_free_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ram_usage_free_transparent_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.ram_free_color_transparent = (this.ram_usage_free_transparent.IsChecked.HasValue) ? this.ram_usage_free_transparent.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }

            this.ram_usage_free_colorpicker.IsEnabled = !Global.Configuration.desktop_settings.ram_free_color_transparent;
        }

        private void ram_usage_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.ram_usage_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.ram_usage_effect_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.shortcuts_assistant_enabled = (this.sc_assistant_enabled.IsChecked.HasValue) ? this.sc_assistant_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_ctrl_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.sc_assistant_ctrl_color.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.ctrl_key_color = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_ctrl_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_ctrl_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.ctrl_key_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_win_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.sc_assistant_win_color.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.win_key_color = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_win_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_win_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.win_key_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_alt_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.sc_assistant_alt_color.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.alt_key_color = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_alt_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_alt_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.alt_key_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void interactive_effects_usage_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.interactive_effects_enabled = (this.interactive_effects_usage_enabled.IsChecked.HasValue) ? this.interactive_effects_usage_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void interactive_effects_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.interactive_effect_type = (InteractiveEffects)Enum.Parse(typeof(InteractiveEffects), this.interactive_effects_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void interactive_effects_primary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.interactive_effects_primary_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.interactive_effect_primary_color = Utils.ColorUtils.MediaColorToDrawingColor(this.interactive_effects_primary_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void interactive_effects_random_primary_color_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.interactive_effects_random_primary_color = (this.interactive_effects_random_primary_color_enabled.IsChecked.HasValue) ? this.interactive_effects_random_primary_color_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void interactive_effects_secondary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.interactive_effects_secondary_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.desktop_settings.interactive_effect_secondary_color = Utils.ColorUtils.MediaColorToDrawingColor(this.interactive_effects_secondary_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void interactive_effects_random_secondary_color_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.interactive_effects_random_secondary_color = (this.interactive_effects_random_secondary_color_enabled.IsChecked.HasValue) ? this.interactive_effects_random_secondary_color_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void interactive_effects_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
            {
                if (IsLoaded)
                {
                    Global.Configuration.desktop_settings.interactive_effect_speed = (float)this.interactive_effects_speed_slider.Value;
                    ConfigManager.Save(Global.Configuration);
                }

                if (this.interactive_effects_speed_label is TextBlock)
                {
                    this.interactive_effects_speed_label.Text = "x " + this.interactive_effects_speed_slider.Value;
                }
            }
        }

        private void interactive_effects_width_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
            {
                if (IsLoaded)
                {
                    Global.Configuration.desktop_settings.interactive_effect_width = (int)this.interactive_effects_width_slider.Value;
                    ConfigManager.Save(Global.Configuration);
                }

                if (this.interactive_effects_width_label is TextBlock)
                {
                    this.interactive_effects_width_label.Text = this.interactive_effects_width_slider.Value + " px";
                }
            }
        }

        //// Misc

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void desktop_cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.lighting_areas = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ks_cpu_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.cpu_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ks_ram_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.ram_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Global.effengine.ToggleRecord();

            if(Global.effengine.isrecording)
                (sender as Button).Content = "Stop Recording";
            else
                (sender as Button).Content = "Record";
        }

        private void interactive_effects_mouse_interaction_enable_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.desktop_settings.interactive_effects_mouse_clicking = (this.interactive_effects_mouse_interaction_enable.IsChecked.HasValue) ? this.interactive_effects_mouse_interaction_enable.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }
    }
}
