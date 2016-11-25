﻿using Aurora.Settings;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.TheDivision
{
    /// <summary>
    /// Interaction logic for Control_TheDivision.xaml
    /// </summary>
    public partial class Control_TheDivision : UserControl
    {
        private ProfileManager profile_manager;

        public Control_TheDivision()
        {
            InitializeComponent();

            profile_manager = Global.Configuration.ApplicationProfiles["The Division"];

            SetSettings();
        }

        private void SetSettings()
        {
            this.scriptmanager.ProfileManager = profile_manager;

            this.game_enabled.IsChecked = (profile_manager.Settings as TheDivisionSettings).isEnabled;
        }

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"Aurora.exe";
            startInfo.Arguments = @"-install_logitech";
            startInfo.Verb = "runas";
            Process.Start(startInfo);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Predefined, profile_manager.ProcessNames[0]);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as TheDivisionSettings).isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }
    }
}
