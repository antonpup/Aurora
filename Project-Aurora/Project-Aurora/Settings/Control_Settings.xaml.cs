using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Xceed.Wpf.Toolkit;
using Aurora.Profiles.Desktop;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_Settings.xaml
    /// </summary>
    public partial class Control_Settings : UserControl
    {
        private RegistryKey runRegistryPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private const string StartupTaskID = "AuroraStartup";

        private Window winBitmapView = null;
        private Image imgBitmap = new Image();
        private static bool bitmapViewOpen;

        public Control_Settings()
        {
            InitializeComponent();

            this.tabMain.DataContext = Global.Configuration;

            if (runRegistryPath.GetValue("Aurora") != null)
                runRegistryPath.DeleteValue("Aurora");

            try
            {
                using (TaskService service = new TaskService())
                {
                    Microsoft.Win32.TaskScheduler.Task task = service.FindTask(StartupTaskID);
                    if (task != null)
                    {
                        TaskDefinition definition = task.Definition;
                        //Update path of startup task
                        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        definition.Actions.Clear();
                        definition.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));
                        service.RootFolder.RegisterTaskDefinition(StartupTaskID, definition);
                        this.run_at_win_startup.IsChecked = task.Enabled;
                    }
                    else
                    {
                        TaskDefinition td = service.NewTask();
                        td.RegistrationInfo.Description = "Start Aurora on Startup";

                        td.Triggers.Add(new LogonTrigger { Enabled = true });

                        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                        td.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));

                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        td.Settings.DisallowStartIfOnBatteries = false;
                        td.Settings.DisallowStartOnRemoteAppSession = false;
                        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                        service.RootFolder.RegisterTaskDefinition(StartupTaskID, td);
                        this.run_at_win_startup.IsChecked = true;
                    }
                }
            }
            catch(Exception exc)
            {
                Global.logger.Error("Error caught when updating startup task. Error: " + exc.ToString());
            }

            string v = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

            this.lblVersion.Content = ((int.Parse(v[0].ToString()) > 0) ? "" : "beta ") + $"v{v}" + " by Antonpup & simon-wh";

            this.start_silently_enabled.IsChecked = Global.Configuration.start_silently;

            this.app_exit_mode.SelectedIndex = (int)Global.Configuration.close_mode;
            this.app_detection_mode.SelectedIndex = (int)Global.Configuration.detection_mode;
            this.chkOverlayPreview.IsChecked = Global.Configuration.OverlaysInPreview;

            load_excluded_listbox();

            this.volume_as_brightness_enabled.IsChecked = Global.Configuration.UseVolumeAsBrightness;

            this.timed_dimming_checkbox.IsChecked = Global.Configuration.time_based_dimming_enabled;
            this.timed_dimming_start_hour_updown.Value = Global.Configuration.time_based_dimming_start_hour;
            this.timed_dimming_start_minute_updown.Value = Global.Configuration.time_based_dimming_start_minute;
            this.timed_dimming_end_hour_updown.Value = Global.Configuration.time_based_dimming_end_hour;
            this.timed_dimming_end_minute_updown.Value = Global.Configuration.time_based_dimming_end_minute;
            this.timed_dimming_with_games_checkbox.IsChecked = Global.Configuration.time_based_dimming_affect_games;

            this.nighttime_enabled_checkbox.IsChecked = Global.Configuration.nighttime_enabled;
            this.nighttime_start_hour_updown.Value = Global.Configuration.nighttime_start_hour;
            this.nighttime_start_minute_updown.Value = Global.Configuration.nighttime_start_minute;
            this.nighttime_end_hour_updown.Value = Global.Configuration.nighttime_end_hour;
            this.nighttime_end_minute_updown.Value = Global.Configuration.nighttime_end_minute;


            this.volume_overlay_enabled.IsChecked = Global.Configuration.volume_overlay_settings.enabled;
            this.volume_low_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.volume_overlay_settings.low_color);
            this.volume_med_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.volume_overlay_settings.med_color);
            this.volume_high_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.volume_overlay_settings.high_color);
            this.volume_ks.Sequence = Global.Configuration.volume_overlay_settings.sequence;
            this.volume_effects_delay.Value = Global.Configuration.volume_overlay_settings.delay;
            this.volume_overlay_dim_background.IsChecked = Global.Configuration.volume_overlay_settings.dim_background;
            this.volume_overlay_dim_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.volume_overlay_settings.dim_color);

            this.skype_overlay_enabled.IsChecked = Global.Configuration.skype_overlay_settings.enabled;
            this.skype_unread_messages_enabled.IsChecked = Global.Configuration.skype_overlay_settings.mm_enabled;
            this.skype_unread_primary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.mm_color_primary);
            this.skype_unread_secondary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.mm_color_secondary);
            this.skype_unread_messages_ks.Sequence = Global.Configuration.skype_overlay_settings.mm_sequence;
            this.skype_incoming_calls_enabled.IsChecked = Global.Configuration.skype_overlay_settings.call_enabled;
            this.skype_incoming_calls_primary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.call_color_primary);
            this.skype_incoming_calls_secondary_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.skype_overlay_settings.call_color_secondary);
            this.skype_incoming_calls_ks.Sequence = Global.Configuration.skype_overlay_settings.call_sequence;

            this.idle_effects_type.SelectedIndex = (int)Global.Configuration.idle_type;
            this.idle_effects_delay.Value = Global.Configuration.idle_delay;
            this.idle_effects_primary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.idle_effect_primary_color);
            this.idle_effects_secondary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.idle_effect_secondary_color);
            this.idle_effects_speed_label.Text = "x " + Global.Configuration.idle_speed;
            this.idle_effects_speed_slider.Value = (float)Global.Configuration.idle_speed;
            this.idle_effects_amount.Value = Global.Configuration.idle_amount;
            this.idle_effects_frequency.Value = (int)Global.Configuration.idle_frequency;

            this.devices_kb_brand.SelectedItem = Global.Configuration.keyboard_brand;
            this.devices_kb_layout.SelectedIndex = (int)Global.Configuration.keyboard_localization;
            this.devices_mouse_brand.SelectedItem = Global.Configuration.mouse_preference;
            this.devices_mouse_orientation.SelectedItem = Global.Configuration.mouse_orientation;
            this.ComboBox_virtualkeyboard_keycap_type.SelectedItem = Global.Configuration.virtualkeyboard_keycap_type;
            this.wrapper_allow_in_background_enabled.IsChecked = Global.Configuration.allow_wrappers_in_background;
            this.devices_disable_keyboard_lighting.IsChecked = Global.Configuration.devices_disable_keyboard;
            this.devices_disable_mouse_lighting.IsChecked = Global.Configuration.devices_disable_mouse;
            this.devices_disable_headset_lighting.IsChecked = Global.Configuration.devices_disable_headset;

            this.updates_autocheck_on_start.IsChecked = Global.Configuration.updates_check_on_start_up;
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
                                    //Fix conflict with AtomOrb due to async
                                    lock (map)
                                    {
                                        map.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                    }
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
                Global.logger.Warn(ex.ToString());
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.effengine.NewLayerRender += OnLayerRendered;
            this.ctrlPluginManager.Host = Global.PluginManager;
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

        private void app_detection_mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.detection_mode = (ApplicationDetectionMode)Enum.Parse(typeof(ApplicationDetectionMode), this.app_detection_mode.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_as_brightness_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.UseVolumeAsBrightness = (this.volume_as_brightness_enabled.IsChecked.HasValue) ? this.volume_as_brightness_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.time_based_dimming_enabled = (this.timed_dimming_checkbox.IsChecked.HasValue) ? this.timed_dimming_checkbox.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_start_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.time_based_dimming_start_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_start_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.time_based_dimming_start_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_end_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.time_based_dimming_end_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_end_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.time_based_dimming_end_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void timed_dimming_with_games_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.time_based_dimming_affect_games = (this.timed_dimming_with_games_checkbox.IsChecked.HasValue) ? this.timed_dimming_with_games_checkbox.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.idle_type = (IdleEffects)Enum.Parse(typeof(IdleEffects), this.idle_effects_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && idle_effects_delay.Value.HasValue)
            {
                Global.Configuration.idle_delay = idle_effects_delay.Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && idle_effects_amount.Value.HasValue)
            {
                Global.Configuration.idle_amount = idle_effects_amount.Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_frequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && idle_effects_frequency.Value.HasValue)
            {
                Global.Configuration.idle_frequency = (float)idle_effects_frequency.Value.Value;
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
                Global.Configuration.idle_effect_primary_color = Utils.ColorUtils.MediaColorToDrawingColor(this.idle_effects_primary_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_secondary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.idle_effects_secondary_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.idle_effect_secondary_color = Utils.ColorUtils.MediaColorToDrawingColor(this.idle_effects_secondary_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void idle_effects_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded)
            {
                if (IsLoaded)
                {
                    Global.Configuration.idle_speed = (float)this.idle_effects_speed_slider.Value;
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
            Window_ProcessSelection dialog = new Window_ProcessSelection { ButtonLabel = "Exclude Process" };
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ChosenExecutableName)) // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition
                Global.Configuration.excluded_programs.Add(dialog.ChosenExecutableName);

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

        private void sliderPercentages_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider sld = sender as Slider;
            if (sld == null)
                return;

            TextBlock label = sld.Tag as TextBlock;

            if (label == null)
                return;

            label.Text = (int)(sld.Value * 100) + " %";
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider sld = sender as Slider;
            if (sld == null)
                return;

            TextBlock label = sld.Tag as TextBlock;

            if (label == null)
                return;

            label.Text = sld.Value.ToString();
        }

        private void run_at_win_startup_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                try
                {
                    using (TaskService ts = new TaskService())
                    {
                        //Find existing task
                        var task = ts.FindTask(StartupTaskID);
                        task.Enabled = (sender as CheckBox).IsChecked.Value;
                    }
                }
                catch(Exception exc)
                {
                    Global.logger.Error("run_at_win_startup_Checked Exception: " + exc);
                }
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

        private void devices_retry_Click(object sender, RoutedEventArgs e)
        {
            Global.dev_manager.Initialize();
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
        private void devices_view_first_time_steelseries_Click(object sender, RoutedEventArgs e)
        {
            Devices.SteelSeries.SteelSeriesInstallInstructions instructions = new Devices.SteelSeries.SteelSeriesInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_dualshock_Click(object sender, RoutedEventArgs e)
        {
            Devices.Dualshock.DualshockInstallInstructions instructions = new Devices.Dualshock.DualshockInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_roccat_Click(object sender, RoutedEventArgs e)
        {
            Devices.Roccat.RoccatInstallInstructions instructions = new Devices.Roccat.RoccatInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_enable_logitech_color_enhance_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                //Global.Configuration.logitech_enhance_brightness = (this.devices_enable_logitech_color_enhance.IsChecked.HasValue) ? this.devices_enable_logitech_color_enhance.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void wrapper_allow_in_background_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.allow_wrappers_in_background = (this.wrapper_allow_in_background_enabled.IsChecked.HasValue) ? this.wrapper_allow_in_background_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void updates_check_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                string updater_path = System.IO.Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

                if (File.Exists(updater_path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = updater_path;
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

        private void nighttime_enabled_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.nighttime_enabled = (this.nighttime_enabled_checkbox.IsChecked.HasValue) ? this.nighttime_enabled_checkbox.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_start_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.nighttime_start_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_start_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.nighttime_start_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_end_hour_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.nighttime_end_hour = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void nighttime_end_minute_updown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && (sender as IntegerUpDown).Value.HasValue)
            {
                Global.Configuration.nighttime_end_minute = (sender as IntegerUpDown).Value.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void devices_kb_layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.keyboard_localization = (PreferredKeyboardLocalization)Enum.Parse(typeof(PreferredKeyboardLocalization), this.devices_kb_layout.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);

                Global.kbLayout.LoadBrandDefault();
            }
        }

        private void devices_kb_brand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.keyboard_brand = (PreferredKeyboard)Enum.Parse(typeof(PreferredKeyboard), this.devices_kb_brand.SelectedItem.ToString());
                ConfigManager.Save(Global.Configuration);

                Global.kbLayout.LoadBrandDefault();
            }
        }

        private void devices_mouse_brand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.mouse_preference = (PreferredMouse)Enum.Parse(typeof(PreferredMouse), this.devices_mouse_brand.SelectedItem.ToString());
                ConfigManager.Save(Global.Configuration);

                Global.kbLayout.LoadBrandDefault();
            }
        }

        private void devices_mouse_orientation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.mouse_orientation = (MouseOrientationType)Enum.Parse(typeof(MouseOrientationType), this.devices_mouse_orientation.SelectedItem.ToString());
                ConfigManager.Save(Global.Configuration);

                Global.kbLayout.LoadBrandDefault();
            }
        }

        private void ComboBox_virtualkeyboard_keycap_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.virtualkeyboard_keycap_type = (KeycapType)Enum.Parse(typeof(KeycapType), this.ComboBox_virtualkeyboard_keycap_type.SelectedItem.ToString());
                ConfigManager.Save(Global.Configuration);

                Global.kbLayout.LoadBrandDefault();
            }
        }

        private void devices_disable_keyboard_lighting_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.devices_disable_keyboard = ((sender as CheckBox).IsChecked.HasValue) ? (sender as CheckBox).IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);

                Global.dev_manager.ResetDevices();
            }
        }

        private void devices_disable_mouse_lighting_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.devices_disable_mouse = ((sender as CheckBox).IsChecked.HasValue) ? (sender as CheckBox).IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);

                Global.dev_manager.ResetDevices();
            }
        }

        private void devices_disable_headset_lighting_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.devices_disable_headset = ((sender as CheckBox).IsChecked.HasValue) ? (sender as CheckBox).IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);

                Global.dev_manager.ResetDevices();
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
            startInfo.FileName = Path.Combine(Global.ExecutingDirectory, "Aurora-SkypeIntegration.exe");
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

        private void start_silently_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.start_silently = (this.start_silently_enabled.IsChecked.HasValue) ? this.start_silently_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void wrapper_install_logitech_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.InstallLogitech();
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during Logitech Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for Logitech could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_razer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (BinaryWriter razer_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "RzChromaSDK.dll"), FileMode.Create)))
                    {
                        razer_wrapper_86.Write(Properties.Resources.Aurora_RazerLEDWrapper86);
                    }

                    using (BinaryWriter razer_wrapper_64 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "RzChromaSDK64.dll"), FileMode.Create)))
                    {
                        razer_wrapper_64.Write(Properties.Resources.Aurora_RazerLEDWrapper64);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for Razer applied to\r\n" + dialog.SelectedPath);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during Razer Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for Razer could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_lightfx_32_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (BinaryWriter lightfx_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
                    {
                        lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) applied to\r\n" + dialog.SelectedPath);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during LightFX (32 bit) Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_lightfx_64_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (BinaryWriter lightfx_wrapper_64 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
                    {
                        lightfx_wrapper_64.Write(Properties.Resources.Aurora_LightFXWrapper64);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) applied to\r\n" + dialog.SelectedPath);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during LightFX (64 bit) Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void volume_overlay_dim_background_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.volume_overlay_settings.dim_background = (this.volume_overlay_dim_background.IsChecked.HasValue) ? this.volume_overlay_dim_background.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void volume_overlay_dim_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.volume_overlay_dim_color.SelectedColor.HasValue)
            {
                Global.Configuration.volume_overlay_settings.dim_color = Utils.ColorUtils.MediaColorToDrawingColor(this.volume_overlay_dim_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void btnShowBitmapWindow_Click(object sender, RoutedEventArgs e)
        {
            if (winBitmapView == null)
            {
                if (bitmapViewOpen == true)
                {
                    System.Windows.MessageBox.Show("Keyboard Bitmap View already open.\r\nPlease close it.");
                    return;
                }

                winBitmapView = new Window();
                winBitmapView.Closed += WinBitmapView_Closed;
                winBitmapView.ResizeMode = ResizeMode.CanResize;
                //winBitmapView.SizeToContent = SizeToContent.WidthAndHeight;

                winBitmapView.Title = "Keyboard Bitmap View";
                winBitmapView.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                Global.effengine.NewLayerRender += Effengine_NewLayerRender;

                imgBitmap.SnapsToDevicePixels = true;
                imgBitmap.HorizontalAlignment = HorizontalAlignment.Stretch;
                imgBitmap.VerticalAlignment = VerticalAlignment.Stretch;
                /*imgBitmap.MinWidth = 0;
                imgBitmap.MinHeight = 0;*/
                imgBitmap.MinWidth = Effects.canvas_width;
                imgBitmap.MinHeight = Effects.canvas_height;

                winBitmapView.Content = imgBitmap;

                winBitmapView.UpdateLayout();
                winBitmapView.Show();
            }
            else
            {
                winBitmapView.BringIntoView();
            }
        }

        private void Effengine_NewLayerRender(System.Drawing.Bitmap bitmap)
        {
            try
            {
                Dispatcher.Invoke(
                    () =>
                    {
                        lock (bitmap)
                        {
                            using (MemoryStream memory = new MemoryStream())
                            {
                                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                memory.Position = 0;
                                BitmapImage bitmapimage = new BitmapImage();
                                bitmapimage.BeginInit();
                                bitmapimage.StreamSource = memory;
                                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapimage.EndInit();

                                imgBitmap.Source = bitmapimage;
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                Global.logger.Warn(ex.ToString());
            }
        }

        private void WinBitmapView_Closed(object sender, EventArgs e)
        {
            winBitmapView = null;
            Global.effengine.NewLayerRender -= Effengine_NewLayerRender;
            bitmapViewOpen = false;
        }

        private void btnShowLogsFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
                System.Diagnostics.Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora/Logs/"));
        }

        private void chkOverlayPreview_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.OverlaysInPreview = (this.chkOverlayPreview.IsChecked.HasValue) ? this.chkOverlayPreview.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void chkHigherPriority_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;
        }

        private void btnShowGSILog_Click(object sender, RoutedEventArgs e) => new Window_GSIHttpDebug().Show();
    }
}
