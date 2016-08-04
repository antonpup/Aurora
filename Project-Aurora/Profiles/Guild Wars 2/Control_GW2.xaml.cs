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
        private ProfileManager profile_manager;

        public Control_GW2()
        {
            InitializeComponent();

            profile_manager = Global.Configuration.ApplicationProfiles["GW2"];

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

            this.game_enabled.IsChecked = (profile_manager.Settings as GW2Settings).isEnabled;
            this.cz.ColorZonesList = (profile_manager.Settings as GW2Settings).lighting_areas;
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
                (profile_manager.Settings as GW2Settings).isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GW2Settings).lighting_areas = (sender as ColorZones).ColorZonesList;
                profile_manager.SaveProfiles();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Predefined, profile_manager.ProcessNames[0]);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
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
