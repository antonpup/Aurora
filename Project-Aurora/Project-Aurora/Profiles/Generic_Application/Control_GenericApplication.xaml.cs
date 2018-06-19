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
        private Application app;

        public Control_GenericApplication(Application profile)
        {
            InitializeComponent();

            app = profile;

            SetSettings();

            app.ProfileChanged += Profile_manager_ProfileChanged;
        }

        private void Profile_manager_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.DataContext = app;
        }

        private void profile_nighttime_check_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !app.Disposed)
            {
                (app.Profile as GenericApplicationProfile)._simulateNighttime = (this.profile_nighttime_check.IsChecked.HasValue) ? this.profile_nighttime_check.IsChecked.Value : false;
            }
        }
    }
}
