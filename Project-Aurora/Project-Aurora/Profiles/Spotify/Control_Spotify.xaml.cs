using Aurora.Profiles.Spotify.GSI;
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

namespace Aurora.Profiles.Spotify
{
    /// <summary> 
    /// Interaction logic for Control_Minecraft.xaml 
    /// </summary> 
    public partial class Control_Spotify : UserControl
    {

        private Application profile;

        public Control_Spotify(Application profile)
        {
            this.profile = profile;

            InitializeComponent();
            SetSettings();


            profile.ProfileChanged += (sender, e) => SetSettings();
        }

        private void SetSettings()
        {
            GameEnabled.IsChecked = profile.Settings.IsEnabled;
        }

        private void GameEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile.Settings.IsEnabled = GameEnabled.IsChecked ?? false;
                profile.SaveProfiles();
            }
        }

        private void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            InstallPlugin();
        }

        private void UnpatchButton_Click(object sender, RoutedEventArgs e)
        {
            UninstallPlugin();
        }

        private void InstallPlugin()
        {
            string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pluginDirectory = Path.Combine(userprofile, ".spicetify", "Extensions");

            if (!Directory.Exists(pluginDirectory))
                Directory.CreateDirectory(pluginDirectory);

            string pluginFile = Path.Combine(pluginDirectory, "AuroraSpotify.js");
            if (File.Exists(pluginFile))
            {
                MessageBox.Show("Plugin already installed");
                return;
            }

            try
            {
                using (FileStream pluginStream = File.Create(pluginFile))
                {
                    pluginStream.Write(Properties.Resources.AuroraSpotify, 0, Properties.Resources.AuroraSpotify.Length);
                }
                MessageBox.Show("Plugin installed successfully");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error installng plugin: " + e.Message);
            }
        }

        private void UninstallPlugin()
        {
            string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string path = Path.Combine(userprofile, ".spicetify", "Extensions", "AuroraSpotify.js");

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
    }
}