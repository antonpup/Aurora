using Aurora.Controls;
using Aurora.Settings;
using Ionic.Zip;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Profiles.Payday_2
{
    /// <summary>
    /// Interaction logic for Control_PD2.xaml
    /// </summary>
    public partial class Control_PD2 : UserControl
    {
        private ProfileManager profile_manager;

        public Control_PD2(ProfileManager profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            profile_manager.ProfileChanged += Profile_manager_ProfileChanged;
        }

        private void Profile_manager_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.profilemanager.ProfileManager = profile_manager;
            this.scriptmanager.ProfileManager = profile_manager;

            this.game_enabled.IsChecked = (profile_manager.Settings as PD2Settings).isEnabled;

            this.background_enabled.IsChecked = (profile_manager.Settings as PD2Settings).bg_enabled;
            this.ambient_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).ambient_color);
            this.assault_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).assault_color);
            this.winters_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).winters_color);
            this.assault_fade_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).assault_fade_color);
            this.bg_assaultspeed.Value = (profile_manager.Settings as PD2Settings).assault_speed_mult;
            this.bg_assaultspeed_label.Content = "x " + (profile_manager.Settings as PD2Settings).assault_speed_mult.ToString("0.00");
            this.assault_animation_enabled.IsChecked = (profile_manager.Settings as PD2Settings).assault_animation_enabled;
            this.low_suspicion_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).low_suspicion_color);
            this.medium_suspicion_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).medium_suspicion_color);
            this.high_suspicion_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).high_suspicion_color);
            this.suspicion_effect_type.SelectedIndex = (int)(profile_manager.Settings as PD2Settings).suspicion_effect_type;
            this.background_show_suspicion.IsChecked = (profile_manager.Settings as PD2Settings).bg_show_suspicion;

            this.health_enabled.IsChecked = (profile_manager.Settings as PD2Settings).health_enabled;
            this.health_healthy_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).healthy_color);
            this.health_hurt_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).hurt_color);
            this.health_effect_type.SelectedIndex = (int)(profile_manager.Settings as PD2Settings).health_effect_type;
            this.hp_keysequence.Sequence = (profile_manager.Settings as PD2Settings).health_sequence;

            this.ammo_enabled.IsChecked = (profile_manager.Settings as PD2Settings).ammo_enabled;
            this.ammo_hasammo_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).ammo_color);
            this.ammo_noammo_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).noammo_color);
            this.ammo_effect_type.SelectedIndex = (int)(profile_manager.Settings as PD2Settings).ammo_effect_type;
            this.ammo_keysequence.Sequence = (profile_manager.Settings as PD2Settings).ammo_sequence;

            this.downed_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).downed_color);
            this.arrested_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).arrested_color);

            this.swansong_enabled.IsChecked = (profile_manager.Settings as PD2Settings).swansong_enabled;
            this.swansong_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as PD2Settings).swansong_color);
            this.swansong_speed.Value = (profile_manager.Settings as PD2Settings).swansong_speed_mult;
            this.swansong_speed_label.Content = "x " + (profile_manager.Settings as PD2Settings).assault_speed_mult.ToString("0.00");

            this.cz.ColorZonesList = (profile_manager.Settings as PD2Settings).lighting_areas;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
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
                (profile_manager.Settings as PD2Settings).bg_enabled = (this.background_enabled.IsChecked.HasValue) ? this.background_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void ambient_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ambient_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).ambient_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ambient_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void assault_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.assault_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).assault_color = Utils.ColorUtils.MediaColorToDrawingColor(this.assault_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void winters_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.winters_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).winters_color = Utils.ColorUtils.MediaColorToDrawingColor(this.winters_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_assaultspeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && sender is Slider)
            {
                (profile_manager.Settings as PD2Settings).assault_speed_mult = (float)((sender as Slider).Value);
                this.bg_assaultspeed_label.Content = "x " + (profile_manager.Settings as PD2Settings).assault_speed_mult.ToString("0.00");
                profile_manager.SaveProfiles();
            }
        }

        private void assault_fade_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.assault_fade_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).assault_fade_color = Utils.ColorUtils.MediaColorToDrawingColor(this.assault_fade_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void assault_animation_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).assault_animation_enabled = (this.assault_animation_enabled.IsChecked.HasValue) ? this.assault_animation_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void background_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).bg_peripheral_use = (this.background_peripheral_use.IsChecked.HasValue) ? this.background_peripheral_use.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void background_show_suspicion_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).bg_show_suspicion = (this.background_show_suspicion.IsChecked.HasValue) ? this.background_show_suspicion.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void low_suspicion_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.low_suspicion_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).low_suspicion_color = Utils.ColorUtils.MediaColorToDrawingColor(this.low_suspicion_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void medium_suspicion_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.medium_suspicion_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).medium_suspicion_color = Utils.ColorUtils.MediaColorToDrawingColor(this.medium_suspicion_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void high_suspicion_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.high_suspicion_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).high_suspicion_color = Utils.ColorUtils.MediaColorToDrawingColor(this.high_suspicion_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void suspicion_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).suspicion_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.suspicion_effect_type.SelectedIndex.ToString());
                profile_manager.SaveProfiles();
            }
        }

        private void health_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).health_enabled = (this.health_enabled.IsChecked.HasValue) ? this.health_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void health_healthy_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_healthy_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).healthy_color = Utils.ColorUtils.MediaColorToDrawingColor(this.health_healthy_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void health_hurt_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_hurt_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).hurt_color = Utils.ColorUtils.MediaColorToDrawingColor(this.health_hurt_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void health_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).health_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.health_effect_type.SelectedIndex.ToString());
                profile_manager.SaveProfiles();
            }
        }

        private void hp_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).health_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        private void ammo_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).ammo_enabled = (this.ammo_enabled.IsChecked.HasValue) ? this.ammo_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void ammo_hasammo_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ammo_hasammo_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).ammo_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ammo_hasammo_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void ammo_noammo_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ammo_noammo_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).noammo_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ammo_noammo_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void ammo_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).ammo_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.ammo_effect_type.SelectedIndex.ToString());
                profile_manager.SaveProfiles();
            }
        }

        private void ammo_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).ammo_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        private void downed_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.downed_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).downed_color = Utils.ColorUtils.MediaColorToDrawingColor(this.downed_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void arrested_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.arrested_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).arrested_color = Utils.ColorUtils.MediaColorToDrawingColor(this.arrested_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void swansong_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).swansong_enabled = (this.swansong_enabled.IsChecked.HasValue) ? this.swansong_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void swansong_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.swansong_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as PD2Settings).swansong_color = Utils.ColorUtils.MediaColorToDrawingColor(this.swansong_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void swansong_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && sender is Slider)
            {
                (profile_manager.Settings as PD2Settings).swansong_speed_mult = (float)((sender as Slider).Value);
                this.swansong_speed_label.Content = "x " + (profile_manager.Settings as PD2Settings).swansong_speed_mult.ToString("0.00");
                profile_manager.SaveProfiles();
            }
        }

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as PD2Settings).lighting_areas = (sender as ColorZones).ColorZonesList;
                profile_manager.SaveProfiles();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
