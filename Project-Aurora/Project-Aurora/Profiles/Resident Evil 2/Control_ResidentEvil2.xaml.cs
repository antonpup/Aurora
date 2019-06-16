using Aurora.Controls;
using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.ResidentEvil2
{
    /// <summary>
    /// Interaction logic for Control_ResidentEvil2.xaml
    /// </summary>
    public partial class Control_ResidentEvil2 : UserControl
    {
        private Application profile_manager;

        public Control_ResidentEvil2(Application profile)
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

            if (!this.preview_status.HasItems)
            {
                this.preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Fine);
                this.preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.LiteFine);
                this.preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Caution);
                this.preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Danger);
                this.preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Dead);
                this.preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.OffGame);
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

        private void preview_status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_ResidentEvil2).Player.Status = (Player_ResidentEvil2.PlayerStatus)this.preview_status.SelectedItem;
            }
        }

        private void preview_poison_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (profile_manager.Config.Event._game_state as GameState_ResidentEvil2).Player.Poison = (sender as CheckBox).IsChecked.Value;
        }

        private void preview_rank_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_ResidentEvil2).Player.Rank = (sender as IntegerUpDown).Value.Value;
        }
    }
}
