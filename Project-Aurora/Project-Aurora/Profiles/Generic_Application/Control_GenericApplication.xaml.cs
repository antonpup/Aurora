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
        private Application profile_manager;

        public Control_GenericApplication(Application profile)
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
            this.profile_enabled.IsChecked = profile_manager.Settings.IsEnabled;
            this.app_name_textbox.Text = (profile_manager.Profile as GenericApplicationProfile).ApplicationName;
        }
        
        private bool HasProfile()
        {
            return Global.LightingStateManager.Events.ContainsKey(profile_manager.Config.ProcessNames[0]);
        }
               
        private void app_name_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                if (HasProfile())
                    (profile_manager.Profile as GenericApplicationProfile).ApplicationName = app_name_textbox.Text;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void profile_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                profile_manager.Settings.IsEnabled = (this.profile_enabled.IsChecked.HasValue) ? this.profile_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void profile_nighttime_check_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                (profile_manager.Profile as GenericApplicationProfile)._simulateNighttime = (this.profile_nighttime_check.IsChecked.HasValue) ? this.profile_nighttime_check.IsChecked.Value : false;
            }
        }
    }
}
