using Aurora.Settings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.Move_or_Die
{
    /// <summary>
    /// Interaction logic for Control_MoD.xaml
    /// </summary>
    public partial class Control_MoD : UserControl
    {
        private Application profile_manager;

        public Control_MoD(Application profile)
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

        private int GameID = 323850;

        private bool InstallWrapper()
        {
            string installpath = System.IO.Path.Combine(Utils.SteamUtils.GetGamePath(this.GameID), "Love", "win");

            if (Directory.Exists(installpath))
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

        private bool InstallWrapper(string installpath)
        {
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

                string enginepath = System.IO.Path.Combine(installpath, "Love", "win", "RzChromaSDK.dll");
                string enginepath64 = System.IO.Path.Combine(installpath, "Love", "win", "RzChromaSDK64.dll");

                if (File.Exists(path))
                    File.Delete(path);

                if (File.Exists(path64))
                    File.Delete(path64);

                if (File.Exists(enginepath))
                    File.Delete(enginepath);

                if (File.Exists(enginepath64))
                    File.Delete(enginepath64);

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

        private void patch_cue_dll_button_Click(object sender, RoutedEventArgs e)
        {
            string gamepath = Utils.SteamUtils.GetGamePath(this.GameID);
            bool sucess = disable(System.IO.Path.Combine(gamepath, "Love", "win", "CUESDK.dll"));
            bool sucess64 = disable(System.IO.Path.Combine(gamepath, "Love", "win", "CUESDK.x64.dll"));
            if (sucess && sucess64)
                MessageBox.Show("Sucesfully disabled MoD Corsair support.");
            else if (sucess || sucess64)
                MessageBox.Show("Error: Partly disabled MoD Corsair support. Is it already disabled?");
            else
                MessageBox.Show("Couldn't disabled MoD Corsair support. Is it already disabled?");
        }

        private bool disable(string file)
        {
            if (File.Exists(file))
            {
                if (File.Exists(file + ".disabled"))
                    File.Delete(file + ".disabled");

                File.Move(file, file + ".disabled");
                return true;
            }
            else
                return false;
        }

        private void unpatch_cue_dll_button_Click(object sender, RoutedEventArgs e)
        {
            string gamepath = Utils.SteamUtils.GetGamePath(this.GameID);
            bool sucess = enable(System.IO.Path.Combine(gamepath, "Love", "win", "CUESDK.dll"));
            bool sucess64 = enable(System.IO.Path.Combine(gamepath, "Love", "win", "CUESDK.x64.dll"));
            if (sucess && sucess64)
                MessageBox.Show("Sucesfully re-enabled MoD Corsair support.");
            else if (sucess || sucess64)
                MessageBox.Show("Error: Partly re-enabled MoD Corsair support. Is it already enabled?");
            else
                MessageBox.Show("Couldn't re-enabled MoD Corsair support. Is it already enabled?");
        }

        private bool enable(string file)
        {
            if (File.Exists(file))
                return false;

            string file_d = file + ".disabled";
            if (File.Exists(file_d))
            {
                File.Move(file_d, file);
                return true;
            }
            else
                return false;
        }
    }
}
