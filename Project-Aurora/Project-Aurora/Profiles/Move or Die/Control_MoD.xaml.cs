using Aurora.Settings;
using Aurora.Utils;
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

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.InstallWrapper(dialog.SelectedPath);

                MessageBox.Show("Aurora Wrapper Patch for Razer applied to\r\n" + dialog.SelectedPath);
            }
        }

        private const int GameID = 323850;

        private bool InstallWrapper(string installpath = "")
        {
            if (string.IsNullOrWhiteSpace(installpath))
                installpath = Path.Combine(SteamUtils.GetGamePath(GameID), "Love", "win");

            return FileUtils.TryWrite(Path.Combine(installpath, "RzChromaSDK.dll"), Properties.Resources.Aurora_RazerLEDWrapper86) &&
                FileUtils.TryWrite(Path.Combine(installpath, "RzChromaSDK64.dll"), Properties.Resources.Aurora_RazerLEDWrapper64);
        }

        private bool UninstallWrapper()
        {
            var installpath = SteamUtils.GetGamePath(GameID);

            return FileUtils.TryDelete(Path.Combine(installpath, "Love", "win", "RzChromaSDK.dll")) &&
                FileUtils.TryDelete(Path.Combine(installpath, "Love", "win", "RzChromaSDK64.dll"));
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
            string gamepath = Utils.SteamUtils.GetGamePath(GameID);

            bool sucess = FileUtils.TryDisable(Path.Combine(gamepath, "Love", "win", "CUESDK.dll"));
            bool sucess64 = FileUtils.TryDisable(Path.Combine(gamepath, "Love", "win", "CUESDK.x64.dll"));

            if (sucess && sucess64)
                MessageBox.Show("Sucesfully disabled MoD Corsair support.");
            else if (sucess || sucess64)
                MessageBox.Show("Error: Partly disabled MoD Corsair support. Is it already disabled?");
            else
                MessageBox.Show("Couldn't disabled MoD Corsair support. Is it already disabled?");
        }

        private void unpatch_cue_dll_button_Click(object sender, RoutedEventArgs e)
        {
            string gamepath = SteamUtils.GetGamePath(GameID);
            bool sucess = FileUtils.TryEnable(Path.Combine(gamepath, "Love", "win", "CUESDK.dll"));
            bool sucess64 = FileUtils.TryEnable(Path.Combine(gamepath, "Love", "win", "CUESDK.x64.dll"));

            if (sucess && sucess64)
                MessageBox.Show("Sucesfully re-enabled MoD Corsair support.");
            else if (sucess || sucess64)
                MessageBox.Show("Error: Partly re-enabled MoD Corsair support. Is it already enabled?");
            else
                MessageBox.Show("Couldn't re-enabled MoD Corsair support. Is it already enabled?");
        }
    }
}
