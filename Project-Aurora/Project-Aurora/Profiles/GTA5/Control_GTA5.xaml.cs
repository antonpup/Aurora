using Aurora.Controls;
using Aurora.Profiles.GTA5.GSI;
using Aurora.Settings;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.GTA5
{
    /// <summary>
    /// Interaction logic for Control_GTA5.xaml
    /// </summary>
    public partial class Control_GTA5 : UserControl
    {
        private ProfileManager profile_manager;

        private Timer preview_wantedlevel_timer;
        private int frame = 0;

        public Control_GTA5(ProfileManager profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            preview_wantedlevel_timer = new Timer(1000);
            preview_wantedlevel_timer.Elapsed += Preview_wantedlevel_timer_Elapsed;
        }

        private void SetSettings()
        {
            this.profilemanager.ProfileManager = profile_manager;
            this.scriptmanager.ProfileManager = profile_manager;

            this.game_enabled.IsChecked = (profile_manager.Settings as GTA5Settings).IsEnabled;
            this.cz.ColorZonesList = (profile_manager.Settings as GTA5Settings).lighting_areas;
        }

        private void preview_state_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_GTA5).CurrentState = (GTA5.GSI.PlayerState)Enum.Parse(typeof(GTA5.GSI.PlayerState), this.preview_team.SelectedIndex.ToString());
            }
        }

        private void Preview_wantedlevel_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(frame % 2 == 0)
            {
                (profile_manager.Config.Event._game_state as GameState_GTA5).LeftSirenColor = System.Drawing.Color.Red;
                (profile_manager.Config.Event._game_state as GameState_GTA5).RightSirenColor = System.Drawing.Color.Blue;
            }
            else
            {
                (profile_manager.Config.Event._game_state as GameState_GTA5).LeftSirenColor = System.Drawing.Color.Blue;
                (profile_manager.Config.Event._game_state as GameState_GTA5).RightSirenColor = System.Drawing.Color.Red;
            }

            frame++;
        }

        private void preview_wantedlevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            {
                int value = (sender as IntegerUpDown).Value.Value;
                if (value == 0)
                {
                    preview_wantedlevel_timer.Stop();
                    (profile_manager.Config.Event._game_state as GameState_GTA5).HasCops = false;
                }
                else
                {
                    preview_wantedlevel_timer.Start();
                    preview_wantedlevel_timer.Interval = 600D - 50D * value;
                    (profile_manager.Config.Event._game_state as GameState_GTA5).HasCops = true;
                }
            }
        }

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).lighting_areas = (sender as ColorZones).ColorZonesList;
                profile_manager.SaveProfiles();
            }
        }

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"Aurora.exe";
                startInfo.Arguments = @"-install_logitech";
                startInfo.Verb = "runas";
                Process.Start(startInfo);
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Could not start Aurora Logitech Patcher. Error: " + exc, Logging_Level.Error);
            }
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }
    }
}
