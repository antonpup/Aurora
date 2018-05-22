using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.ETS2 {
    /// <summary>
    /// Interaction logic for Control_ETS2.xaml
    /// </summary>
    public partial class Control_ETS2 : UserControl {

        private Application profile_manager;

        public Control_ETS2(Application profile) {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            profile_manager.ProfileChanged += Profile_manager_ProfileChanged;
        }

        private void Profile_manager_ProfileChanged(object sender, EventArgs e) {
            SetSettings();
        }

        private void SetSettings() {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e) {
            if (IsLoaded) {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        /// <summary>
        /// Installs either the 32-bit or 64-bit version of the Telemetry Server DLL.
        /// </summary>
        /// <param name="x64">Install 64-bit (true) or 32-bit (false)?</param>
        private bool InstallDLL(bool x64) {
            string gamePath = Utils.SteamUtils.GetGamePath(227300);
            if (String.IsNullOrWhiteSpace(gamePath))
                return false;
            
            string installPath = System.IO.Path.Combine(gamePath, "bin", x64 ? "win_x64" : "win_x86", "plugins", "ets2-telemetry-server.dll");

            if (!File.Exists(installPath))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(installPath));

            using (FileStream cfg_stream = File.Create(installPath)) {
                var sourceDll = x64 ? Properties.Resources.ets2_telemetry_server_x64 : Properties.Resources.ets2_telemetry_server_x86;
                cfg_stream.Write(sourceDll, 0, sourceDll.Length);
            }

            return true;
        }

        private void install_button_Click(object sender, RoutedEventArgs e) {
            if (!InstallDLL(true)) {
                MessageBox.Show("64-bit ETS2 Telemetry Server DLL installed failed.");
            } else if (!InstallDLL(false)) {
                MessageBox.Show("32-bit ETS2 Telemetry Server DLL installed failed.");
            } else {
                MessageBox.Show("ETS2 Telemetry Server DLLs installed successfully.");
            }
        }

        private void uninstall_button_Click(object sender, RoutedEventArgs e) {
            string gamePath = Utils.SteamUtils.GetGamePath(227300);
            if (String.IsNullOrWhiteSpace(gamePath)) return;
            string x86Path = System.IO.Path.Combine(gamePath, "bin", "win_x86", "plugins", "ets2-telemetry-server.dll");
            string x64Path = System.IO.Path.Combine(gamePath, "bin", "win_x64", "plugins", "ets2-telemetry-server.dll");
            if (File.Exists(x64Path))
                File.Delete(x64Path);
            if (File.Exists(x86Path))
                File.Delete(x86Path);
            MessageBox.Show("ETS2 Telemetry Server DLLs uninstalled successfully.");
        }

        private void visit_ets2ts_button_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/Funbit/ets2-telemetry-server");
        }
    }
}
