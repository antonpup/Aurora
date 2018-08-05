using Aurora.Profiles.Minecraft.GSI;
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

        #region Overview handlers
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
        #endregion

        #region Preview Handlers
        private GameState_Minecraft State => profile.Config.Event._game_state as GameState_Minecraft;

        private void InGameCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.InGame = (sender as CheckBox).IsChecked ?? false;
        }

        private void HealthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            State.Player.Health = (float)e.NewValue;
            State.Player.HealthMax = 20f;
        }

        private void HungerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            State.Player.FoodLevel = (int)e.NewValue;
        }

        private void ArmorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            State.Player.Armor = (int)e.NewValue;
        }

        private void ExperienceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            State.Player.Experience = (float)e.NewValue;
        }

        private void IsBurningCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.IsBurning = (sender as CheckBox).IsChecked ?? false;
        }

        private void IsInWaterCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.IsInWater = (sender as CheckBox).IsChecked ?? false;
        }

        private void IsSneakingCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.IsSneaking = (sender as CheckBox).IsChecked ?? false;
        }

        private void IsRidingCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.IsRidingHorse = (sender as CheckBox).IsChecked ?? false;
        }

        private void RainStrengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            State.World.RainStrength = (float)e.NewValue;
            State.World.IsRaining = e.NewValue > 0;
        }

        private void WorldTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            State.World.WorldTime = (long)(e.NewValue * 24000); // 24000 is max time in Minecraft before next day
            State.World.IsDayTime = e.NewValue <= 0.5; // At half point, it becomes night time?
        }
        #endregion
    }
}