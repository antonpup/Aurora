using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.Divinity2
{
    /// <summary>
    /// Interaction logic for Control_Divinity2.xaml
    /// </summary>
    public partial class Control_Divinity2 : UserControl
    {
        private Application profile_manager;

        public Control_Divinity2(Application profile)
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

        private int GameID = 435150;

        private bool InstallWrapper(string installpath = "")
        {
            if (String.IsNullOrWhiteSpace(installpath))
                installpath = Utils.SteamUtils.GetGamePath(this.GameID);


            if (!String.IsNullOrWhiteSpace(installpath))
            {
                string classic = System.IO.Path.Combine(installpath, "Classic");
                if (Directory.Exists(classic))
                {
                    using (BinaryWriter razer_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(classic, "RzChromaSDK.dll"), FileMode.Create)))
                    {
                        razer_wrapper_86.Write(Properties.Resources.Aurora_RazerLEDWrapper86);
                    }
                    using (BinaryWriter razer_wrapper_64 = new BinaryWriter(new FileStream(System.IO.Path.Combine(classic, "RzChromaSDK64.dll"), FileMode.Create)))
                    {
                        razer_wrapper_64.Write(Properties.Resources.Aurora_RazerLEDWrapper64);
                    }
                }

                string defEd = System.IO.Path.Combine(installpath, "DefEd\\bin");
                if (Directory.Exists(defEd))
                {
                    using (BinaryWriter razer_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(defEd, "RzChromaSDK.dll"), FileMode.Create)))
                    {
                        razer_wrapper_86.Write(Properties.Resources.Aurora_RazerLEDWrapper86);
                    }
                    using (BinaryWriter razer_wrapper_64 = new BinaryWriter(new FileStream(System.IO.Path.Combine(defEd, "RzChromaSDK64.dll"), FileMode.Create)))
                    {
                        razer_wrapper_64.Write(Properties.Resources.Aurora_RazerLEDWrapper64);
                    }
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
                string classic = System.IO.Path.Combine(installpath, "Classic");
                string defEd = System.IO.Path.Combine(installpath, "DefEd\\bin");
                string path_classic = System.IO.Path.Combine(classic, "RzChromaSDK.dll");
                string path64_classic = System.IO.Path.Combine(classic, "RzChromaSDK64.dll");
                string path_defEd = System.IO.Path.Combine(defEd, "RzChromaSDK.dll");
                string path64_defEd = System.IO.Path.Combine(defEd, "RzChromaSDK64.dll");

                if (File.Exists(path_classic))
                    File.Delete(path_classic);

                if (File.Exists(path64_classic))
                    File.Delete(path64_classic);

                if (File.Exists(path_defEd))
                    File.Delete(path_defEd);

                if (File.Exists(path64_defEd))
                    File.Delete(path64_defEd);

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
