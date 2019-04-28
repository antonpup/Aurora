using Aurora.Controls;
using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.GSI.Nodes;
using Aurora.Settings;
using System;
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
            InitializeComponent();

            profile_manager = profile;

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
                this.preview_team.Items.Add(PlayerTeam.Spectator);
                this.preview_team.Items.Add(PlayerTeam.Blue);
                this.preview_team.Items.Add(PlayerTeam.Orange);
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

        private void preview_team_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (profile_manager.Config.Event._game_state as GameState_RocketLeague).Player.Team = (PlayerTeam)this.preview_team.SelectedItem;
        }

        private void preview_boost_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(sender is Slider)
            {
                this.preview_boost_amount_label.Text = (int)((sender as Slider).Value)+"%";

                if(IsLoaded)
                    (profile_manager.Config.Event._game_state as GameState_RocketLeague).Player.BoostAmount = (int)((sender as Slider).Value);
            }
        }

        private void preview_team1_score_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_RocketLeague).Match.OrangeTeam_Score = (sender as IntegerUpDown).Value.Value;
        }

        private void preview_team2_score_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_RocketLeague).Match.BlueTeam_Score = (sender as IntegerUpDown).Value.Value;
        }
    }
}
