using Aurora.Controls;
using Aurora.Profiles.Dishonored.GSI;
using Aurora.Profiles.Dishonored.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.Dishonored
{
    /// <summary>
    /// Interaction logic for Control_Dishonored.xaml
    /// </summary>
    public partial class Control_Dishonored : UserControl
    {
        private Application profile_manager;

        public Control_Dishonored(Application profile)
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
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }
        private void preview_health_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                this.preview_health_amount_label.Text = (int)((sender as Slider).Value) + "%";

                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_Dishonored).Player.CurrentHealth = (int)((sender as Slider).Value);
                    (profile_manager.Config.Event._game_state as GameState_Dishonored).Player.MaximumHealth = 100;
                }
            }
        }

        private void preview_mana_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                this.preview_mana_amount_label.Text = (int)((sender as Slider).Value) + "%";

                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_Dishonored).Player.CurrentMana = (int)((sender as Slider).Value);
                    (profile_manager.Config.Event._game_state as GameState_Dishonored).Player.MaximumMana = 100;
                }
            }
        }

        private void preview_manapots_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_Dishonored).Player.ManaPots = (sender as IntegerUpDown).Value.Value;
        }

        private void preview_healthpots_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_Dishonored).Player.HealthPots = (sender as IntegerUpDown).Value.Value;
        }
    }
}
