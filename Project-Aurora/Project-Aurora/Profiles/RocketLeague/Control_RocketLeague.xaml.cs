using Aurora.Controls;
using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.RocketLeague
{
    /// <summary>
    /// Interaction logic for Control_RocketLeague.xaml
    /// </summary>
    public partial class Control_RocketLeague : UserControl
    {
        private Application profile_manager;

        public Control_RocketLeague(Application profile)
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

        private void Button_DownloadBakkesMod(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://bakkesmod.com/index.php");
        }

        private void Button_GithubLink(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://github.com/diogotr7/AuroraGSI-RocketLeague/releases");
        }

        private void Button_InstallPluginURI(object sender, RoutedEventArgs e)
        {
            Process.Start(@"bakkesmod://install/45");
        }
    }
}
