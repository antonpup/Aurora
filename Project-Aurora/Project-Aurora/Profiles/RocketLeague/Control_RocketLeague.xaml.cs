using Aurora.Controls;
using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.RocketLeague
{
    /// <summary>
    /// Interaction logic for Control_RocketLeague.xaml
    /// </summary>
    public partial class Control_RocketLeague : UserControl
    {
        private Application profile_manager;

        public Control_RocketLeague(Application profile)
        {
            profile_manager = profile;

            InitializeComponent();

            SetSettings();

            profile_manager.ProfileChanged += Profile_manager_ProfileChanged;
        }

        private void Profile_manager_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;
            if (!this.preview_team.HasItems)
            {
                this.preview_team.DisplayMemberPath = "Text";
                this.preview_team.SelectedValuePath = "Value";
                this.preview_team.Items.Add(new { Text = "Spectator", Value = -1});
                this.preview_team.Items.Add(new { Text = "Blue", Value = 0 });
                this.preview_team.Items.Add(new { Text = "Orange", Value = 1 });
                this.preview_team.SelectedIndex = 1;
            }

            if (!this.preview_status.HasItems)
            {
                this.preview_status.ItemsSource = Enum.GetValues(typeof(RLStatus)).Cast<RLStatus>();
                this.preview_status.SelectedIndex = (int)RLStatus.InGame;
            }

            this.ColorPicker_team1.SelectedColor = Colors.Blue;
            this.ColorPicker_team2.SelectedColor = Colors.Orange;
            this.preview_team1_score.Value = 0;
            this.preview_team2_score.Value = 0;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void Button_DownloadBakkesMod(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://bakkesmod.com/index.php");
        }

        private void Button_BakkesPluginsLink(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://bakkesplugins.com/plugins/view/53");
        }

        private void Button_InstallPluginURI(object sender, RoutedEventArgs e)
        {
            Process.Start(@"bakkesmod://install/53");
        }

        private void preview_team_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_RocketLeague).Player.Team = (int)((this.preview_team.SelectedItem as dynamic).Value);
        }

        private void preview_status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_RocketLeague).Game.Status = (RLStatus)(this.preview_status.SelectedItem);
        }

        private void preview_boost_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                this.preview_boost_amount_label.Text = (int)((sender as Slider).Value * 100) + "%";

                if (IsLoaded)
                    (profile_manager.Config.Event._game_state as GameState_RocketLeague).Player.Boost = (float)((sender as Slider).Value);
            }
        }

        private void preview_team1_score_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_RocketLeague).Match.Blue.Goals = (sender as IntegerUpDown).Value ?? 0;
        }

        private void preview_team2_score_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_RocketLeague).Match.Orange.Goals = (sender as IntegerUpDown).Value ?? 0;
        }

        private void ColorPicker_Team1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if(sender is ColorPicker)
            {
                var clr = this.ColorPicker_team1.SelectedColor ?? new Color();
                (profile_manager.Config.Event._game_state as GameState_RocketLeague).Match.Blue.TeamColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);          
            }
        }

        private void ColorPicker_Team2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (sender is ColorPicker)
            {
                var clr = this.ColorPicker_team2.SelectedColor ?? new Color();
                (profile_manager.Config.Event._game_state as GameState_RocketLeague).Match.Orange.TeamColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
            }
        }
    }
}
