using Aurora.Controls;
using Aurora.Devices;
using Aurora.Settings;
using System;
using System.Collections.Generic;
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

namespace Aurora.Profiles.CSGO
{
    /// <summary>
    /// Interaction logic for Control_CSGO.xaml
    /// </summary>
    public partial class Control_CSGO : UserControl
    {
        private List<DeviceKeys> recordedKeys = new List<DeviceKeys>();

        private Timer preview_bomb_timer;
        private Timer preview_bomb_remove_effect_timer;

        private int preview_kills = 0;
        private int preview_killshs = 0;

        public Control_CSGO()
        {
            InitializeComponent();

            this.game_enabled.IsChecked = Global.Configuration.csgo_settings.isEnabled;

            this.preview_team.Items.Add(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.Undefined);
            this.preview_team.Items.Add(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.CT);
            this.preview_team.Items.Add(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.T);

            this.background_enabled.IsChecked = Global.Configuration.csgo_settings.bg_team_enabled;
            this.t_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.t_color);
            this.ct_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.ct_color);
            this.ambient_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.ambient_color);
            this.background_peripheral_use.IsChecked = Global.Configuration.csgo_settings.bg_peripheral_use;
            this.background_dim_enabled.IsChecked = Global.Configuration.csgo_settings.bg_enable_dimming;
            this.background_dim_value.Text = Global.Configuration.csgo_settings.bg_dim_after + "s";
            this.background_dim_aftertime.Value = Global.Configuration.csgo_settings.bg_dim_after;

            this.health_enabled.IsChecked = Global.Configuration.csgo_settings.health_enabled;
            this.health_healthy_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.healthy_color);
            this.health_hurt_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.hurt_color);
            this.health_effect_type.SelectedIndex = (int)Global.Configuration.csgo_settings.health_effect_type;
            this.hp_keysequence.Sequence = Global.Configuration.csgo_settings.health_sequence;

            this.ammo_enabled.IsChecked = Global.Configuration.csgo_settings.ammo_enabled;
            this.ammo_hasammo_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.ammo_color);
            this.ammo_noammo_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.noammo_color);
            this.ammo_effect_type.SelectedIndex = (int)Global.Configuration.csgo_settings.ammo_effect_type;
            this.ammo_keysequence.Sequence = Global.Configuration.csgo_settings.ammo_sequence;

            this.bomb_enabled.IsChecked = Global.Configuration.csgo_settings.bomb_enabled;
            this.bomb_flash_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.bomb_flash_color);
            this.bomb_primed_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.bomb_primed_color);
            this.bomb_display_winner.IsChecked = Global.Configuration.csgo_settings.bomb_display_winner_color;
            this.bomb_gradual_effect.IsChecked = Global.Configuration.csgo_settings.bomb_gradual;
            this.bomb_keysequence.Sequence = Global.Configuration.csgo_settings.bomb_sequence;
            this.bomb_peripheral_use.IsChecked = Global.Configuration.csgo_settings.bomb_peripheral_use;

            this.kills_enabled.IsChecked = Global.Configuration.csgo_settings.kills_indicator;
            this.kills_regular_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.kills_regular_color);
            this.kills_headshot_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.kills_headshot_color);
            this.kills_keysequence.Sequence = Global.Configuration.csgo_settings.kills_sequence;

            this.cz.ColorZonesList = Global.Configuration.csgo_settings.lighting_areas;

            this.flashbang_enabled.IsChecked = Global.Configuration.csgo_settings.flashbang_enabled;
            this.flashbang_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.flash_color);
            this.flashbang_peripheral_use.IsChecked = Global.Configuration.csgo_settings.flashbang_peripheral_use;

            this.typing_enabled.IsChecked = Global.Configuration.csgo_settings.typing_enabled;
            this.typing_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.typing_color);
            this.typing_keysequence.Sequence = Global.Configuration.csgo_settings.typing_sequence;

            this.burning_enabled.IsChecked = Global.Configuration.csgo_settings.burning_enabled;
            this.burning_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.csgo_settings.burning_color);
            this.burning_animation.IsChecked = Global.Configuration.csgo_settings.burning_animation;
            this.burning_peripheral_use.IsChecked = Global.Configuration.csgo_settings.burning_peripheral_use;


            preview_bomb_timer = new Timer(45000);
            preview_bomb_timer.Elapsed += new ElapsedEventHandler(preview_bomb_timer_Tick);

            preview_bomb_remove_effect_timer = new Timer(5000);
            preview_bomb_remove_effect_timer.Elapsed += new ElapsedEventHandler(preview_bomb_remove_effect_timer_Tick);


            //Copy cfg file if needed
            if (!Global.Configuration.csgo_settings.first_time_installed)
            {
                InstallGSI();
                Global.Configuration.csgo_settings.first_time_installed = true;
            }
        }

        private void preview_bomb_timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(
                    () =>
                    {
                        this.preview_bomb_defused.IsEnabled = false;
                        this.preview_bomb_start.IsEnabled = true;

                        GameEvent_CSGO.SetBombState(Aurora.Profiles.CSGO.GSI.Nodes.BombState.Exploded);
                        preview_bomb_timer.Stop();

                        preview_bomb_remove_effect_timer.Start();
                    });
        }

        private void preview_bomb_remove_effect_timer_Tick(object sender, EventArgs e)
        {
            GameEvent_CSGO.SetBombState(Aurora.Profiles.CSGO.GSI.Nodes.BombState.Undefined);
            preview_bomb_remove_effect_timer.Stop();
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
                Global.Configuration.csgo_settings.isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        ////Preview

        private void preview_team_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam)this.preview_team.Items[this.preview_team.SelectedIndex])
            {
                case (Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.T):
                    GameEvent_CSGO.SetTeam(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.T);
                    break;
                case (Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.CT):
                    GameEvent_CSGO.SetTeam(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.CT);
                    break;
                default:
                    GameEvent_CSGO.SetTeam(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.Undefined);
                    break;
            }
        }

        private void preview_health_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hp_val = (int)this.preview_health_slider.Value;
            if (this.preview_health_amount is Label)
            {
                this.preview_health_amount.Content = hp_val + "%";
                GameEvent_CSGO.SetHealth(hp_val);
            }
        }

        private void preview_ammo_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int ammo_val = (int)this.preview_ammo_slider.Value;
            if (this.preview_ammo_amount is Label)
            {
                this.preview_ammo_amount.Content = ammo_val + "%";
                GameEvent_CSGO.SetClip(ammo_val);
                GameEvent_CSGO.SetClipMax(100);
            }
        }

        private void preview_flash_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int flash_val = (int)this.preview_flash_slider.Value;
            if (this.preview_flash_amount is Label)
            {
                this.preview_flash_amount.Content = flash_val + "%";
                GameEvent_CSGO.SetFlashAmount((int)(((double)flash_val / 100.0) * 255.0));
            }
        }

        private void preview_burning_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int burning_val = (int)this.preview_burning_slider.Value;
            if (this.preview_burning_amount is Label)
            {
                this.preview_burning_amount.Content = burning_val + "%";
                GameEvent_CSGO.SetBurnAmount((int)(((double)burning_val / 100.0) * 255.0));
            }
        }

        private void preview_bomb_start_Click(object sender, RoutedEventArgs e)
        {
            this.preview_bomb_defused.IsEnabled = true;
            this.preview_bomb_start.IsEnabled = false;

            GameEvent_CSGO.SetBombState(Aurora.Profiles.CSGO.GSI.Nodes.BombState.Planted);
            preview_bomb_timer.Start();
            preview_bomb_remove_effect_timer.Stop();
        }

        private void preview_bomb_defused_Click(object sender, RoutedEventArgs e)
        {
            this.preview_bomb_defused.IsEnabled = false;
            this.preview_bomb_start.IsEnabled = true;

            GameEvent_CSGO.SetBombState(Aurora.Profiles.CSGO.GSI.Nodes.BombState.Defused);
            preview_bomb_timer.Stop();
            preview_bomb_remove_effect_timer.Start();
        }

        private void preview_typing_enabled_Checked(object sender, RoutedEventArgs e)
        {
            GameEvent_CSGO.SetPlayerActivity((((this.preview_typing_enabled.IsChecked.HasValue) ? this.preview_typing_enabled.IsChecked.Value : false) ? Aurora.Profiles.CSGO.GSI.Nodes.PlayerActivity.TextInput : Aurora.Profiles.CSGO.GSI.Nodes.PlayerActivity.Undefined));
        }

        private void preview_respawn_Click(object sender, RoutedEventArgs e)
        {
            GameEvent_CSGO.Respawned();
        }

        private void preview_addkill_hs_Click(object sender, RoutedEventArgs e)
        {
            GameEvent_CSGO.GotAKill(true);
            preview_killshs++;
            preview_kills_label.Text = String.Format("Kills: {0} Headshots: {1}", preview_kills, preview_killshs);
        }

        private void preview_addkill_Click(object sender, RoutedEventArgs e)
        {
            GameEvent_CSGO.GotAKill();
            preview_kills++;
            preview_kills_label.Text = String.Format("Kills: {0} Headshots: {1}", preview_kills, preview_killshs);
        }
        private void preview_kills_reset_Click(object sender, RoutedEventArgs e)
        {
            GameEvent_CSGO.RoundStart();
            preview_kills = 0;
            preview_killshs = 0;

            preview_kills_label.Text = String.Format("Kills: {0} Headshots: {1}", preview_kills, preview_killshs);
        }

        ////Background

        private void background_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bg_team_enabled = (this.background_enabled.IsChecked.HasValue) ? this.background_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void background_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bg_peripheral_use = (this.background_peripheral_use.IsChecked.HasValue) ? this.background_peripheral_use.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void t_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.t_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.t_color = Utils.ColorUtils.MediaColorToDrawingColor(this.t_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ct_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ct_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.ct_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ct_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ambient_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ambient_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.ambient_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ambient_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void background_dim_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bg_enable_dimming = (this.background_dim_enabled.IsChecked.HasValue) ? this.background_dim_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
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
                    Global.Configuration.csgo_settings.bg_dim_after = val;
                    ConfigManager.Save(Global.Configuration);
                }
            }
        }

        ////Player Health

        private void health_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.health_enabled = (this.health_enabled.IsChecked.HasValue) ? this.health_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void health_healthy_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_healthy_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.healthy_color = Utils.ColorUtils.MediaColorToDrawingColor(this.health_healthy_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void health_hurt_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.health_hurt_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.hurt_color = Utils.ColorUtils.MediaColorToDrawingColor(this.health_hurt_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void hp_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.health_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void health_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.health_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.health_effect_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        ////Player Ammo

        private void ammo_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.ammo_enabled = (this.ammo_enabled.IsChecked.HasValue) ? this.ammo_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_hasammo_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ammo_hasammo_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.ammo_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ammo_hasammo_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_noammo_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.ammo_noammo_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.noammo_color = Utils.ColorUtils.MediaColorToDrawingColor(this.ammo_noammo_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.ammo_effect_type = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), this.ammo_effect_type.SelectedIndex.ToString());
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void ammo_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.ammo_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        ////Bomb

        private void bomb_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bomb_enabled = (this.bomb_enabled.IsChecked.HasValue) ? this.bomb_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bomb_flash_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bomb_flash_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.bomb_flash_color = Utils.ColorUtils.MediaColorToDrawingColor(this.bomb_flash_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bomb_primed_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bomb_primed_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.bomb_primed_color = Utils.ColorUtils.MediaColorToDrawingColor(this.bomb_primed_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bomb_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bomb_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bomb_display_winner_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bomb_display_winner_color = (this.bomb_display_winner.IsChecked.HasValue) ? this.bomb_display_winner.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bomb_gradual_effect_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bomb_gradual = (this.bomb_gradual_effect.IsChecked.HasValue) ? this.bomb_gradual_effect.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bomb_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.bomb_peripheral_use = (this.bomb_peripheral_use.IsChecked.HasValue) ? this.bomb_peripheral_use.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        ////Kills Indicator

        private void kills_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.kills_indicator = (this.kills_enabled.IsChecked.HasValue) ? this.kills_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void kills_regular_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.kills_regular_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.kills_regular_color = Utils.ColorUtils.MediaColorToDrawingColor(this.kills_regular_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void kills_headshot_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.kills_headshot_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.kills_headshot_color = Utils.ColorUtils.MediaColorToDrawingColor(this.kills_headshot_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void kills_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.kills_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        ////Color Zones

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.lighting_areas = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        ////Flashbang/Burning

        private void flashbang_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.flashbang_enabled = (this.flashbang_enabled.IsChecked.HasValue) ? this.flashbang_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void flashbang_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.flashbang_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.flash_color = Utils.ColorUtils.MediaColorToDrawingColor(this.flashbang_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void flashbang_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.flashbang_peripheral_use = (this.flashbang_peripheral_use.IsChecked.HasValue) ? this.flashbang_peripheral_use.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void burning_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.burning_enabled = (this.burning_enabled.IsChecked.HasValue) ? this.burning_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void burning_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.burning_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.burning_color = Utils.ColorUtils.MediaColorToDrawingColor(this.burning_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void burning_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.burning_peripheral_use = (this.burning_peripheral_use.IsChecked.HasValue) ? this.burning_peripheral_use.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void burning_animation_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.burning_animation = (this.burning_animation.IsChecked.HasValue) ? this.burning_animation.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        ////Typing Keys

        private void typing_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.typing_enabled = (this.typing_enabled.IsChecked.HasValue) ? this.typing_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void typing_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.typing_color_colorpicker.SelectedColor.HasValue)
            {
                Global.Configuration.csgo_settings.typing_color = Utils.ColorUtils.MediaColorToDrawingColor(this.typing_color_colorpicker.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void typing_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.csgo_settings.typing_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Predefined, "csgo.exe");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
        }

        private bool InstallGSI()
        {
            String installpath = Utils.SteamUtils.GetGamePath(730);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "csgo", "cfg", "gamestate_integration_aurora.cfg");

                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }

                using (FileStream cfg_stream = File.Create(path))
                {
                    cfg_stream.Write(Properties.Resources.gamestate_integration_aurora_csgo, 0, Properties.Resources.gamestate_integration_aurora_csgo.Length);
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
            String installpath = Utils.SteamUtils.GetGamePath(730);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "csgo", "cfg", "gamestate_integration_aurora.cfg");

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
