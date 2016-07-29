using Aurora.Controls;
using Aurora.Settings;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.GTA5
{
    /// <summary>
    /// Interaction logic for Control_GTA5.xaml
    /// </summary>
    public partial class Control_GTA5 : UserControl
    {
        private ProfileManager profile_manager;

        private Timer preview_wantedlevel_timer;

        public Control_GTA5()
        {
            InitializeComponent();

            profile_manager = Global.Configuration.ApplicationProfiles["GTA5"];

            SetSettings();

            preview_wantedlevel_timer = new Timer(1000);
            preview_wantedlevel_timer.Elapsed += Preview_wantedlevel_timer_Elapsed;
        }

        private void SetSettings()
        {
            this.profilemanager.ProfileManager = profile_manager;
            this.scriptmanager.ProfileManager = profile_manager;

            this.game_enabled.IsChecked = (profile_manager.Settings as GTA5Settings).isEnabled;

            this.background_enabled.IsChecked = (profile_manager.Settings as GTA5Settings).bg_color_enabled;
            this.background_peripheral_use.IsChecked = (profile_manager.Settings as GTA5Settings).bg_peripheral_use;
            this.bg_ambient_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_ambient);
            this.bg_franklin_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_franklin);
            this.bg_michael_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_michael);
            this.bg_trevor_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_trevor);
            this.bg_chop_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_chop);
            this.bg_online_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_online);
            this.bg_online_mission_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_online_mission);
            this.bg_online_heistfinale_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_online_heistfinale);
            this.bg_online_spectator_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_online_spectator);
            this.bg_race_gold_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_race_gold);
            this.bg_race_silver_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_race_silver);
            this.bg_race_bronze_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).bg_race_bronze);

            this.siren_enabled.IsChecked = (profile_manager.Settings as GTA5Settings).siren_enabled;
            this.left_siren_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).left_siren_color);
            this.right_siren_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GTA5Settings).right_siren_color);
            this.siren_effect_type.SelectedIndex = (int)(profile_manager.Settings as GTA5Settings).siren_type;

            this.siren_left_keysequence.Sequence = (profile_manager.Settings as GTA5Settings).left_siren_sequence;
            this.siren_right_keysequence.Sequence = (profile_manager.Settings as GTA5Settings).right_siren_sequence;

            this.cz.ColorZonesList = (profile_manager.Settings as GTA5Settings).lighting_areas;
        }


        private void Preview_wantedlevel_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GameEvent_GTA5.IncrementSirenKeyframe();
        }

        private void preview_wantedlevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            {
                int value = (sender as IntegerUpDown).Value.Value;
                if (value == 0)
                {
                    preview_wantedlevel_timer.Stop();
                    GameEvent_GTA5.SetCopStatus(false);
                }
                else
                {
                    preview_wantedlevel_timer.Start();
                    preview_wantedlevel_timer.Interval = 600D - 50D * value;
                    GameEvent_GTA5.SetCopStatus(true);
                }
            }
        }

        private void background_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).bg_color_enabled = (this.background_enabled.IsChecked.HasValue) ? this.background_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void background_peripheral_use_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).bg_peripheral_use = (this.background_peripheral_use.IsChecked.HasValue) ? this.background_peripheral_use.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void bg_ambient_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_ambient_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_ambient = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_ambient_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_franklin_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_franklin_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_franklin = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_franklin_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_michael_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_michael_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_michael = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_michael_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_trevor_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_trevor_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_trevor = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_trevor_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_chop_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_chop_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_chop = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_chop_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_online_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_online_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_online = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_online_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_online_mission_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_online_mission_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_online_mission = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_online_mission_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_online_heistfinale_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_online_heistfinale_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_online_heistfinale = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_online_heistfinale_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_online_spectator_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_online_spectator_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_online_spectator = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_online_spectator_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_race_gold_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_race_gold_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_race_gold = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_race_gold_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_race_silver_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_race_silver_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_race_silver = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_race_silver_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void bg_race_bronze_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.bg_race_bronze_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).bg_race_bronze = Utils.ColorUtils.MediaColorToDrawingColor(this.bg_race_bronze_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void siren_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).siren_enabled = (this.siren_enabled.IsChecked.HasValue) ? this.siren_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void left_siren_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.left_siren_color_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).left_siren_color = Utils.ColorUtils.MediaColorToDrawingColor(this.left_siren_color_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void right_siren_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.right_siren_color_colorpicker.SelectedColor.HasValue)
            {
                (profile_manager.Settings as GTA5Settings).right_siren_color = Utils.ColorUtils.MediaColorToDrawingColor(this.right_siren_color_colorpicker.SelectedColor.Value);
                profile_manager.SaveProfiles();
            }
        }

        private void siren_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).siren_type = (GTA5_PoliceEffects)Enum.Parse(typeof(GTA5_PoliceEffects), this.siren_effect_type.SelectedIndex.ToString());
                profile_manager.SaveProfiles();
            }
        }

        private void siren_left_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).left_siren_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        private void siren_right_keysequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).right_siren_sequence = (sender as Controls.KeySequence).Sequence;
                profile_manager.SaveProfiles();
            }
        }

        private void cz_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).lighting_areas = (sender as ColorZones).ColorZonesList;
                profile_manager.SaveProfiles();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Predefined, "gta5.exe");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
        }

        private void preview_team_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                GameEvent_GTA5.SetCurrentState((GTA5.GSI.PlayerState)Enum.Parse(typeof(GTA5.GSI.PlayerState), this.preview_team.SelectedIndex.ToString()));
            }
        }

        private void patch_button_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"Aurora.exe";
            startInfo.Arguments = @"-install_logitech";
            startInfo.Verb = "runas";
            Process.Start(startInfo);
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                (profile_manager.Settings as GTA5Settings).isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }
    }
}
