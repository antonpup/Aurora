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

namespace Aurora.Profiles.Generic_Application
{
    /// <summary>
    /// Interaction logic for Control_GenericApplication.xaml
    /// </summary>
    public partial class Control_GenericApplication : UserControl
    {
        private string app_key = "";

        public Control_GenericApplication(string application_key)
        {
            InitializeComponent();

            app_key = application_key;

            if (Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                this.profile_enabled.IsChecked = Global.Configuration.additional_profiles[app_key].isEnabled;
                this.app_name_textbox.Text = Global.Configuration.additional_profiles[app_key].ApplicationName;

                this.sc_assistant_enabled.IsChecked = Global.Configuration.additional_profiles[app_key].shortcuts_assistant_enabled;
                this.sc_assistant_ctrl_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.additional_profiles[app_key].ctrl_key_color);
                this.sc_assistant_ctrl_keys.Sequence = Global.Configuration.additional_profiles[app_key].ctrl_key_sequence;
                this.sc_assistant_alt_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(Global.Configuration.additional_profiles[app_key].alt_key_color);
                this.sc_assistant_alt_keys.Sequence = Global.Configuration.additional_profiles[app_key].alt_key_sequence;

                this.cz_day.ColorZonesList = Global.Configuration.additional_profiles[app_key].lighting_areas_day;
                this.cz_night.ColorZonesList = Global.Configuration.additional_profiles[app_key].lighting_areas_night;
            }
        }

        private void app_name_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                if (Global.Configuration.additional_profiles.ContainsKey(app_key))
                    Global.Configuration.additional_profiles[app_key].ApplicationName = app_name_textbox.Text;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.GenericApplication, app_key);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.geh.SetPreview(PreviewType.Desktop);
        }

        private void sc_assistant_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].shortcuts_assistant_enabled = (this.sc_assistant_enabled.IsChecked.HasValue) ? this.sc_assistant_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_ctrl_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.sc_assistant_ctrl_color.SelectedColor.HasValue && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].ctrl_key_color = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_ctrl_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_ctrl_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].ctrl_key_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_alt_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.sc_assistant_alt_color.SelectedColor.HasValue && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].alt_key_color = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_alt_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_alt_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].alt_key_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cz_day_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsInitialized && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].lighting_areas_day = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cz_night_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsInitialized && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].lighting_areas_night = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void profile_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && Global.Configuration.additional_profiles.ContainsKey(app_key))
            {
                Global.Configuration.additional_profiles[app_key].isEnabled = (this.profile_enabled.IsChecked.HasValue) ? this.profile_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);

            }
        }
    }
}
