using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Aurora.Controls;
using Aurora.Settings;

namespace Aurora.Profiles.BF3
{
    /// <summary>
    /// Interaction logic for Control_BF3.xaml
    /// </summary>
    public partial class Control_BF3 : UserControl
    {
        private Application profile_manager;

        public Control_BF3(Application profile)
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

        private void patch_button_Click(object sender, RoutedEventArgs e)
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

        private void unpatch_button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (UninstallWrapper(dialog.SelectedPath))
                    MessageBox.Show("Aurora LightFX Wrapper uninstalled successfully.");
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

        private bool InstallWrapper(string installpath)
        {
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                //86
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

        private bool UninstallWrapper(string installpath)
        {
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                //86
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
