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
        private Application profile_manager;

        public Control_Desktop(Application profile)
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
            this.profile_enabled.IsChecked = profile_manager.Settings.IsEnabled;
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
                profile_manager.Settings.IsEnabled = (this.profile_enabled.IsChecked.HasValue) ? this.profile_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        //// Misc

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
