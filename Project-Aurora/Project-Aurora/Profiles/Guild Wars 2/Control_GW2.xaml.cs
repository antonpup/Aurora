using Aurora.Controls;
using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.Guild_Wars_2
{
    /// <summary>
    /// Interaction logic for Control_GW2.xaml
    /// </summary>
    public partial class Control_GW2 : UserControl
    {
        private Application profile_manager;

        public Control_GW2(Application profile)
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

        private void patch_32bit_button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (InstallWrapper(dialog.SelectedPath, false))
                    MessageBox.Show("Aurora Wrapper Patch for LightFX applied to\r\n" + dialog.SelectedPath);
                else
                    MessageBox.Show("Aurora LightFX Wrapper could not be installed.");
            }
        }

        private void patch_64bit_button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (InstallWrapper(dialog.SelectedPath, true))
                    MessageBox.Show("Aurora Wrapper Patch for LightFX applied to\r\n" + dialog.SelectedPath);
                else
                    MessageBox.Show("Aurora LightFX Wrapper could not be installed.");
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

        private bool InstallWrapper(string installpath = "", bool is64bit = false)
        {
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string path = System.IO.Path.Combine(installpath, "LightFX.dll");

                if (!File.Exists(path))
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

                using (BinaryWriter lightfx_wrapper = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    lightfx_wrapper.Write( (is64bit ? Properties.Resources.Aurora_LightFXWrapper64 : Properties.Resources.Aurora_LightFXWrapper86) );
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
