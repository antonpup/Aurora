using Aurora.Settings;
using Aurora.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Aurora.Profiles.Desktop
{
    /// <summary>
    /// Interaction logic for Control_Desktop.xaml
    /// </summary>
    public partial class Control_Desktop : UserControl
    {
        private ProfileManager profile_manager;

        public Control_Desktop(ProfileManager profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            profile_manager.ProfileChanged += Desktop_profile_ProfileChanged;
        }

        private void Desktop_profile_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.profilemanager.ProfileManager = profile_manager;
            this.scriptmanager.ProfileManager = profile_manager;

            this.profile_enabled.IsChecked = (profile_manager.Settings as DesktopSettings).IsEnabled;

            this.desktop_cz.ColorZonesList = (profile_manager.Settings as DesktopSettings).lighting_areas;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as DesktopSettings).IsEnabled = (this.profile_enabled.IsChecked.HasValue) ? this.profile_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        //// Misc

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void desktop_cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as DesktopSettings).lighting_areas = (sender as ColorZones).ColorZonesList;
                profile_manager.SaveProfiles();
            }
        }
    }
}
