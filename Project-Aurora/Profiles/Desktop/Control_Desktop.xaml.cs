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
        public Control_Desktop()
        {
            InitializeComponent();

            SetSettings();

            Global.Configuration.desktop_profile.ProfileChanged += Desktop_profile_ProfileChanged;
        }

        private void Desktop_profile_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.profilemanager.ProfileManager = Global.Configuration.desktop_profile;
            this.scriptmanager.ProfileManager = Global.Configuration.desktop_profile;

            this.profile_enabled.IsChecked = (Global.Configuration.desktop_profile.Settings as DesktopSettings).isEnabled;

            this.desktop_cz.ColorZonesList = (Global.Configuration.desktop_profile.Settings as DesktopSettings).lighting_areas;

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
                (Global.Configuration.desktop_profile.Settings as DesktopSettings).isEnabled = (this.profile_enabled.IsChecked.HasValue) ? this.profile_enabled.IsChecked.Value : false;
                Global.Configuration.desktop_profile.SaveProfiles();
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
                (Global.Configuration.desktop_profile.Settings as DesktopSettings).lighting_areas = (sender as ColorZones).ColorZonesList;
                Global.Configuration.desktop_profile.SaveProfiles();
            }
        }
    }
}
