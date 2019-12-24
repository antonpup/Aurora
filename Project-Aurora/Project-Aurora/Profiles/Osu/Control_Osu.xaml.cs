using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace Aurora.Profiles.Osu {

    public partial class Control_Osu : System.Windows.Controls.UserControl {
        public Control_Osu(Application profile) {
            InitializeComponent();
            enableOsu.DataContext = profile.Settings;
        }

        /// <summary>
        /// Listener for a HyperLink to open the Uri it has specified.
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        /// <summary>
        /// Opens the OsuSync release page.
        /// </summary>
        private void OpenOsuSyncRepo_Click(object sender, System.Windows.RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(@"https://github.com/OsuSync/Sync/releases"));
        }

        /// <summary>
        /// Installs the Aurora OsuSync Plugin.
        /// </summary>
        private void InstallPlugin_Click(object sender, System.Windows.RoutedEventArgs e) {
            SelectOsuSync(dir => {
                try {
                    using (var stream = new MemoryStream(Properties.Resources.OsuSyncAuroraPlugin))
                        stream.CopyTo(new FileStream(GetPluginPath(dir), FileMode.Create));
                    MessageBox.Show("Aurora OsuSync Plugin successfully installed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } catch (Exception ex) {
                    MessageBox.Show("Aurora OsuSync Plugin was not removed.\n\n" + ex.Message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        /// <summary>
        /// Uninstalls the Aurora OsuSync Plugin.
        /// </summary>
        private void UninstallPlugin_Click(object sender, System.Windows.RoutedEventArgs e) {
            SelectOsuSync(dir => {
                try {
                    var f = GetPluginPath(dir);
                    if (File.Exists(f))
                        File.Delete(f);
                    MessageBox.Show("Aurora OsuSync Plugin successfully removed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } catch (Exception ex) {
                    MessageBox.Show("Aurora OsuSync Plugin was not removed.\n\n" + ex.Message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        /// <summary>
        /// Pops up a dialog box asking the user to select the OsuSync program.
        /// When the user does so, it calls "then" with the OsuSync directory.
        /// </summary>
        private void SelectOsuSync(System.Action<string> then) {
            var dialog = new OpenFileDialog {
                Title = "Select OsuSync program",
                Filter = "OsuSync|sync.exe",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                then.Invoke(Path.GetDirectoryName(dialog.FileName));
        }

        /// <summary>
        /// Gets the full path of the Aurora OsuSync Plugin for the given OsuSync directory.
        /// </summary>
        private string GetPluginPath(string osuSyncDir) => Path.Combine(osuSyncDir, "Plugins", "OsuSyncAuroraPlugin.dll");
    }
}
