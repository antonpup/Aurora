using System;
using System.Windows;

namespace Aurora.Profiles.Desktop
{
    /// <summary>
    /// Interaction logic for Control_Desktop.xaml
    /// </summary>
    public partial class Control_Desktop
    {
        private readonly Application _profileManager;

        public Control_Desktop(Application profile)
        {
            InitializeComponent();

            _profileManager = profile;

            SetSettings();

            _profileManager.ProfileChanged += Desktop_profile_ProfileChanged;
        }

        private void Desktop_profile_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            profile_enabled.IsChecked = _profileManager.Settings.IsEnabled;
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
                _profileManager.Settings.IsEnabled = profile_enabled.IsChecked ?? false;
                _profileManager.SaveProfiles();
            }
        }
    }
}
