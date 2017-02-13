using Aurora.Controls;
using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Profiles.Generic_Application
{
    /// <summary>
    /// Interaction logic for Control_GenericApplication.xaml
    /// </summary>
    public partial class Control_GenericApplication : UserControl
    {
        private ProfileManager profile_manager;

        public Control_GenericApplication(ProfileManager profile)
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

            this.profile_enabled.IsChecked = (profile_manager.Settings as GenericApplicationSettings).IsEnabled;
            this.app_name_textbox.Text = (profile_manager.Settings as GenericApplicationSettings).ApplicationName;
        }
        
        private bool HasProfile()
        {
            return Global.ProfilesManager.Events.ContainsKey(profile_manager.Config.ProcessNames[0]);
        }
               
        private void app_name_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                if (HasProfile())
                    (profile_manager.Settings as GenericApplicationSettings).ApplicationName = app_name_textbox.Text;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void profile_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).IsEnabled = (this.profile_enabled.IsChecked.HasValue) ? this.profile_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void profile_nighttime_check_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings)._simulateNighttime = (this.profile_nighttime_check.IsChecked.HasValue) ? this.profile_nighttime_check.IsChecked.Value : false;
            }
        }
    }
}
