using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.DOOM
{
    /// <summary>
    /// Interaction logic for Control_Overwatch.xaml
    /// </summary>
    public partial class Control_DOOM : UserControl
    {
        private Application profile_manager;

        public Control_DOOM(Application profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();
        }

        private void SetSettings()
        {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;
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

        private int GameID = 379720;

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


        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }
    }
}
