using Aurora.Controls;
using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.LeagueOfLegends
{
    /// <summary>
    /// Interaction logic for Control_LoL.xaml
    /// </summary>
    public partial class Control_LoL : UserControl
    {
        private Application profile_manager;

        public Control_LoL(Application profile)
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

        private void patch_button_manual_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (BinaryWriter lightfx_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
                {
                    lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
                }

                MessageBox.Show("Aurora Wrapper Patch for LightFX applied to\r\n" + dialog.SelectedPath);
            }
        }

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            string lolpath;
            try
            {
                lolpath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Riot Games, Inc\League of Legends", "Location", null);
            }
            catch
            {
                lolpath = String.Empty;
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(lolpath))
                {
                    lolpath = Path.Combine(lolpath, "Game");
                    if (Directory.Exists(lolpath))
                    {
                        using (BinaryWriter lightfx_wrapper_86 = new BinaryWriter(new FileStream(Path.Combine(lolpath, "LightFX.dll"), FileMode.Create)))
                        {
                            lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
                        }
                        MessageBox.Show("Aurora Wrapper Patch for LightFX applied to\r\n" + lolpath);
                        return;
                    }
                }
                MessageBox.Show("Couldn't find League of Legends path automatically, please patch manually");
                return;
            }
            catch(Exception exc)
            {
                Global.logger.Error("Error patching League of Legends:" + exc.Message);
                MessageBox.Show("Error patching League of Legends: " + exc.Message);
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
    }
}
