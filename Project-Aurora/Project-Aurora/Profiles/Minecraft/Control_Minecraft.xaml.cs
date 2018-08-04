using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Profiles.Minecraft {
    /// <summary>
    /// Interaction logic for Control_Minecraft.xaml
    /// </summary>
    public partial class Control_Minecraft : UserControl {

        private Application profile;
        private (string Name, DateTime Date) latestModVersion;

        public Control_Minecraft(Application profile) {
            this.profile = profile;

            InitializeComponent();
            SetSettings();
            PrintLatestMCModVersion();

            profile.ProfileChanged += (sender, e) => SetSettings();
        }

        private void SetSettings() {
            GameEnabled.IsChecked = profile.Settings.IsEnabled;
        }

        /// <summary>
        /// Sends a request to the GitLab API to fetch the tags (releases) for the Minecraft mod.
        /// </summary>
        private async Task GetLatestMCModVersion() {
            try {
                // URL to send the request to
                string requestUrl = @"https://gitlab.com/api/v4/projects/wibble199%2Faurora-gsi-minecraft/repository/tags?order_by=updated&sort=desc";

                // Send request
                HttpClient client = new HttpClient();
                string response = await client.GetStringAsync(requestUrl);

                // Parse JSON response
                JArray json = JArray.Parse(response);

                // Store the name/date of the latest version
                latestModVersion = (
                    Name: json[0]["name"].ToString(),
                    Date: DateTime.Parse(json[0]["commit"]["created_at"].ToString())
                );

            } catch (Exception ex) {
                // In case of error (e.g. API unavailable) mark as unknown.
                latestModVersion = (Name: "Unknown", Date: DateTime.MinValue);
                Global.logger.Error("Exception occured while attempting to fetch latest Minecraft GSI mod information" + ex);
            }
        }

        /// <summary>
        /// Fetchs latest Minecraft mod version from GitLab and updates the labels on the control.
        /// </summary>
        private async void PrintLatestMCModVersion() {
            DateTime lastChecked = DateTime.Now;
            await GetLatestMCModVersion();
            ModLatestVersion.Content = latestModVersion.Name;
            ModLatestDate.Content = "Released on: " + latestModVersion.Date.ToString();
            ModLatestCheckDate.Content = "Checked at: " + lastChecked.ToString();
        }

        private void GameEnabled_Checked(object sender, RoutedEventArgs e) {
            if (IsLoaded) {
                profile.Settings.IsEnabled = GameEnabled.IsChecked ?? false;
                profile.SaveProfiles();
            }
        }

        private void GoToForgePage_Click(object sender, RoutedEventArgs e) {
            Process.Start(@"https://files.minecraftforge.net/");
        }

        private void CheckForUpdatesNow_Click(object sender, RoutedEventArgs e) {
            PrintLatestMCModVersion();
        }

        private void GoToReleasesPage_Click(object sender, RoutedEventArgs e) {
            Process.Start(@"https://gitlab.com/wibble199/aurora-gsi-minecraft/tags");
        }

        private void GoToLatestDownloadPage_Click(object sender, RoutedEventArgs e) {
            Process.Start($"https://gitlab.com/wibble199/aurora-gsi-minecraft/tags/{latestModVersion.Name}");
        }
    }
}