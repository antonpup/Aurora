using Aurora.Controls;
using Aurora.Profiles.Borderlands2.GSI;
using Aurora.Profiles.Borderlands2.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.Borderlands2
{
    /// <summary>
    /// Interaction logic for Control_Borderlands2.xaml
    /// </summary>
    public partial class Control_Borderlands2 : UserControl
    {
        private Application profile_manager;

        public Control_Borderlands2(Application profile)
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
                    (profile_manager.Config.Event._game_state as GameState_Borderlands2).Player.CurrentHealth = (float)((sender as Slider).Value);
                    (profile_manager.Config.Event._game_state as GameState_Borderlands2).Player.MaximumHealth = 100.0f;
                }
            }
        }

        private void preview_shield_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                this.preview_shield_amount_label.Text = (int)((sender as Slider).Value) + "%";

                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_Borderlands2).Player.CurrentShield = (float)((sender as Slider).Value);
                    (profile_manager.Config.Event._game_state as GameState_Borderlands2).Player.MaximumShield = 100.0f;
                }     
            }
        }

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.SaveProfiles();
            }
        }
    }
}
