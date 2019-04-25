using Aurora.Controls;
using Aurora.Profiles.CloneHero.GSI;
using Aurora.Profiles.CloneHero.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.CloneHero
{
    /// <summary>
    /// Interaction logic for Control_CloneHero.xaml
    /// </summary>
    public partial class Control_CloneHero : UserControl
    {
        private Application profile_manager;

        public Control_CloneHero(Application profile)
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
        private void preview_streak_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            {
                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak = (sender as IntegerUpDown).Value.Value;

                    #region NoteStreak Extras

                    // Breaks up the note streak into the 1x, 2x, 3x, 4x zones for easy lighting options
                    int streak = (sender as IntegerUpDown).Value.Value;
                    if (streak >= 0 && streak <= 10)
                    {
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak1x = streak;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak2x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak3x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak4x = 0;

                        // This accounts for how CH changes the color once the bar fills up
                        if (streak == 10)
                        {
                            (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak2x = 10;
                        }
                    }
                    else if (streak > 10 && streak <= 20)
                    {
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak1x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak2x = streak - 10;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak3x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak4x = 0;

                        // This accounts for how CH changes the color once the bar fills up
                        if (streak == 20)
                        {
                            (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak3x = 10;
                        }
                    }
                    else if (streak > 20 && streak <= 30)
                    {
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak1x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak2x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak3x = streak - 20;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak4x = 0;

                        // This accounts for how CH changes the color once the bar fills up
                        if (streak == 30)
                        {
                            (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak4x = 10;
                        }
                    }
                    else if (streak > 30 && streak <= 40)
                    {
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak1x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak2x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak3x = 0;
                        (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NoteStreak4x = streak - 30;
                    }
                    #endregion
                }
            }
        }
        private void preview_sp_percent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                this.preview_sp_percent_label.Text = (int)((sender as Slider).Value) + "%";

                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.StarPowerPercent = (int)((sender as Slider).Value);
                }
            }
        }
        private void preview_sp_enabled_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                //this.preview_sp_enabled_label.Text = (int)((sender as Slider).Value) + "%";
                if ((int)((sender as Slider).Value) == 0)
                {
                    this.preview_sp_enabled_label.Text = "false";
                }
                else
                {
                    this.preview_sp_enabled_label.Text = "true";
                }

                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsStarPowerActive = ((sender as Slider).Value) == 1 ? true : false;
                }
            }
        }
        private void preview_sp_active(object sender, RoutedEventArgs e)
        {
            this.preview_sp_enabled_label.Text = "true";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsStarPowerActive = true;
            }
        }
        private void preview_sp_deactive(object sender, RoutedEventArgs e)
        {
            this.preview_sp_enabled_label.Text = "false";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsStarPowerActive = false;
            }
        }
        private void preview_menu_active(object sender, RoutedEventArgs e)
        {
            this.preview_menu_enabled_label.Text = "true";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsAtMenu = true;
            }
        }
        private void preview_menu_deactive(object sender, RoutedEventArgs e)
        {
            this.preview_menu_enabled_label.Text = "false";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsAtMenu = false;
            }
        }
        private void preview_fc_active(object sender, RoutedEventArgs e)
        {
            this.preview_fc_enabled_label.Text = "true";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsFC = true;
            }
        }
        private void preview_fc_deactive(object sender, RoutedEventArgs e)
        {
            this.preview_fc_enabled_label.Text = "false";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsFC = false;
            }
        }
        /*private void preview_solo_active(object sender, RoutedEventArgs e)
        {
            this.preview_solo_enabled_label.Text = "true";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsSoloActive = true;
            }
        }
        private void preview_solo_deactive(object sender, RoutedEventArgs e)
        {
            this.preview_solo_enabled_label.Text = "false";
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsSoloActive = false;
            }
        }*/

        private void preview_notes_total_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            {
                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.NotesTotal = (sender as IntegerUpDown).Value.Value;
                }
            }
        }

        private void preview_score_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            {
                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.Score = (sender as IntegerUpDown).Value.Value;
                }
            }
        }

        #region frets

        // Green
        private void preview_green_active(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsGreenPressed = true;
            }
        }
        private void preview_green_deactive(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsGreenPressed = false;
            }
        }

        // Red
        private void preview_red_active(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsRedPressed = true;
            }
        }
        private void preview_red_deactive(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsRedPressed = false;
            }
        }

        // Yellow
        private void preview_yellow_active(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsYellowPressed = true;
            }
        }
        private void preview_yellow_deactive(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsYellowPressed = false;
            }
        }

        // Blue
        private void preview_blue_active(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsBluePressed = true;
            }
        }
        private void preview_blue_deactive(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsBluePressed = false;
            }
        }

        // Orange
        private void preview_orange_active(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsOrangePressed = true;
            }
        }
        private void preview_orange_deactive(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.IsOrangePressed = false;
            }
        }

        #endregion

        /*
        private void preview_mana_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                this.preview_mana_amount_label.Text = (int)((sender as Slider).Value) + "%";

                if (IsLoaded)
                {
                    (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.CurrentMana = (int)((sender as Slider).Value);
                    (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.MaximumMana = 100;
                }
            }
        }

        private void preview_manapots_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.ManaPots = (sender as IntegerUpDown).Value.Value;
        }

        private void preview_healthpots_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                (profile_manager.Config.Event._game_state as GameState_CloneHero).Player.HealthPots = (sender as IntegerUpDown).Value.Value;
        }*/
    }
}
