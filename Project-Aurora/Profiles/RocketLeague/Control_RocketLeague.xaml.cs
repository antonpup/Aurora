using Aurora.Controls;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.RocketLeague
{
    /// <summary>
    /// Interaction logic for Control_RocketLeague.xaml
    /// </summary>
    public partial class Control_RocketLeague : UserControl
    {
        public Control_RocketLeague()
        {
            InitializeComponent();

            this.game_enabled.IsChecked = Global.Configuration.rocketleague_settings.isEnabled;

            this.bg_enabled.IsChecked = Global.Configuration.rocketleague_settings.bg_enabled;
            this.bg_ambient_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor( Global.Configuration.rocketleague_settings.bg_ambient_color );
            this.bg_use_team_colors.IsChecked = Global.Configuration.rocketleague_settings.bg_use_team_color;
            this.bg_team1_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.bg_team_1);
            this.bg_team2_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.bg_team_2);
            this.bg_show_team_score_split.IsChecked = Global.Configuration.rocketleague_settings.bg_show_team_score_split;

            this.boost_enabled.IsChecked = Global.Configuration.rocketleague_settings.boost_enabled;
            this.boost_low_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.boost_low);
            this.boost_med_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.boost_mid);
            this.boost_high_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.boost_high);
            this.boost_ks.Sequence = Global.Configuration.rocketleague_settings.boost_sequence;
            this.boost_peripheral_use_enabled.IsChecked = Global.Configuration.rocketleague_settings.boost_peripheral_use;

            this.speed_enabled.IsChecked = Global.Configuration.rocketleague_settings.speed_enabled;
            this.speed_low_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.speed_low);
            this.speed_med_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.speed_mid);
            this.speed_high_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.rocketleague_settings.speed_high);
            this.speed_ks.Sequence = Global.Configuration.rocketleague_settings.speed_sequence;

            this.colorzones.ColorZonesList = Global.Configuration.rocketleague_settings.lighting_areas;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Global.Configuration.rocketleague_settings.isEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bg_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if(IsLoaded && sender is CheckBox)
            {
                Global.Configuration.rocketleague_settings.bg_enabled = (sender as CheckBox).IsChecked.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bg_ambient_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if(IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.bg_ambient_color = Utils.ColorUtils.MediaColorToDrawingColor( (sender as ColorPicker).SelectedColor.Value );
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bg_use_team_colors_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.rocketleague_settings.bg_use_team_color = (sender as CheckBox).IsChecked.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bg_team1_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.bg_team_1 = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bg_team2_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.bg_team_2 = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void boost_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.rocketleague_settings.boost_enabled = (sender as CheckBox).IsChecked.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void boost_low_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.boost_low = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void boost_med_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.boost_mid = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void boost_high_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.boost_mid = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void boost_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && sender is Controls.KeySequence)
            {
                Global.Configuration.rocketleague_settings.boost_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void boost_peripheral_use_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.rocketleague_settings.boost_peripheral_use = (sender as CheckBox).IsChecked.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void speed_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.rocketleague_settings.speed_enabled = (sender as CheckBox).IsChecked.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void speed_low_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.speed_low = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void speed_med_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.speed_mid = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void speed_high_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
            {
                Global.Configuration.rocketleague_settings.speed_high = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void speed_ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && sender is Controls.KeySequence)
            {
                Global.Configuration.rocketleague_settings.speed_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void colorzones_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && sender is ColorZones)
            {
                Global.Configuration.rocketleague_settings.lighting_areas = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void bg_show_team_score_split_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Global.Configuration.rocketleague_settings.bg_show_team_score_split = (sender as CheckBox).IsChecked.Value;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Predefined, "rocketleague.exe");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
        }

        private void preview_boost_amount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(sender is Slider)
            {
                this.preview_boost_amount_label.Text = (int)((sender as Slider).Value)+"%";

                if(IsLoaded)
                    GameEvent_RocketLeague.SetBoost((float)((sender as Slider).Value)/33.0f);
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
