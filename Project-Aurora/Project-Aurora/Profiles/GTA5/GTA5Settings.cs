using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.GTA5
{
    public enum GTA5_PoliceEffects
    {
        [Description("Default")]
        Default = 0,
        [Description("Half Alternating")]
        Alt_Half = 1,
        [Description("Full Alternating")]
        Alt_Full = 2,
        [Description("Half Alternating (Blinking)")]
        Alt_Half_Blink = 3,
        [Description("Full Alternating (Blinking)")]
        Alt_Full_Blink = 4,
    }

    public class GTA5Settings : ProfileSettings
    {
        //Effects
        //// Background
        public bool bg_color_enabled;
        public Color bg_ambient;
        public Color bg_franklin;
        public Color bg_chop;
        public Color bg_michael;
        public Color bg_trevor;
        public Color bg_online;
        public Color bg_online_mission;
        public Color bg_online_heistfinale;
        public Color bg_online_spectator;
        public Color bg_race_gold;
        public Color bg_race_silver;
        public Color bg_race_bronze;
        public bool bg_peripheral_use;

        //// Police Siren
        public bool siren_enabled;
        public GTA5_PoliceEffects siren_type;
        public Color left_siren_color;
        public Color middle_siren_color;
        public Color right_siren_color;
        public KeySequence left_siren_sequence;
        public KeySequence middle_siren_sequence;
        public KeySequence right_siren_sequence;
        public bool siren_peripheral_use;

        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public GTA5Settings()
        {
            //General
            IsEnabled = true;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                new Settings.Layers.Layer("GTA 5 Police Siren", new Layers.GTA5PoliceSirenLayerHandler()),
                new Settings.Layers.Layer("GTA 5 Background", new Layers.GTA5BackgroundLayerHandler())
            };

            //Effects
            //// Background
            bg_color_enabled = true;
            bg_ambient = Color.FromArgb(255, 255, 255);
            bg_franklin = Color.FromArgb(48, 255, 0);
            bg_chop = Color.FromArgb(127, 0, 0);
            bg_michael = Color.FromArgb(48, 255, 255);
            bg_trevor = Color.FromArgb(176, 80, 0);
            bg_online = Color.FromArgb(0, 70, 228);
            bg_online_mission = Color.FromArgb(156, 110, 175);
            bg_online_heistfinale = Color.FromArgb(255, 122, 196);
            bg_online_spectator = Color.FromArgb(142, 127, 153);
            bg_race_gold = Color.FromArgb(255, 170, 0);
            bg_race_silver = Color.FromArgb(191, 191, 191);
            bg_race_bronze = Color.FromArgb(255, 51, 0);
            bg_peripheral_use = true;

            //// Police Siren
            siren_enabled = true;
            siren_type = GTA5_PoliceEffects.Default;
            left_siren_color = Color.FromArgb(255, 0, 0);
            middle_siren_color = Color.FromArgb(255, 255, 255);
            right_siren_color = Color.FromArgb(0, 0, 255);
            left_siren_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3,
                Devices.DeviceKeys.F4, Devices.DeviceKeys.F5, Devices.DeviceKeys.F6
            });
            middle_siren_sequence = new KeySequence();
            right_siren_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9,
                Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
            });
            siren_peripheral_use = true;

            //// Lighting Areas
            lighting_areas = new List<ColorZone>();
        }
    }
}
