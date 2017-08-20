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
using Xceed.Wpf.Toolkit;
using Aurora.Profiles.Dota_2.GSI;

namespace Aurora.Profiles.Dota_2
{
    /// <summary>
    /// Interaction logic for Control_Dota2.xaml
    /// </summary>
    public partial class Control_Dota2 : UserControl
    {
        private Application profile_manager;

        private int respawn_time = 15;
        private Timer preview_respawn_timer;
        private int killstreak = 0;

        public Control_Dota2(Application profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            preview_respawn_timer = new Timer(1000);
            preview_respawn_timer.Elapsed += new ElapsedEventHandler(preview_respawn_timer_Tick);

            //Copy cfg file if needed
            if (!(profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled)
            {
                InstallGSI();
                (profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
            }

            profile_manager.ProfileChanged += Control_Dota2_ProfileChanged;

        }

        private void Control_Dota2_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;

            if (!this.preview_team.HasItems)
            {
                this.preview_team.Items.Add(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.None);
                this.preview_team.Items.Add(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.Dire);
                this.preview_team.Items.Add(Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam.Radiant);
            }
        }

        private void preview_respawn_timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(
                        () =>
                        {
                            if (this.respawn_time < 0)
                            {
                                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.IsAlive = true;

                                this.preview_killplayer.IsEnabled = true;

                                preview_respawn_timer.Stop();
                            }
                            else
                            {
                                this.preview_respawn_time.Content = "Seconds to respawn: " + this.respawn_time;
                                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.SecondsToRespawn = this.respawn_time;

                                this.respawn_time--;
                            }
                        });
        }

        //Overview

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            if (InstallGSI())
                System.Windows.MessageBox.Show("Aurora GSI Config file installed successfully.");
            else
                System.Windows.MessageBox.Show("Aurora GSI Config file could not be installed.\r\nGame is not installed.");
        }

        private void unpatch_button_Click(object sender, RoutedEventArgs e)
        {
            if (UninstallGSI())
                System.Windows.MessageBox.Show("Aurora GSI Config file uninstalled successfully.");
            else
                System.Windows.MessageBox.Show("Aurora GSI Config file could not be uninstalled.\r\nGame is not installed.");
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
            (profile_manager.Config.Event._game_state as GameState_Dota2).Player.Team = (Aurora.Profiles.Dota_2.GSI.Nodes.PlayerTeam)this.preview_team.SelectedItem;
        }

        private void preview_health_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hp_val = (int)this.preview_health_slider.Value;
            if (this.preview_health_amount is Label)
            {
                this.preview_health_amount.Content = hp_val + "%";
                
                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.Health = hp_val;
                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.MaxHealth = 100;
                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.HealthPercent = hp_val;
                
            }
        }

        private void preview_mana_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int mana_val = (int)this.preview_mana_slider.Value;
            if (this.preview_mana_amount is Label)
            {
                this.preview_mana_amount.Content = mana_val + "%";

                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.Mana = mana_val;
                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.MaxMana = 100;
                (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.ManaPercent = mana_val;
            }
        }

        private void preview_killplayer_Click(object sender, RoutedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.IsAlive = false;

            respawn_time = 15;
            (profile_manager.Config.Event._game_state as GameState_Dota2).Hero.SecondsToRespawn = this.respawn_time;
            this.preview_killplayer.IsEnabled = false;
            (profile_manager.Config.Event._game_state as GameState_Dota2).Player.KillStreak = this.killstreak = 0;
            this.preview_killstreak_label.Content = "Killstreak: " + this.killstreak;

            preview_respawn_timer.Start();
        }

        private void preview_addkill_Click(object sender, RoutedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_Dota2).Player.KillStreak = killstreak++;
            (profile_manager.Config.Event._game_state as GameState_Dota2).Player.Kills++;
            this.preview_killstreak_label.Content = "Killstreak: " + this.killstreak;
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
