using Aurora.Controls;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Settings;
using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Profiles.CSGO
{
    /// <summary>
    /// Interaction logic for Control_CSGO.xaml
    /// </summary>
    public partial class Control_CSGO : UserControl
    {
        private Application profile_manager;

        private Timer preview_bomb_timer;
        private Timer preview_bomb_remove_effect_timer;

        private int preview_kills = 0;
        private int preview_killshs = 0;

        public Control_CSGO(Application profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            preview_bomb_timer = new Timer(45000);
            preview_bomb_timer.Elapsed += new ElapsedEventHandler(preview_bomb_timer_Tick);

            preview_bomb_remove_effect_timer = new Timer(5000);
            preview_bomb_remove_effect_timer.Elapsed += new ElapsedEventHandler(preview_bomb_remove_effect_timer_Tick);

            profile_manager.ProfileChanged += Profile_manager_ProfileChanged;

            //Copy cfg file if needed
            if (!(profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled)
            {
                InstallGSI();
                (profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
            }
        }

        private void Profile_manager_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;
            this.preview_team.Items.Clear();
            this.preview_team.Items.Add(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.Undefined);
            this.preview_team.Items.Add(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.CT);
            this.preview_team.Items.Add(Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.T);
            this.preview_team.SelectedItem = Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam.Undefined;
        }

        private void preview_bomb_timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(
                    () =>
                    {
                        this.preview_bomb_defused.IsEnabled = false;
                        this.preview_bomb_start.IsEnabled = true;

                        (profile_manager.Config.Event._game_state as GameState_CSGO).Round.Bomb = GSI.Nodes.BombState.Exploded;
                        preview_bomb_timer.Stop();

                        preview_bomb_remove_effect_timer.Start();
                    });
        }

        private void preview_bomb_remove_effect_timer_Tick(object sender, EventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_CSGO).Round.Bomb = GSI.Nodes.BombState.Undefined;
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
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        ////Preview

        private void preview_team_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.preview_team.Items.Count == 0)
                return;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.Team = (Aurora.Profiles.CSGO.GSI.Nodes.PlayerTeam)this.preview_team.SelectedItem;
        }

        private void preview_health_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hp_val = (int)this.preview_health_slider.Value;
            if (this.preview_health_amount is Label)
            {
                this.preview_health_amount.Content = hp_val + "%";
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.Health = hp_val;
            }
        }

        private void preview_ammo_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int ammo_val = (int)this.preview_ammo_slider.Value;
            if (this.preview_ammo_amount is Label)
            {
                this.preview_ammo_amount.Content = ammo_val + "%";
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.Weapons.ActiveWeapon.AmmoClip = ammo_val;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.Weapons.ActiveWeapon.AmmoClipMax = 100;
            }
        }

        private void preview_flash_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int flash_val = (int)this.preview_flash_slider.Value;
            if (this.preview_flash_amount is Label)
            {
                this.preview_flash_amount.Content = flash_val + "%";
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.Flashed = (int)(((double)flash_val / 100.0) * 255.0);
            }
        }

        private void preview_burning_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int burning_val = (int)this.preview_burning_slider.Value;
            if (this.preview_burning_amount is Label)
            {
                this.preview_burning_amount.Content = burning_val + "%";
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.Burning = (int)(((double)burning_val / 100.0) * 255.0);
            }
        }

        private void preview_bomb_start_Click(object sender, RoutedEventArgs e)
        {
            this.preview_bomb_defused.IsEnabled = true;
            this.preview_bomb_start.IsEnabled = false;

            (profile_manager.Config.Event._game_state as GameState_CSGO).Round.Bomb = GSI.Nodes.BombState.Planted;
            preview_bomb_timer.Start();
            preview_bomb_remove_effect_timer.Stop();
        }

        private void preview_bomb_defused_Click(object sender, RoutedEventArgs e)
        {
            this.preview_bomb_defused.IsEnabled = false;
            this.preview_bomb_start.IsEnabled = true;

            (profile_manager.Config.Event._game_state as GameState_CSGO).Round.Bomb = GSI.Nodes.BombState.Defused;
            preview_bomb_timer.Stop();
            preview_bomb_remove_effect_timer.Start();
        }

        private void preview_typing_enabled_Checked(object sender, RoutedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.Activity = (((this.preview_typing_enabled.IsChecked.HasValue) ? this.preview_typing_enabled.IsChecked.Value : false) ? Aurora.Profiles.CSGO.GSI.Nodes.PlayerActivity.TextInput : Aurora.Profiles.CSGO.GSI.Nodes.PlayerActivity.Undefined);
        }

        private void preview_respawn_Click(object sender, RoutedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (profile_manager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.Health = 100;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.Health = 99;

            int curr_hp_val = (int)this.preview_health_slider.Value;

            System.Threading.Timer reset_conditions_timer = null;
            reset_conditions_timer = new System.Threading.Timer((obj) =>
                {
                    (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.Health = curr_hp_val;
                    (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.Health = 100;

                    reset_conditions_timer.Dispose();
                },
                null, 500, System.Threading.Timeout.Infinite);
        }

        private void preview_addkill_hs_Click(object sender, RoutedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (profile_manager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

            (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = preview_kills;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = ++preview_kills;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = preview_killshs;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = ++preview_killshs;

            System.Threading.Timer reset_conditions_timer = null;
            reset_conditions_timer = new System.Threading.Timer((obj) =>
            {
                (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = preview_kills;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = preview_kills;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = preview_killshs;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = preview_killshs;

                reset_conditions_timer.Dispose();
            },
                null, 500, System.Threading.Timeout.Infinite);

            preview_kills_label.Text = String.Format("Kills: {0} Headshots: {1}", preview_kills, preview_killshs);
        }

        private void preview_addkill_Click(object sender, RoutedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (profile_manager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

            (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = preview_kills;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = ++preview_kills;

            System.Threading.Timer reset_conditions_timer = null;
            reset_conditions_timer = new System.Threading.Timer((obj) =>
            {
                (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = preview_kills;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = preview_kills;

                reset_conditions_timer.Dispose();
            },
                null, 500, System.Threading.Timeout.Infinite);

            preview_kills_label.Text = String.Format("Kills: {0} Headshots: {1}", preview_kills, preview_killshs);
        }
        private void preview_kills_reset_Click(object sender, RoutedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (profile_manager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

            (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = preview_kills;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = 0;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = preview_killshs;
            (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = 0;

            System.Threading.Timer reset_conditions_timer = null;
            reset_conditions_timer = new System.Threading.Timer((obj) =>
            {
                (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = 0;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = 0;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = 0;
                (profile_manager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = 0;

                reset_conditions_timer.Dispose();
            },
                null, 500, System.Threading.Timeout.Infinite);

            preview_kills = 0;
            preview_killshs = 0;

            preview_kills_label.Text = String.Format("Kills: {0} Headshots: {1}", preview_kills, preview_killshs);
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
