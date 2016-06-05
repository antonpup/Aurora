using Aurora.Controls;
using Aurora.Settings;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Aurora.Profiles.Payday_2
{
    /// <summary>
    /// Interaction logic for Control_PD2.xaml
    /// </summary>
    public partial class Control_PD2 : UserControl
    {
        public Control_PD2()
        {
            InitializeComponent();

            this.game_enabled.IsChecked = Global.Configuration.pd2_settings.isEnabled;

            this.background_enabled.IsChecked = Global.Configuration.pd2_settings.bg_enabled;
            this.ambient_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.ambient_color);
            this.assault_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.assault_color);
            this.winters_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.winters_color);
            this.assault_fade_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.assault_fade_color);
            this.bg_assaultspeed.Value = Global.Configuration.pd2_settings.assault_speed_mult;
            this.bg_assaultspeed_label.Content = "x " + Global.Configuration.pd2_settings.assault_speed_mult.ToString("0.00");
            this.assault_animation_enabled.IsChecked = Global.Configuration.pd2_settings.assault_animation_enabled;
            this.low_suspicion_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.low_suspicion_color);
            this.medium_suspicion_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.medium_suspicion_color);
            this.high_suspicion_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.high_suspicion_color);
            this.suspicion_effect_type.SelectedIndex = (int)Global.Configuration.pd2_settings.suspicion_effect_type;
            this.background_show_suspicion.IsChecked = Global.Configuration.pd2_settings.bg_show_suspicion;

            this.health_enabled.IsChecked = Global.Configuration.pd2_settings.health_enabled;
            this.health_healthy_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.healthy_color);
            this.health_hurt_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.hurt_color);
            this.health_effect_type.SelectedIndex = (int)Global.Configuration.pd2_settings.health_effect_type;
            this.hp_keysequence.Sequence = Global.Configuration.pd2_settings.health_sequence;

            this.ammo_enabled.IsChecked = Global.Configuration.pd2_settings.ammo_enabled;
            this.ammo_hasammo_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.ammo_color);
            this.ammo_noammo_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.noammo_color);
            this.ammo_effect_type.SelectedIndex = (int)Global.Configuration.pd2_settings.ammo_effect_type;
            this.ammo_keysequence.Sequence = Global.Configuration.pd2_settings.ammo_sequence;

            this.downed_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.downed_color);
            this.arrested_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.arrested_color);

            this.swansong_enabled.IsChecked = Global.Configuration.pd2_settings.swansong_enabled;
            this.swansong_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.pd2_settings.swansong_color);
            this.swansong_speed.Value = Global.Configuration.pd2_settings.swansong_speed_mult;
            this.swansong_speed_label.Content = "x " + Global.Configuration.pd2_settings.assault_speed_mult.ToString("0.00");

            this.cz.ColorZonesList = Global.Configuration.pd2_settings.lighting_areas;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void get_hook_button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(@"http://paydaymods.com/download/"));
        }

        private void install_mod_button_Click(object sender, RoutedEventArgs e)
        {
            string pd2path = Utils.SteamUtils.GetGamePath(218620);

            if(!String.IsNullOrWhiteSpace(pd2path))
            {
                if(Directory.Exists(pd2path))
                {
                    if(Directory.Exists(System.IO.Path.Combine(pd2path, "mods")))
                    {
                        using (MemoryStream gsi_pd2_ms = new MemoryStream(Properties.Resources.PD2_GSI))
                        {
                            using (ZipFile zip = ZipFile.Read(gsi_pd2_ms))
                            {
                                foreach (ZipEntry entry in zip)
                                {
                                    entry.Extract(pd2path, ExtractExistingFileAction.OverwriteSilently);
                                }
                            }

                        }

                        System.Windows.MessageBox.Show("GSI for Payday 2 installed.");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("BLT Hook was not found.\r\nCould not install the GSI mod.");
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Payday 2 directory is not found.\r\nCould not install the GSI mod.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Payday 2 is not installed through Steam.\r\nCould not install the GSI mod.");
            }
        }

        private void preview_gamestate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                GameEvent_PD2.SetGameState((GSI.Nodes.GameStates)Enum.Parse(typeof(GSI.Nodes.GameStates), this.preview_gamestate.SelectedIndex.ToString()));
            }
        }

        private void preview_levelphase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                GameEvent_PD2.SetLevelPhase((GSI.Nodes.LevelPhase)Enum.Parse(typeof(GSI.Nodes.LevelPhase), this.preview_levelphase.SelectedIndex.ToString()));
            }
        }

        private void preview_playerstate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                GameEvent_PD2.SetPlayerState((GSI.Nodes.PlayerState)Enum.Parse(typeof(GSI.Nodes.PlayerState), this.preview_playerstate.SelectedIndex.ToString()));
            }
        }

        private void preview_health_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hp_val = (int)this.preview_health_slider.Value;
            if (this.preview_health_amount is Label)
            {
                this.preview_health_amount.Content = hp_val + "%";
                GameEvent_PD2.SetHealth(hp_val);
                GameEvent_PD2.SetHealthMax(100);
            }
        }

        private void preview_ammo_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int ammo_val = (int)this.preview_ammo_slider.Value;
            if (this.preview_ammo_amount is Label)
            {
                this.preview_ammo_amount.Content = ammo_val + "%";
                GameEvent_PD2.SetClip(ammo_val);
                GameEvent_PD2.SetClipMax(100);
            }
        }

        private void preview_suspicion_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float susp_val = (float)this.preview_suspicion_slider.Value;
            if (this.preview_suspicion_amount is Label)
            {
                this.preview_suspicion_amount.Content = (int)susp_val + "%";
                GameEvent_PD2.SetSuspicion(susp_val / 100.0f);
            }
        }

        private void preview_flashbang_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float flash_val = (float)this.preview_flashbang_slider.Value;
            if (this.preview_flashbang_amount is Label)
            {
                this.preview_flashbang_amount.Content = (int)flash_val + "%";
                GameEvent_PD2.SetFlashbangAmount(flash_val / 100.0f);
            }
        }

        private void preview_swansong_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                GameEvent_PD2.SetSwanSong((sender as CheckBox).IsChecked.Value);
            }
        }

        private void background_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.bg_enabled = (this.background_enabled.IsChecked.HasValue) ? this.background_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ambient_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ambient_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.ambient_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ambient_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void assault_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.assault_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.assault_color = Utils.ColorUtils.MediaColorToDrawingColor(this.assault_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void winters_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.winters_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.winters_color = Utils.ColorUtils.MediaColorToDrawingColor(this.winters_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bg_assaultspeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && sender is Slider)
            {
                Global.Configuration.pd2_settings.assault_speed_mult = (float)((sender as Slider).Value);
                this.bg_assaultspeed_label.Content = "x " + Global.Configuration.pd2_settings.assault_speed_mult.ToString("0.00");
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void assault_fade_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.assault_fade_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.assault_fade_color = Utils.ColorUtils.MediaColorToDrawingColor(this.assault_fade_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void assault_animation_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.assault_animation_enabled = (this.assault_animation_enabled.IsChecked.HasValue) ? this.assault_animation_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void background_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.bg_peripheral_use = (this.background_peripheral_use.IsChecked.HasValue) ? this.background_peripheral_use.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void background_show_suspicion_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.bg_show_suspicion = (this.background_show_suspicion.IsChecked.HasValue) ? this.background_show_suspicion.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void low_suspicion_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.low_suspicion_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.low_suspicion_color = Utils.ColorUtils.MediaColorToDrawingColor(this.low_suspicion_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void medium_suspicion_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.medium_suspicion_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.medium_suspicion_color = Utils.ColorUtils.MediaColorToDrawingColor(this.medium_suspicion_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void high_suspicion_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.high_suspicion_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.high_suspicion_color = Utils.ColorUtils.MediaColorToDrawingColor(this.high_suspicion_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void suspicion_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.suspicion_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.suspicion_effect_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void health_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.health_enabled = (this.health_enabled.IsChecked.HasValue) ? this.health_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void health_healthy_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_healthy_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.healthy_color = Utils.ColorUtils.MediaColorToDrawingColor(this.health_healthy_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void health_hurt_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_hurt_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.hurt_color = Utils.ColorUtils.MediaColorToDrawingColor(this.health_hurt_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void health_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.health_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.health_effect_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void hp_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.health_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.ammo_enabled = (this.ammo_enabled.IsChecked.HasValue) ? this.ammo_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_hasammo_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ammo_hasammo_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.ammo_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ammo_hasammo_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_noammo_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ammo_noammo_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.noammo_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ammo_noammo_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.ammo_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.ammo_effect_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.ammo_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void downed_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.downed_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.downed_color = Utils.ColorUtils.MediaColorToDrawingColor(this.downed_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void arrested_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.arrested_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.arrested_color = Utils.ColorUtils.MediaColorToDrawingColor(this.arrested_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void swansong_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.swansong_enabled = (this.swansong_enabled.IsChecked.HasValue) ? this.swansong_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void swansong_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.swansong_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.pd2_settings.swansong_color = Utils.ColorUtils.MediaColorToDrawingColor(this.swansong_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void swansong_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && sender is Slider)
            {
                Global.Configuration.pd2_settings.swansong_speed_mult = (float)((sender as Slider).Value);
                this.swansong_speed_label.Content = "x " + Global.Configuration.pd2_settings.swansong_speed_mult.ToString("0.00");
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.pd2_settings.lighting_areas = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Predefined, "payday2_win32_release.exe");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
        }
    }
}
