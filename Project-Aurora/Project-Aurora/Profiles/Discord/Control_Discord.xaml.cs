using Aurora.Profiles.Discord.GSI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.IO;

namespace Aurora.Profiles.Discord
{
    /// <summary>
    /// Interaction logic for Control_Minecraft.xaml
    /// </summary>
    public partial class Control_Discord : UserControl {
        private Application profile;

        public Control_Discord(Application profile) {
            this.profile = profile;

            InitializeComponent();
            SetSettings();         

            profile.ProfileChanged += (sender, e) => SetSettings();
        }

        private void SetSettings() {
            GameEnabled.IsChecked = profile.Settings.IsEnabled;
        }

        private void GameEnabled_Checked(object sender, RoutedEventArgs e) {
            if (IsLoaded) {
                profile.Settings.IsEnabled = GameEnabled.IsChecked ?? false;
                profile.SaveProfiles();
            }
        }

        private void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pluginDirectory = Path.Combine(appdata, "BetterDiscord", "plugins");

            if (!Directory.Exists(pluginDirectory))
                Directory.CreateDirectory(pluginDirectory);

            string pluginFile = Path.Combine(pluginDirectory, "AuroraGSI.plugin.js");
            WriteFile(pluginFile);
        }

        private void UnpatchButton_Click(object sender, RoutedEventArgs e)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appdata, "BetterDiscord", "plugins", "AuroraGSI.plugin.js");

            if (File.Exists(path))
            {
                File.Delete(path);
                MessageBox.Show("Plugin uninstalled successfully");
                return;
            }
            else
            {
                MessageBox.Show("Plugin not found.");
                return;
            }
        }

        private void ManualPatchButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                string pluginFile = Path.Combine(dialog.SelectedPath, "AuroraGSI.plugin.js");
                WriteFile(pluginFile);
            }
        }

        private void WriteFile(string pluginFile)
        {
            if (File.Exists(pluginFile))
            {
                MessageBox.Show("Plugin already installed");
                return;
            }

            try
            {
                using (FileStream pluginStream = File.Create(pluginFile))
                {
                    pluginStream.Write(Properties.Resources.DiscordGSIPlugin, 0, Properties.Resources.DiscordGSIPlugin.Length);
                }
                MessageBox.Show("Plugin installed successfully");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error installng plugin: " + e.Message);
            }
        }
    }
}