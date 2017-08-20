using Aurora.Controls;
using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.DiscoDodgeball
{
    /// <summary>
    /// Interaction logic for Control_DiscoDodgeball.xaml
    /// </summary>
    public partial class Control_DiscoDodgeball : UserControl
    {
        private Application profile_manager;

        public Control_DiscoDodgeball(Application profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            //Apply LightFX Wrapper, if needed.
            if (!(profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled)
            {
                InstallWrapper();
                (profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
            }

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

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            if (InstallWrapper())
                MessageBox.Show("Aurora LightFX Wrapper installed successfully.");
            else
                MessageBox.Show("Aurora LightFX Wrapper could not be installed.\r\nGame is not installed.");
        }

        private void unpatch_button_Click(object sender, RoutedEventArgs e)
        {
            if (UninstallWrapper())
                MessageBox.Show("Aurora LightFX Wrapper uninstalled successfully.");
            else
                MessageBox.Show("Aurora LightFX Wrapper could not be uninstalled.\r\nGame is not installed.");
        }

        private void patch_drm_button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (InstallWrapper(dialog.SelectedPath))
                    MessageBox.Show("Aurora Wrapper Patch for LightFX applied to\r\n" + dialog.SelectedPath);
                else
                    MessageBox.Show("Aurora LightFX Wrapper could not be installed.\r\nGame is not installed.");
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private bool InstallWrapper(string installpath = "")
        {
            if (String.IsNullOrWhiteSpace(installpath))
                installpath = Utils.SteamUtils.GetGamePath(270450);


            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "LightFX.dll");

                if (!File.Exists(path))
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

                using (BinaryWriter lightfx_wrapper_86 = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
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
            String installpath = Utils.SteamUtils.GetGamePath(270450);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "LightFX.dll");

                if (File.Exists(path))
                    File.Delete(path);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
