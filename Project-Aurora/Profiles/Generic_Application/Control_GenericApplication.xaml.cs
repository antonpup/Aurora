using Aurora.Controls;
using Aurora.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Profiles.Generic_Application
{
    /// <summary>
    /// Interaction logic for Control_GenericApplication.xaml
    /// </summary>
    public partial class Control_GenericApplication : UserControl
    {
        private ProfileManager profile_manager;

        public Control_GenericApplication(ProfileManager profile)
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

            this.profile_enabled.IsChecked = (profile_manager.Settings as GenericApplicationSettings).isEnabled;
            this.app_name_textbox.Text = (profile_manager.Settings as GenericApplicationSettings).ApplicationName;

            this.sc_assistant_enabled.IsChecked = (profile_manager.Settings as GenericApplicationSettings).shortcuts_assistant_enabled;
            this.sc_assistant_ctrl_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GenericApplicationSettings).ctrl_key_color);
            this.sc_assistant_ctrl_keys.Sequence = (profile_manager.Settings as GenericApplicationSettings).ctrl_key_sequence;
            this.sc_assistant_alt_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((profile_manager.Settings as GenericApplicationSettings).alt_key_color);
            this.sc_assistant_alt_keys.Sequence = (profile_manager.Settings as GenericApplicationSettings).alt_key_sequence;

            this.cz_day.ColorZonesList = (profile_manager.Settings as GenericApplicationSettings).lighting_areas_day;
            this.cz_night.ColorZonesList = (profile_manager.Settings as GenericApplicationSettings).lighting_areas_night;
        }
        
        private bool HasProfile()
        {
            return Global.Configuration.additional_profiles.ContainsKey(profile_manager.ProcessNames[0]);
        }
               
        private void app_name_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                if (HasProfile())
                    (profile_manager.Settings as GenericApplicationSettings).ApplicationName = app_name_textbox.Text;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void sc_assistant_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).shortcuts_assistant_enabled = (this.sc_assistant_enabled.IsChecked.HasValue) ? this.sc_assistant_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_ctrl_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.sc_assistant_ctrl_color.SelectedColor.HasValue && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).ctrl_key_color = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_ctrl_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_ctrl_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).ctrl_key_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_alt_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && this.sc_assistant_alt_color.SelectedColor.HasValue && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).alt_key_color = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_alt_color.SelectedColor.Value);
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void sc_assistant_alt_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).alt_key_sequence = (sender as Controls.KeySequence).Sequence;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cz_day_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsInitialized && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).lighting_areas_day = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cz_night_ColorZonesListUpdated(object sender, EventArgs e)
        {
            if (IsInitialized && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).lighting_areas_night = (sender as ColorZones).ColorZonesList;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void profile_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && HasProfile())
            {
                (profile_manager.Settings as GenericApplicationSettings).isEnabled = (this.profile_enabled.IsChecked.HasValue) ? this.profile_enabled.IsChecked.Value : false;
                ConfigManager.Save(Global.Configuration);

            }
        }
    }
}
