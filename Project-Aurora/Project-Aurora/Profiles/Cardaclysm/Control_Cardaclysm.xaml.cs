using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.Cardaclysm
{
    /// <summary>
    /// Interaction logic for Control_Cardaclysm.xaml
    /// </summary>
    public partial class Control_Cardaclysm : UserControl
    {
        private Application profile_manager;

        public Control_Cardaclysm(Application profile)
        {
            InitializeComponent();

            profile_manager = profile;

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

        private void go_to_steelseries_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (ConfigUI)System.Windows.Application.Current.MainWindow;
            mainWindow.GoToSteelSeriesPage();
        }
    }
}
