using Aurora.Controls;
using Aurora.Profiles.Borderlands2.GSI;
using Aurora.Profiles.Borderlands2.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.Borderlands2
{
    /// <summary>
    /// Interaction logic for Control_Borderlands2.xaml
    /// </summary>
    public partial class Control_Borderlands2 : UserControl
    {
        private Application profile_manager;

        public Control_Borderlands2(Application profile)
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

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.SaveProfiles();
            }
        }
    }
}
