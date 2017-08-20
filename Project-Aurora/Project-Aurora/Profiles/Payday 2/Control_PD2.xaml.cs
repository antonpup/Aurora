using Aurora.Controls;
using Aurora.Profiles.Payday_2.GSI;
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
        private Application profile_manager;

        public Control_PD2(Application profile)
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
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
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

                        MessageBox.Show("GSI for Payday 2 installed.");
                    }
                    else
                    {
                        MessageBox.Show("BLT Hook was not found.\r\nCould not install the GSI mod.");
                    }
                }
                else
                {
                    MessageBox.Show("Payday 2 directory is not found.\r\nCould not install the GSI mod.");
                }
            }
            else
            {
                MessageBox.Show("Payday 2 is not installed through Steam.\r\nCould not install the GSI mod.");
            }
        }

        private void preview_gamestate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_PD2).Game.State = (GSI.Nodes.GameStates)Enum.Parse(typeof(GSI.Nodes.GameStates), this.preview_gamestate.SelectedIndex.ToString());
            }
        }

        private void preview_levelphase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_PD2).Level.Phase = (GSI.Nodes.LevelPhase)Enum.Parse(typeof(GSI.Nodes.LevelPhase), this.preview_levelphase.SelectedIndex.ToString());
            }
        }

        private void preview_playerstate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.State = (GSI.Nodes.PlayerState)Enum.Parse(typeof(GSI.Nodes.PlayerState), this.preview_playerstate.SelectedIndex.ToString());
            }
        }

        private void preview_health_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hp_val = (int)this.preview_health_slider.Value;
            if (this.preview_health_amount is Label)
            {
                this.preview_health_amount.Content = hp_val + "%";
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.Health.Current = hp_val;
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.Health.Max = 100;
            }
        }

        private void preview_ammo_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int ammo_val = (int)this.preview_ammo_slider.Value;
            if (this.preview_ammo_amount is Label)
            {
                this.preview_ammo_amount.Content = ammo_val + "%";
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.Weapons.SelectedWeapon.Current_Clip = ammo_val;
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.Weapons.SelectedWeapon.Max_Clip = 100;
            }
        }

        private void preview_suspicion_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float susp_val = (float)this.preview_suspicion_slider.Value;
            if (this.preview_suspicion_amount is Label)
            {
                this.preview_suspicion_amount.Content = (int)susp_val + "%";
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.SuspicionAmount = susp_val / 100.0f;
            }
        }

        private void preview_flashbang_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float flash_val = (float)this.preview_flashbang_slider.Value;
            if (this.preview_flashbang_amount is Label)
            {
                this.preview_flashbang_amount.Content = (int)flash_val + "%";
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.FlashAmount = flash_val / 100.0f;
            }
        }

        private void preview_swansong_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (profile_manager.Config.Event._game_state as GameState_PD2).Players.LocalPlayer.IsSwanSong = (sender as CheckBox).IsChecked.Value;
            }
        }

        private void get_lib_button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(@"https://github.com/simon-wh/PAYDAY-2-BeardLib"));
        }
    }
}
