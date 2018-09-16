using Aurora.Profiles.Minecraft.GSI;
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
        private ObservableCollection<ModDetails> ModList = new ObservableCollection<ModDetails>();

        public Control_Minecraft(Application profile) {
            this.profile = profile;

            InitializeComponent();
            SetSettings();
            
            ModListControl.ItemsSource = ModList;
            PopulateModList();

            profile.ProfileChanged += (sender, e) => SetSettings();
        }

        private void SetSettings() {
            GameEnabled.IsChecked = profile.Settings.IsEnabled;
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

        private void GoToReleasesPage_Click(object sender, RoutedEventArgs e) {
            Process.Start(@"https://gitlab.com/aurora-gsi-minecraft");
        }

        private void GoToModDownloadPage_Click(object sender, RoutedEventArgs e) {
            Process.Start(((sender as Button).DataContext as ModDetails).Link);
        }

        private async void PopulateModList() {
            ModList.Clear();
            try {
                HttpClient client = new HttpClient();

                // Fetch all the data for the 'Aurora GSI Minecraft' group
                string groupDataResponse = await client.GetStringAsync(@"https://gitlab.com/api/v4/groups/aurora-gsi-minecraft/");
                JObject groupData = JObject.Parse(groupDataResponse);

                // For each project that is in this group
                foreach (var project in groupData["projects"]) {
                    // Get the project tags (which contains the releases)
                    string projectTagsResponse = await client.GetStringAsync($"{project["_links"]["self"]}/repository/tags?order_by=updated&sort=desc");
                    JArray projectTags = JArray.Parse(projectTagsResponse);

                    string versionStr = projectTags[0]["name"].ToString();

                    // Add an entry to the mod details list
                    ModList.Add(new ModDetails {
                        Name = project["name"].ToString(), // Get the project name (includes MC version)
                        Version = versionStr, // Get the version string (e.g. "v0.1.2")
                        Link = $"{project["web_url"].ToString()}/tags/{versionStr}", // Generate a link to the download page (e.g. "https://gitlab.com/aurora-gsi-minecraft/mc1.7.10/tags/v0.1.2")
                        Date = DateTime.Parse(projectTags[0]["commit"]["created_at"].ToString()).ToShortDateString() // Show the date the latest version of the mod was released.
                    });
                }
                
            } catch { }
        }

        private void RefreshModList_Click(object sender, RoutedEventArgs e) {
            PopulateModList();
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

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            State.Player.Absorption = (float)e.NewValue;
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

        private void HasWitherCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.PlayerEffects.HasWither = (sender as CheckBox).IsChecked ?? false;
        }

        private void HasPoisonCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.PlayerEffects.HasPoison = (sender as CheckBox).IsChecked ?? false;
        }

        private void HasRegenCh_Checked(object sender, RoutedEventArgs e) {
            State.Player.PlayerEffects.HasRegeneration = (sender as CheckBox).IsChecked ?? false;
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

    class ModDetails {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Link { get; set; }
        public string Date { get; set; }
    }
}