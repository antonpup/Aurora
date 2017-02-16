using Aurora.Controls;
using Aurora.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.Evolve
{
    /// <summary>
    /// Interaction logic for Control_Evolve.xaml
    /// </summary>
    public partial class Control_Evolve : UserControl
    {
        private ProfileManager profile_manager;

        public Control_Evolve(ProfileManager profile)
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

            this.game_enabled.IsChecked = (profile_manager.Settings as EvolveSettings).IsEnabled;
            this.cz.ColorZonesList = (profile_manager.Settings as EvolveSettings).lighting_areas;
        }

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"Aurora.exe";
            startInfo.Arguments = @"-install_logitech";
            startInfo.Verb = "runas";
            Process.Start(startInfo);
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as EvolveSettings).IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as EvolveSettings).lighting_areas = (sender as ColorZones).ColorZonesList;
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
