using Aurora.Controls;
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
        private ProfileManager profile_manager;

        public Control_RocketLeague(ProfileManager profile)
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
            this.profilemanager.ProfileManager = profile_manager;
            this.scriptmanager.ProfileManager = profile_manager;

            this.game_enabled.IsChecked = (profile_manager.Settings as RocketLeagueSettings).isEnabled;

            this.bg_enabled.IsChecked = (profile_manager.Settings as RocketLeagueSettings).bg_enabled;
            this.bg_ambient_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).bg_ambient_color);
            this.bg_use_team_colors.IsChecked = (profile_manager.Settings as RocketLeagueSettings).bg_use_team_color;
            this.bg_team1_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).bg_team_1);
            this.bg_team2_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).bg_team_2);
            this.bg_show_team_score_split.IsChecked = (profile_manager.Settings as RocketLeagueSettings).bg_show_team_score_split;

            this.boost_enabled.IsChecked = (profile_manager.Settings as RocketLeagueSettings).boost_enabled;
            this.boost_low_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).boost_low);
            this.boost_med_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).boost_mid);
            this.boost_high_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).boost_high);
            this.boost_ks.Sequence = (profile_manager.Settings as RocketLeagueSettings).boost_sequence;
            this.boost_peripheral_use_enabled.IsChecked = (profile_manager.Settings as RocketLeagueSettings).boost_peripheral_use;

            this.speed_enabled.IsChecked = (profile_manager.Settings as RocketLeagueSettings).speed_enabled;
            this.speed_low_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).speed_low);
            this.speed_med_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).speed_mid);
            this.speed_high_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as RocketLeagueSettings).speed_high);
            this.speed_ks.Sequence = (profile_manager.Settings as RocketLeagueSettings).speed_sequence;

            this.colorzones.ColorZonesList = (profile_manager.Settings as RocketLeagueSettings).lighting_areas;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as RocketLeagueSettings).isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void bg_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if(IsLoaded && sender is CheckBox)
            {
                (profile_manager.Settings as RocketLeagueSettings).bg_enabled = (sender as CheckBox).IsChecked.Value;
                profile_manager.SaveProfiles();
            }
        }

        private void bg_ambient_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if(IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).bg_ambient_color = Utils.ColorUtils.MediaColorToDrawingColor( (sender as ColorPicker).SelectedColor.Value );
                profile_manager.SaveProfiles();
            }
        }

        private void bg_use_team_colors_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                (profile_manager.Settings as RocketLeagueSettings).bg_use_team_color = (sender as CheckBox).IsChecked.Value;
                profile_manager.SaveProfiles();
            }
        }

        private void bg_team1_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).bg_team_1 = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_team2_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).bg_team_2 = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void boost_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                (profile_manager.Settings as RocketLeagueSettings).boost_enabled = (sender as CheckBox).IsChecked.Value;
                profile_manager.SaveProfiles();
            }
        }

        private void boost_low_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).boost_low = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void boost_med_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).boost_mid = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void boost_high_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).boost_mid = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void boost_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && sender is Controls.KeySequence)
            {
                (profile_manager.Settings as RocketLeagueSettings).boost_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        private void boost_peripheral_use_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                (profile_manager.Settings as RocketLeagueSettings).boost_peripheral_use = (sender as CheckBox).IsChecked.Value;
                profile_manager.SaveProfiles();
            }
        }

        private void speed_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                (profile_manager.Settings as RocketLeagueSettings).speed_enabled = (sender as CheckBox).IsChecked.Value;
                profile_manager.SaveProfiles();
            }
        }

        private void speed_low_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).speed_low = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void speed_med_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).speed_mid = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void speed_high_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                (profile_manager.Settings as RocketLeagueSettings).speed_high = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void speed_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && sender is Controls.KeySequence)
            {
                (profile_manager.Settings as RocketLeagueSettings).speed_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        private void colorzones_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && sender is ColorZones)
            {
                (profile_manager.Settings as RocketLeagueSettings).lighting_areas = (sender as ColorZones).ColorZonesList;
                profile_manager.SaveProfiles();
            }
        }

        private void bg_show_team_score_split_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                (profile_manager.Settings as RocketLeagueSettings).bg_show_team_score_split = (sender as CheckBox).IsChecked.Value;
                profile_manager.SaveProfiles();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void preview_boost_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(sender is Slider)
            {
                this.preview_boost_amount_label.Text = (int)((sender as Slider).Value)+"%";

                if(IsLoaded)
                    GameEvent_RocketLeague.SetBoost((float)((sender as Slider).Value) / 100.0f);
            }
        }

        private void preview_team1_score_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            {
                GameEvent_RocketLeague.SetTeam1Score((sender as IntegerUpDown).Value.Value);
            }
        }

        private void preview_team2_score_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            {
                GameEvent_RocketLeague.SetTeam2Score((sender as IntegerUpDown).Value.Value);
            }
        }
    }
}
