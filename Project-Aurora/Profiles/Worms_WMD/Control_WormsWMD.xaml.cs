﻿using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.WormsWMD
{
    /// <summary>
    /// Interaction logic for Control_Overwatch.xaml
    /// </summary>
    public partial class Control_WormsWMD : UserControl
    {
        private ProfileManager profile_manager;

        public Control_WormsWMD(ProfileManager profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();
        }

        private void SetSettings()
        {
            this.scriptmanager.ProfileManager = profile_manager;

            this.game_enabled.IsChecked = profile_manager.Settings.isEnabled;
            this.ow_ce_enabled.IsChecked = (profile_manager.Settings as WormsWMDSettings).colorEnhance_Enabled;
            this.ow_ce_color_factor.Value = (profile_manager.Settings as WormsWMDSettings).colorEnhance_color_factor;
            this.ow_ce_color_factor_label.Text = (profile_manager.Settings as WormsWMDSettings).colorEnhance_color_factor.ToString();
        }

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            if (InstallWrapper())
                MessageBox.Show("Aurora Wrapper Patch installed successfully.");
            else
                MessageBox.Show("Aurora Wrapper Patch could not be installed.\r\nGame is not installed.");
        }

        private void unpatch_button_Click(object sender, RoutedEventArgs e)
        {
            if (UninstallWrapper())
                MessageBox.Show("Aurora Wrapper Patch uninstalled successfully.");
            else
                MessageBox.Show("Aurora Wrapper Patch could not be uninstalled.\r\nGame is not installed.");
        }

        private void patch_button_manually_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.InstallWrapper(dialog.SelectedPath);

                MessageBox.Show("Aurora Wrapper Patch for Razer applied to\r\n" + dialog.SelectedPath);
            }
        }

        private int GameID = 327030;

        private bool InstallWrapper(string installpath = "")
        {
            if (String.IsNullOrWhiteSpace(installpath))
                installpath = Utils.SteamUtils.GetGamePath(this.GameID);


            if (!String.IsNullOrWhiteSpace(installpath))
            {
                using (BinaryWriter razer_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(installpath, "RzChromaSDK.dll"), FileMode.Create)))
                {
                    razer_wrapper_86.Write(Properties.Resources.Aurora_RazerLEDWrapper86);
                }

                using (BinaryWriter razer_wrapper_64 = new BinaryWriter(new FileStream(System.IO.Path.Combine(installpath, "RzChromaSDK64.dll"), FileMode.Create)))
                {
                    razer_wrapper_64.Write(Properties.Resources.Aurora_RazerLEDWrapper64);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UninstallWrapper()
        {
            String installpath = Utils.SteamUtils.GetGamePath(this.GameID);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "RzChromaSDK.dll");
                string path64 = System.IO.Path.Combine(installpath, "RzChromaSDK64.dll");

                if (File.Exists(path))
                    File.Delete(path);

                if (File.Exists(path64))
                    File.Delete(path64);

                return true;
            }
            else
            {
                return false;
            }
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
                (profile_manager.Settings as WormsWMDSettings).isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void ow_ce_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as WormsWMDSettings).colorEnhance_Enabled = (this.ow_ce_enabled.IsChecked.HasValue) ? this.ow_ce_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void ow_ce_color_factor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as WormsWMDSettings).colorEnhance_color_factor = (int)this.ow_ce_color_factor.Value;
                this.ow_ce_color_factor_label.Text = ((int)this.ow_ce_color_factor.Value).ToString();
                profile_manager.SaveProfiles();
            }
        }
    }
}
