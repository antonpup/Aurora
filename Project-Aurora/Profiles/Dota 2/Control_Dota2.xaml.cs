using Aurora.Controls;
using Aurora.Utils;
using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Aurora.Devices;
using Aurora.Settings;

namespace Aurora.Profiles.Dota_2
{
    /// <summary>
    /// Interaction logic for Control_Dota2.xaml
    /// </summary>
    public partial class Control_Dota2 : UserControl
    {
        private ProfileManager profile_manager;

        private int respawn_time = 15;
        private Timer preview_respawn_timer;
        private int killstreak = 0;

        public Control_Dota2()
        {
            InitializeComponent();

            profile_manager = Global.Configuration.ApplicationProfiles["Dota 2"];

            SetSettings();

            preview_respawn_timer = new Timer(1000);
            preview_respawn_timer.Elapsed += new ElapsedEventHandler(preview_respawn_timer_Tick);

            //Copy cfg file if needed
            if (!(profile_manager.Settings as Dota2Settings).first_time_installed)
            {
                InstallGSI();
                (profile_manager.Settings as Dota2Settings).first_time_installed = true;
            }

            profile_manager.ProfileChanged += Control_Dota2_ProfileChanged;

        }

        private void Control_Dota2_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.profilemanager.ProfileManager = profile_manager;
            this.scriptmanager.ProfileManager = profile_manager;

            this.game_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).isEnabled;

            this.preview_team.Items.Add(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.None);
            this.preview_team.Items.Add(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.Dire);
            this.preview_team.Items.Add(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.Radiant);

            this.background_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).bg_team_enabled;
            this.t_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).dire_color);
            this.ct_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).radiant_color);
            this.ambient_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).ambient_color);
            this.background_peripheral_use.IsChecked = (profile_manager.Settings as Dota2Settings).bg_peripheral_use;
            this.background_dim_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).bg_enable_dimming;
            this.background_dim_value.Text = (profile_manager.Settings as Dota2Settings).bg_dim_after + "s";
            this.background_dim_aftertime.Value = (profile_manager.Settings as Dota2Settings).bg_dim_after;
            this.background_enable_respawn_glow.IsChecked = (profile_manager.Settings as Dota2Settings).bg_respawn_glow;
            this.respawn_glow_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_respawn_glow_color);
            this.background_killstreaks_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).bg_display_killstreaks;
            this.background_killstreaks_lines_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).bg_killstreaks_lines;
            this.bg_killstreak_doublekill_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[2]);
            this.bg_killstreak_killingspree_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[3]);
            this.bg_killstreak_dominating_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[4]);
            this.bg_killstreak_megakill_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[5]);
            this.bg_killstreak_unstoppable_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[6]);
            this.bg_killstreak_wickedsick_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[7]);
            this.bg_killstreak_monsterkill_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[8]);
            this.bg_killstreak_godlike_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[9]);
            this.bg_killstreak_godlikeandon_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).bg_killstreakcolors[10]);

            this.health_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).health_enabled;
            this.health_healthy_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).healthy_color);
            this.health_hurt_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).hurt_color);
            this.health_effect_type.SelectedIndex = (int)(profile_manager.Settings as Dota2Settings).health_effect_type;
            this.hp_ks.Sequence = (profile_manager.Settings as Dota2Settings).health_sequence;

            this.mana_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).mana_enabled;
            this.mana_hasmana_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).mana_color);
            this.mana_nomana_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).nomana_color);
            this.mana_effect_type.SelectedIndex = (int)(profile_manager.Settings as Dota2Settings).mana_effect_type;
            this.mana_ks.Sequence = (profile_manager.Settings as Dota2Settings).mana_sequence;

            this.mimic_respawn_timer_checkbox.IsChecked = (profile_manager.Settings as Dota2Settings).mimic_respawn_timer;
            this.mimic_respawn_color_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).mimic_respawn_timer_color);
            this.mimic_respawn_respawning_color_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).mimic_respawn_timer_respawning_color);


            this.abilities_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).abilitykeys_enabled;
            this.abilities_canuse_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).ability_can_use_color);
            this.abilities_cannotuse_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).ability_can_not_use_color);
            UIUtils.SetSingleKey(this.ability_key1_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 0);
            UIUtils.SetSingleKey(this.ability_key2_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 1);
            UIUtils.SetSingleKey(this.ability_key3_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 2);
            UIUtils.SetSingleKey(this.ability_key4_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 3);
            UIUtils.SetSingleKey(this.ability_key5_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 4);
            UIUtils.SetSingleKey(this.ability_key6_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 5);

            this.items_enabled.IsChecked = (profile_manager.Settings as Dota2Settings).items_enabled;
            this.items_use_item_color.IsChecked = (profile_manager.Settings as Dota2Settings).items_use_item_color;
            this.item_color_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).items_color);
            this.item_empty_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).items_empty_color);
            this.item_no_charges_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).items_no_charges_color);
            this.item_on_cooldown_colorpicker.SelectedColor = ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as Dota2Settings).items_on_cooldown_color);
            UIUtils.SetSingleKey(this.item_slot1_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 0);
            UIUtils.SetSingleKey(this.item_slot2_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 1);
            UIUtils.SetSingleKey(this.item_slot3_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 2);
            UIUtils.SetSingleKey(this.item_slot4_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 3);
            UIUtils.SetSingleKey(this.item_slot5_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 4);
            UIUtils.SetSingleKey(this.item_slot6_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 5);
            UIUtils.SetSingleKey(this.stash_slot1_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 6);
            UIUtils.SetSingleKey(this.stash_slot2_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 7);
            UIUtils.SetSingleKey(this.stash_slot3_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 8);
            UIUtils.SetSingleKey(this.stash_slot4_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 9);
            UIUtils.SetSingleKey(this.stash_slot5_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 10);
            UIUtils.SetSingleKey(this.stash_slot6_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 11);

            this.cz.ColorZonesList = (profile_manager.Settings as Dota2Settings).lighting_areas;
        }

        private void preview_respawn_timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(
                        () =>
                        {
                            if (this.respawn_time < 0)
                            {
                                GameEvent_Dota2.SetAlive(true);
                                this.preview_killplayer.IsEnabled = true;

                                preview_respawn_timer.Stop();
                            }
                            else
                            {
                                this.preview_respawn_time.Content = "Seconds to respawn: " + this.respawn_time;
                                GameEvent_Dota2.SetRespawnTime(this.respawn_time);

                                this.respawn_time--;
                            }
                        });
        }

        //Overview

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            if (InstallGSI())
                MessageBox.Show("Aurora GSI Config file installed successfully.");
            else
                MessageBox.Show("Aurora GSI Config file could not be installed.\r\nGame is not installed.");
        }

        private void unpatch_button_Click(object sender, RoutedEventArgs e)
        {
            if (UninstallGSI())
                MessageBox.Show("Aurora GSI Config file uninstalled successfully.");
            else
                MessageBox.Show("Aurora GSI Config file could not be uninstalled.\r\nGame is not installed.");
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        ////Preview

        private void preview_team_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam)this.preview_team.Items[this.preview_team.SelectedIndex])
            {
                case (Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.None):
                    GameEvent_Dota2.SetTeam(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.None);
                    break;
                case (Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.Radiant):
                    GameEvent_Dota2.SetTeam(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.Radiant);
                    break;
                default:
                    GameEvent_Dota2.SetTeam(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.Dire);
                    break;
            }
        }

        private void preview_health_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hp_val = (int)this.preview_health_slider.Value;
            if (this.preview_health_amount is Label)
            {
                this.preview_health_amount.Content = hp_val + "%";
                GameEvent_Dota2.SetHealth(hp_val);
                GameEvent_Dota2.SetHealthMax(100);
            }
        }

        private void preview_mana_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int mana_val = (int)this.preview_mana_slider.Value;
            if (this.preview_mana_amount is Label)
            {
                this.preview_mana_amount.Content = mana_val + "%";
                GameEvent_Dota2.SetMana(mana_val);
                GameEvent_Dota2.SetManaMax(100);
            }
        }

        private void preview_killplayer_Click(object sender, RoutedEventArgs e)
        {
            GameEvent_Dota2.SetAlive(false);
            respawn_time = 15;
            GameEvent_Dota2.SetRespawnTime(this.respawn_time);
            this.preview_killplayer.IsEnabled = false;
            GameEvent_Dota2.SetKillStreak(killstreak = 0);
            this.preview_killstreak_label.Content = "Killstreak: " + this.killstreak;

            preview_respawn_timer.Start();
        }

        private void preview_addkill_Click(object sender, RoutedEventArgs e)
        {
            GameEvent_Dota2.SetKillStreak(killstreak++);
            GameEvent_Dota2.GotAKill();
            this.preview_killstreak_label.Content = "Killstreak: " + this.killstreak;
        }

        ////Background

        private void background_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).bg_team_enabled = (this.background_enabled.IsChecked.HasValue) ? this.background_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void background_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).bg_peripheral_use = (this.background_peripheral_use.IsChecked.HasValue) ? this.background_peripheral_use.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void t_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (this.t_colorpicker.SelectedColor.HasValue)
                (profile_manager.Settings as Dota2Settings).dire_color = ColorUtils.MediaColorToDrawingColor(this.t_colorpicker.SelectedColor.Value);
        }

        private void ct_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ct_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).radiant_color = ColorUtils.MediaColorToDrawingColor(this.ct_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void ambient_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ambient_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).ambient_color = ColorUtils.MediaColorToDrawingColor(this.ambient_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void background_dim_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).bg_enable_dimming = (this.background_dim_enabled.IsChecked.HasValue) ? this.background_dim_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void background_dim_aftertime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)this.background_dim_aftertime.Value;
            if (this.background_dim_value is TextBlock)
            {
                this.background_dim_value.Text = val + "s";
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).bg_dim_after = val;
                    profile_manager.SaveProfiles();
                }
            }
        }

        private void background_killstreaks_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).bg_display_killstreaks = (this.background_killstreaks_enabled.IsChecked.HasValue) ? this.background_killstreaks_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_doublekill_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_doublekill_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[2] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_doublekill_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_killingspree_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_killingspree_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[3] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_killingspree_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_dominating_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_dominating_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[4] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_dominating_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_megakill_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_megakill_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[5] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_megakill_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_unstoppable_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_unstoppable_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[6] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_unstoppable_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_wickedsick_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_wickedsick_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[7] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_wickedsick_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_monsterkill_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_monsterkill_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[8] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_monsterkill_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_godlike_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_godlike_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[9] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_godlike_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_killstreak_godlikeandon_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_killstreak_godlikeandon_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreakcolors[10] = ColorUtils.MediaColorToDrawingColor(this.bg_killstreak_godlikeandon_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        ////Player Health

        private void health_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).health_enabled = (this.health_enabled.IsChecked.HasValue) ? this.health_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void health_healthy_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_healthy_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).healthy_color = ColorUtils.MediaColorToDrawingColor(this.health_healthy_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void health_hurt_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_hurt_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).hurt_color = ColorUtils.MediaColorToDrawingColor(this.health_hurt_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void hp_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).health_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        private void health_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).health_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.health_effect_type.SelectedIndex.ToString());
                profile_manager.SaveProfiles();
            }
        }

        ////Player Mana

        private void mana_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).mana_enabled = (this.mana_enabled.IsChecked.HasValue) ? this.mana_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void mana_hasmana_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.mana_hasmana_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).mana_color = ColorUtils.MediaColorToDrawingColor(this.mana_hasmana_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void mana_nomana_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.mana_nomana_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).nomana_color = ColorUtils.MediaColorToDrawingColor(this.mana_nomana_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void mana_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).mana_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.mana_effect_type.SelectedIndex.ToString());
                profile_manager.SaveProfiles();
            }
        }

        private void mana_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).mana_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        ////Color Zones
        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).lighting_areas = (sender as ColorZones).ColorZonesList;
                profile_manager.SaveProfiles();
            }
        }

        private void mimic_respawn_timer_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).mimic_respawn_timer = (this.mimic_respawn_timer_checkbox.IsChecked.HasValue) ? this.mimic_respawn_timer_checkbox.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void mimic_respawn_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.mimic_respawn_color_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).mimic_respawn_timer_color = ColorUtils.MediaColorToDrawingColor(this.mimic_respawn_color_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void mimic_respawn_respawning_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.mimic_respawn_respawning_color_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).mimic_respawn_timer_respawning_color = ColorUtils.MediaColorToDrawingColor(this.mimic_respawn_respawning_color_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void background_enable_respawn_glow_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).bg_respawn_glow = (this.background_enable_respawn_glow.IsChecked.HasValue) ? this.background_enable_respawn_glow.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void respawn_glow_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.respawn_glow_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).bg_respawn_glow_color = ColorUtils.MediaColorToDrawingColor(this.respawn_glow_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void abilities_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).abilitykeys_enabled = (this.abilities_enabled.IsChecked.HasValue) ? this.abilities_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void abilities_canuse_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.abilities_canuse_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).ability_can_use_color = ColorUtils.MediaColorToDrawingColor(this.abilities_canuse_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void abilities_cannotuse_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.abilities_cannotuse_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).ability_can_not_use_color = ColorUtils.MediaColorToDrawingColor(this.abilities_cannotuse_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void ability_key1_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 1 Key", sender as TextBlock, ability1_keys_callback);
        }

        private void ability1_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability1_keys_callback;
            ability_key1_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).ability_keys[0] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.ability_key1_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 0);
            }
            Global.key_recorder.Reset();
        }

        private void ability_key2_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 2 Key", sender as TextBlock, ability2_keys_callback);
        }

        private void ability2_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability2_keys_callback;
            ability_key2_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).ability_keys[1] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.ability_key1_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 1);
            }
            Global.key_recorder.Reset();
        }

        private void ability_key3_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 3 Key", sender as TextBlock, ability3_keys_callback);
        }

        private void ability3_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability3_keys_callback;
            ability_key3_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).ability_keys[2] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.ability_key3_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 2);
            }
            Global.key_recorder.Reset();
        }

        private void ability_key4_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 4 Key", sender as TextBlock, ability4_keys_callback);
        }

        private void ability4_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability4_keys_callback;
            ability_key4_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).ability_keys[3] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.ability_key4_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 3);
            }
            Global.key_recorder.Reset();
        }

        private void ability_key5_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 5 Key", sender as TextBlock, ability5_keys_callback);
        }

        private void ability5_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability5_keys_callback;
            ability_key5_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).ability_keys[4] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.ability_key5_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 4);
            }
            Global.key_recorder.Reset();
        }

        private void ability_key6_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ultimate Ability Key", sender as TextBlock, ability6_keys_callback);
        }

        private void ability6_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability6_keys_callback;
            ability_key6_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).ability_keys[5] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.ability_key6_textblock, (profile_manager.Settings as Dota2Settings).ability_keys, 5);
            }
            Global.key_recorder.Reset();
        }

        private void item_slot1_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 1 Key", sender as TextBlock, item1_keys_callback);
        }

        private void item1_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item1_keys_callback;
            item_slot1_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[0] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.item_slot1_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 0);
            }
            Global.key_recorder.Reset();
        }

        private void item_slot2_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 2 Key", sender as TextBlock, item2_keys_callback);
        }

        private void item2_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item2_keys_callback;
            item_slot2_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[1] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.item_slot2_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 1);
            }
            Global.key_recorder.Reset();
        }

        private void item_slot3_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 3 Key", sender as TextBlock, item3_keys_callback);
        }

        private void item3_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item3_keys_callback;
            item_slot3_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[2] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.item_slot3_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 2);
            }
            Global.key_recorder.Reset();
        }

        private void item_slot4_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 4 Key", sender as TextBlock, item4_keys_callback);
        }

        private void item4_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item4_keys_callback;
            item_slot4_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[3] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.item_slot4_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 3);
            }
            Global.key_recorder.Reset();
        }

        private void item_slot5_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 5 Key", sender as TextBlock, item5_keys_callback);
        }

        private void item5_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item5_keys_callback;
            item_slot5_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[4] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.item_slot5_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 4);
            }
            Global.key_recorder.Reset();
        }

        private void item_slot6_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 6 Key", sender as TextBlock, item6_keys_callback);
        }

        private void item6_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item6_keys_callback;
            item_slot6_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[5] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.item_slot6_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 5);
            }
            Global.key_recorder.Reset();
        }

        private void stash_slot1_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 1 Key", sender as TextBlock, stash1_keys_callback);
        }

        private void stash1_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash1_keys_callback;
            stash_slot1_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[6] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.stash_slot1_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 6);
            }
            Global.key_recorder.Reset();
        }

        private void stash_slot2_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 2 Key", sender as TextBlock, stash2_keys_callback);
        }

        private void stash2_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash2_keys_callback;
            stash_slot2_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[7] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.stash_slot2_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 7);
            }
            Global.key_recorder.Reset();
        }

        private void stash_slot3_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 3 Key", sender as TextBlock, stash3_keys_callback);
        }

        private void stash3_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash3_keys_callback;
            stash_slot3_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[8] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.stash_slot3_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 8);
            }
            Global.key_recorder.Reset();
        }

        private void stash_slot4_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 4 Key", sender as TextBlock, stash4_keys_callback);
        }

        private void stash4_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash4_keys_callback;
            stash_slot4_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[9] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.stash_slot4_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 9);
            }
            Global.key_recorder.Reset();
        }

        private void stash_slot5_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 5 Key", sender as TextBlock, stash5_keys_callback);
        }

        private void stash5_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash5_keys_callback;
            stash_slot5_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[10] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.stash_slot5_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 10);
            }
            Global.key_recorder.Reset();
        }

        private void stash_slot6_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 6 Key", sender as TextBlock, stash6_keys_callback);
        }

        private void stash6_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash6_keys_callback;
            stash_slot6_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resulting_keys.Length > 0)
            {
                if (IsLoaded)
                {
                    (profile_manager.Settings as Dota2Settings).items_keys[11] = resulting_keys[0];
                    profile_manager.SaveProfiles();
                }

                UIUtils.SetSingleKey(this.stash_slot6_textblock, (profile_manager.Settings as Dota2Settings).items_keys, 11);
            }
            Global.key_recorder.Reset();
        }

        private void item_empty_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.item_empty_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).items_empty_color = ColorUtils.MediaColorToDrawingColor(this.item_empty_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void items_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).items_enabled = (this.items_enabled.IsChecked.HasValue) ? this.items_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void item_on_cooldown_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.item_on_cooldown_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).items_on_cooldown_color = ColorUtils.MediaColorToDrawingColor(this.item_on_cooldown_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void item_no_charges_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.item_no_charges_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).items_no_charges_color = ColorUtils.MediaColorToDrawingColor(this.item_no_charges_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void item_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.item_color_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as Dota2Settings).items_color = ColorUtils.MediaColorToDrawingColor(this.item_color_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void items_use_item_color_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).items_use_item_color = (this.items_use_item_color.IsChecked.HasValue) ? this.items_use_item_color.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void background_killstreaks_lines_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as Dota2Settings).bg_killstreaks_lines = (this.background_killstreaks_lines_enabled.IsChecked.HasValue) ? this.background_killstreaks_lines_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void RecordSingleKey(string whoisrecording, TextBlock textblock, KeyRecorder.RecordingFinishedHandler callback)
        {
            if (Global.key_recorder.IsRecording())
            {

                if (Global.key_recorder.GetRecordingType().Equals(whoisrecording))
                {
                    Global.key_recorder.StopRecording();

                    Global.key_recorder.Reset();
                }
                else
                {
                    System.Windows.MessageBox.Show("You are already recording a key sequence for " + Global.key_recorder.GetRecordingType());
                }
            }
            else
            {
                Global.key_recorder.FinishedRecording += callback;
                Global.key_recorder.StartRecording(whoisrecording, true);
                textblock.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Predefined, "dota2.exe");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
        }

        private bool InstallGSI()
        {
            String installpath = Utils.SteamUtils.GetGamePath(570);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "game", "dota", "cfg", "gamestate_integration", "gamestate_integration_aurora.cfg");

                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }

                using (FileStream cfg_stream = File.Create(path))
                {
                    cfg_stream.Write(Properties.Resources.gamestate_integration_aurora_dota2, 0, Properties.Resources.gamestate_integration_aurora_dota2.Length);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UninstallGSI()
        {
            String installpath = Utils.SteamUtils.GetGamePath(570);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "game", "dota", "cfg", "gamestate_integration", "gamestate_integration_aurora.cfg");

                if (File.Exists(path))
                    File.Delete(path);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
