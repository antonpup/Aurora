using Aurora.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
using Aurora.Profiles.Desktop;
using Microsoft.Win32;
using System.Diagnostics;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_Settings.xaml
    /// </summary>
    public partial class Control_Settings : UserControl
    {
        private RegistryKey runRegistryPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public Control_Settings()
        {
            InitializeComponent();

            this.run_at_win_startup.IsChecked = !(runRegistryPath.GetValue("Aurora", null) == null);

            this.app_exit_mode.SelectedIndex = (int)Global.Configuration.close_mode;

            this.volume_as_brightness_enabled.IsChecked = Global.Configuration.use_volume_as_brightness;

            this.brightness_kb_label.Text = Global.Configuration.keyboard_brightness_modifier + " %";
            this.brightness_kb_slider.Value = (float)Global.Configuration.keyboard_brightness_modifier;

            this.brightness_peri_label.Text = Global.Configuration.peripheral_brightness_modifier + " %";
            this.brightness_peri_slider.Value = (float)Global.Configuration.peripheral_brightness_modifier;

            this.timed_dimming_checkbox.IsChecked = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_enabled;
            this.timed_dimming_start_hour_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_start_hour;
            this.timed_dimming_start_minute_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_start_minute;
            this.timed_dimming_end_hour_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_end_hour;
            this.timed_dimming_end_minute_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_end_minute;
            this.timed_dimming_with_games_checkbox.IsChecked = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_affect_games;

            this.nighttime_enabled_checkbox.IsChecked = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_enabled;
            this.nighttime_start_hour_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_start_hour;
            this.nighttime_start_minute_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_start_minute;
            this.nighttime_end_hour_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_end_hour;
            this.nighttime_end_minute_updown.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_end_minute;

            this.volume_overlay_enabled.IsChecked = Global.Configuration.volume_overlay_settings.enabled;
            this.volume_low_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.volume_overlay_settings.low_color);
            this.volume_med_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.volume_overlay_settings.med_color);
            this.volume_high_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.volume_overlay_settings.high_color);
            this.volume_ks.Sequence = Global.Configuration.volume_overlay_settings.sequence;
            this.volume_effects_delay.Value = Global.Configuration.volume_overlay_settings.delay;

            this.skype_overlay_enabled.IsChecked = Global.Configuration.skype_overlay_settings.enabled;
            this.skype_unread_messages_enabled.IsChecked = Global.Configuration.skype_overlay_settings.mm_enabled;
            this.skype_unread_primary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.mm_color_primary);
            this.skype_unread_secondary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.mm_color_secondary);
            this.skype_unread_messages_ks.Sequence = Global.Configuration.skype_overlay_settings.mm_sequence;
            this.skype_incoming_calls_enabled.IsChecked = Global.Configuration.skype_overlay_settings.call_enabled;
            this.skype_incoming_calls_primary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.call_color_primary);
            this.skype_incoming_calls_secondary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.call_color_secondary);
            this.skype_incoming_calls_ks.Sequence = Global.Configuration.skype_overlay_settings.call_sequence;

            this.idle_effects_type.SelectedIndex = (int)(Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_type;
            this.idle_effects_delay.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_delay;
            this.idle_effects_primary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_primary_color);
            this.idle_effects_secondary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_secondary_color);
            this.idle_effects_speed_label.Text = "x " + (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_speed;
            this.idle_effects_speed_slider.Value = (float)(Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_speed;
            this.idle_effects_amount.Value = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_amount;
            this.idle_effects_frequency.Value = (int)(Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_frequency;

            this.devices_kb_brand.SelectedIndex = (int)Global.Configuration.keyboard_brand;
            this.devices_kb_layout.SelectedIndex = (int)Global.Configuration.keyboard_localization;
            this.devices_enable_logitech_color_enhance.IsChecked = Global.Configuration.logitech_enhance_brightness;

            this.updates_autocheck_on_start.IsChecked = Global.Configuration.updates_check_on_start_up;
            this.updates_background_install_minor.IsChecked = Global.Configuration.updates_allow_silent_minor;

            Global.effengine.NewLayerRender += OnLayerRendered;
        }

        private void OnLayerRendered(System.Drawing.Bitmap map)
        {
            try
            {
                Dispatcher.Invoke(
                            () =>
                            {
                                using (MemoryStream memory = new MemoryStream())
                                {
                                    map.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                    memory.Position = 0;
                                    BitmapImage bitmapimage = new BitmapImage();
                                    bitmapimage.BeginInit();
                                    bitmapimage.StreamSource = memory;
                                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmapimage.EndInit();

                                    this.debug_bitmap_preview.Width = 4 * bitmapimage.Width;
                                    this.debug_bitmap_preview.Height = 4 * bitmapimage.Height;
                                    this.debug_bitmap_preview.Source = bitmapimage;
                                }
                            });
            }
            catch (Exception ex)
            {
                Global.logger.LogLine(ex.ToString(), Logging_Level.Warning);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.about_connected_devices.Text = "Connected Devices\r\n" + Global.dev_manager.GetDevices();
            Global.effengine.NewLayerRender += OnLayerRendered;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.effengine.NewLayerRender -= OnLayerRendered;
        }

        private void app_exit_mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.close_mode = (AppExitMode)Enum.Parse(typeof(AppExitMode), this.app_exit_mode.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_as_brightness_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.use_volume_as_brightness = (this.volume_as_brightness_enabled.IsChecked.HasValue) ? this.volume_as_brightness_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_enabled = (this.timed_dimming_checkbox.IsChecked.HasValue) ? this.timed_dimming_checkbox.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_start_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_start_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_start_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_start_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_end_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_end_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_end_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_end_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_with_games_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_affect_games = (this.timed_dimming_with_games_checkbox.IsChecked.HasValue) ? this.timed_dimming_with_games_checkbox.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_type = (IdleEffects)Enum.Parse(typeof(IdleEffects), this.idle_effects_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && idle_effects_delay.Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_delay = idle_effects_delay.Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && idle_effects_amount.Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_amount = idle_effects_amount.Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_frequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && idle_effects_frequency.Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_frequency = (float)idle_effects_frequency.Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        //// Misc

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void RecordKeySequence(string whoisrecording, Button button, ListBox sequence_listbox)
        {
            if (Global.key_recorder.IsRecording())
            {
                if (Global.key_recorder.GetRecordingType().Equals(whoisrecording))
                {
                    Global.key_recorder.StopRecording();

                    button.Content = "Assign Keys";

                    Devices.DeviceKeys[] recorded_keys = Global.key_recorder.GetKeys();

                    if (sequence_listbox.SelectedIndex > 0 && sequence_listbox.SelectedIndex < (sequence_listbox.Items.Count - 1))
                    {
                        int insertpos = sequence_listbox.SelectedIndex;
                        foreach (var key in recorded_keys)
                        {
                            sequence_listbox.Items.Insert(insertpos, key);
                            insertpos++;
                        }
                    }
                    else
                    {
                        foreach (var key in recorded_keys)
                            sequence_listbox.Items.Add(key);
                    }

                    Global.key_recorder.Reset();
                }
                else
                {
                    System.Windows.MessageBox.Show("You are already recording a key sequence for " + Global.key_recorder.GetRecordingType());
                }
            }
            else
            {
                Global.key_recorder.StartRecording(whoisrecording);
                button.Content = "Stop Assigning";
            }
        }

        private void idle_effects_primary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.idle_effects_primary_color_colorpicker.SelectedColor.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_primary_color = Utils.ColorUtils.MediaColorToDrawingColor(this.idle_effects_primary_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_secondary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.idle_effects_secondary_color_colorpicker.SelectedColor.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_secondary_color = Utils.ColorUtils.MediaColorToDrawingColor(this.idle_effects_secondary_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
            {
                if (IsLoaded)
                {
                    (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_speed = (float)this.idle_effects_speed_slider.Value;
                    ConfigManager.Save(Global.Configuration);
                }

                if (this.idle_effects_speed_label is TextBlock)
                {
                    this.idle_effects_speed_label.Text = "x " + this.idle_effects_speed_slider.Value;
                }
            }
        }

        private void load_excluded_listbox()
        {
            this.excluded_listbox.Items.Clear();

            string[] processes = Global.Configuration.excluded_programs.ToArray();

            foreach (string process in processes)
                this.excluded_listbox.Items.Add(process);
        }


        private void excluded_add_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(this.excluded_process_name.Text))
            {
                if (!Global.Configuration.excluded_programs.Contains(this.excluded_process_name.Text))
                {
                    Global.Configuration.excluded_programs.Add(this.excluded_process_name.Text);
                }
            }

            load_excluded_listbox();
        }

        private void excluded_remove_Click(object sender, RoutedEventArgs e)
        {
            if (this.excluded_listbox.SelectedItem != null)
            {
                if (Global.Configuration.excluded_programs.Contains((string)this.excluded_listbox.SelectedItem))
                {
                    Global.Configuration.excluded_programs.Remove((string)this.excluded_listbox.SelectedItem);
                }
            }

            load_excluded_listbox();
        }

        private void desktop_cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).lighting_areas = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ks_cpu_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).cpu_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ks_ram_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ram_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_overlay_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.volume_overlay_settings.enabled = (this.volume_overlay_enabled.IsChecked.HasValue) ? this.volume_overlay_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_low_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.volume_low_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.volume_overlay_settings.low_color = Utils.ColorUtils.MediaColorToDrawingColor(this.volume_low_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_med_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.volume_med_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.volume_overlay_settings.med_color = Utils.ColorUtils.MediaColorToDrawingColor(this.volume_med_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_high_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.volume_high_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.volume_overlay_settings.high_color = Utils.ColorUtils.MediaColorToDrawingColor(this.volume_high_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.volume_overlay_settings.sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Global.effengine.ToggleRecord();

            if (Global.effengine.isrecording)
                (sender as Button).Content = "Stop Recording";
            else
                (sender as Button).Content = "Record";
        }

        private void brightness_kb_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                Global.Configuration.keyboard_brightness_modifier = (float)this.brightness_kb_slider.Value;
                ConfigManager.Save(Global.Configuration);
            }

            if (this.brightness_kb_label is TextBlock)
            {
                this.brightness_kb_label.Text = (int)(this.brightness_kb_slider.Value * 100) + " %";
            }
        }

        private void brightness_peri_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                Global.Configuration.peripheral_brightness_modifier = (float)this.brightness_peri_slider.Value;
                ConfigManager.Save(Global.Configuration);
            }

            if (this.brightness_peri_label is TextBlock)
            {
                this.brightness_peri_label.Text = (int)(this.brightness_peri_slider.Value * 100) + " %";
            }
        }

        private void run_at_win_startup_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                if ((sender as CheckBox).IsChecked.Value)
                    runRegistryPath.SetValue("Aurora", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" -silent -delay 5000");
                else
                    runRegistryPath.DeleteValue("Aurora");
            }

        }

        private void volume_effects_delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && volume_effects_delay.Value.HasValue)
            {
                Global.Configuration.volume_overlay_settings.delay = volume_effects_delay.Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void devices_view_first_time_logitech_Click(object sender, RoutedEventArgs e)
        {
            Devices.Logitech.LogitechInstallInstructions instructions = new Devices.Logitech.LogitechInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_corsair_Click(object sender, RoutedEventArgs e)
        {
            Devices.Corsair.CorsairInstallInstructions instructions = new Devices.Corsair.CorsairInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_razer_Click(object sender, RoutedEventArgs e)
        {
            Devices.Razer.RazerInstallInstructions instructions = new Devices.Razer.RazerInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_enable_logitech_color_enhance_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.logitech_enhance_brightness = (this.devices_enable_logitech_color_enhance.IsChecked.HasValue) ? this.devices_enable_logitech_color_enhance.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void updates_check_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                string updater_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Aurora-Updater.exe");

                if (File.Exists(updater_path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = updater_path;
                    startInfo.Arguments = Global.Configuration.updates_allow_silent_minor ? "-silent_minor" : "";
                    Process.Start(startInfo);
                }
                else
                {
                    System.Windows.MessageBox.Show("Updater is missing!");
                }
            }
        }

        private void updates_autocheck_on_start_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.updates_check_on_start_up = (this.updates_autocheck_on_start.IsChecked.HasValue) ? this.updates_autocheck_on_start.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void updates_background_install_minor_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.updates_allow_silent_minor = (this.updates_background_install_minor.IsChecked.HasValue) ? this.updates_background_install_minor.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_enabled_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_enabled = (this.nighttime_enabled_checkbox.IsChecked.HasValue) ? this.nighttime_enabled_checkbox.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_start_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_start_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_start_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_start_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_end_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_end_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_end_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_end_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void devices_kb_layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.keyboard_localization = (PreferredKeyboardLocalization)Enum.Parse(typeof(PreferredKeyboardLocalization), this.devices_kb_layout.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void devices_kb_brand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.keyboard_brand = (PreferredKeyboard)Enum.Parse(typeof(PreferredKeyboardLocalization), this.devices_kb_brand.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_overlay_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.skype_overlay_settings.enabled = (this.skype_overlay_enabled.IsChecked.HasValue) ? this.skype_overlay_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_run_integration_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"Aurora-SkypeIntegration.exe";
            Process.Start(startInfo);
        }

        private void skype_unread_messages_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.skype_overlay_settings.mm_enabled = (this.skype_unread_messages_enabled.IsChecked.HasValue) ? this.skype_unread_messages_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_unread_primary_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.skype_unread_primary_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.skype_overlay_settings.mm_color_primary = Utils.ColorUtils.MediaColorToDrawingColor(this.skype_unread_primary_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_unread_secondary_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.skype_unread_secondary_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.skype_overlay_settings.mm_color_secondary = Utils.ColorUtils.MediaColorToDrawingColor(this.skype_unread_secondary_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_unread_messages_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.skype_overlay_settings.mm_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_incoming_calls_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.skype_overlay_settings.call_enabled = (this.skype_incoming_calls_enabled.IsChecked.HasValue) ? this.skype_incoming_calls_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_incoming_calls_primary_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.skype_incoming_calls_primary_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.skype_overlay_settings.call_color_primary = Utils.ColorUtils.MediaColorToDrawingColor(this.skype_incoming_calls_primary_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_incoming_calls_secondary_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.skype_incoming_calls_secondary_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.skype_overlay_settings.call_color_secondary = Utils.ColorUtils.MediaColorToDrawingColor(this.skype_incoming_calls_secondary_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void skype_incoming_calls_messages_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.skype_overlay_settings.call_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }
    }
}
