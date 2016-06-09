using Aurora.Profiles.CSGO;
using Aurora.Profiles.Desktop;
using Aurora.Profiles.Dota_2;
using Aurora.Profiles.Generic_Application;
using Aurora.Profiles.GTA5;
using Aurora.Profiles.Overlays;
using Aurora.Profiles.Overwatch;
using Aurora.Profiles.Payday_2;
using Aurora.Profiles.RocketLeague;
using Aurora.Profiles.TheDivision;
using LedCSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aurora.Settings
{
    public enum PercentEffectType
    {
        [Description("All at once")]
        AllAtOnce = 0,
        [Description("Progressive")]
        Progressive = 1,
        [Description("Progressive (Gradual)")]
        Progressive_Gradual = 2
    }

    public enum LayerEffects
    {
        [Description("None")]
        None = 0,
        [Description("Single Color Overlay")]
        ColorOverlay = 1,
        [Description("Color Breathing")]
        ColorBreathing = 2,
        [Description("Rainbow Shift (Horizontal)")]
        RainbowShift_Horizontal = 3,
        [Description("Rainbow Shift (Vertical)")]
        RainbowShift_Vertical = 4,
        [Description("Rainbow Shift (Diagonal)")]
        RainbowShift_Diagonal = 5,
        [Description("Rainbow Shift (Other Diagonal)")]
        RainbowShift_Diagonal_Other = 6,
        [Description("Rainbow Shift (Custom Angle)")]
        RainbowShift_Custom_Angle = 7,
    }

    public enum MouseOrientationType
    {
        [Description("None")]
        None = 0,
        [Description("Right Handed")]
        RightHanded = 1,
        [Description("Left Handed")]
        LeftHanded = 2
    }

    public enum PreferredKeyboard
    {
        [Description("Automatic Detection")]
        None = 0,
        [Description("Logitech")]
        Logitech = 1,
        [Description("Corsair")]
        Corsair = 2,
        [Description("Razer")]
        Razer = 3
    }

    public enum PreferredKeyboardLocalization
    {
        [Description("Automatic Detection")]
        None = 0,
        [Description("International")]
        intl = 1,
        [Description("United States")]
        us = 2,
        [Description("United Kingdom")]
        uk = 3,
        [Description("Russian")]
        ru = 4,
        [Description("French")]
        fr = 5,
        [Description("Deutch")]
        de = 6
    }

    public class Configuration
    {
        //First Time Installs
        public bool redist_first_time;
        public bool logitech_first_time;
        public bool corsair_first_time;
        public bool razer_first_time;

        //General Program Settings
        public bool allow_peripheral_devices;
        public bool use_volume_as_brightness;
        public bool allow_all_logitech_bitmaps;
        public bool logitech_enhance_brightness;
        public float global_brightness;
        public float keyboard_brightness_modifier;
        public float peripheral_brightness_modifier;
        public bool updates_check_on_start_up;
        public bool updates_allow_silent_minor;
        public MouseOrientationType mouse_orientation;
        public PreferredKeyboard keyboard_brand;
        public PreferredKeyboardLocalization keyboard_localization;
        public HashSet<String> excluded_programs;
        public Dictionary<String, GenericApplicationSettings> additional_profiles;

        //Game Settings
        public DesktopSettings desktop_settings;
        public Dota2Settings dota2_settings;
        public CSGOSettings csgo_settings;
        public GTA5Settings gta5_settings;
        public RocketLeagueSettings rocketleague_settings;
        public OverwatchSettings overwatch_settings;
        public PD2Settings pd2_settings;
        public TheDivisionSettings division_settings;

        //Overlay Settings
        public VolumeOverlaySettings volume_overlay_settings;

        public Configuration()
        {
            //First Time Installs
            redist_first_time = true;
            logitech_first_time = true;
            corsair_first_time = true;
            razer_first_time = true;

            //General Program Settings
            allow_peripheral_devices = true;
            use_volume_as_brightness = false;
            allow_all_logitech_bitmaps = true;
            logitech_enhance_brightness = true;
            global_brightness = 1.0f;
            keyboard_brightness_modifier = 1.0f;
            peripheral_brightness_modifier = 1.0f;
            updates_check_on_start_up = true;
            updates_allow_silent_minor = true;
            mouse_orientation = MouseOrientationType.RightHanded;
            keyboard_brand = PreferredKeyboard.None;
            keyboard_localization = PreferredKeyboardLocalization.None;
            excluded_programs = new HashSet<string>();
            additional_profiles = new Dictionary<string, GenericApplicationSettings>();

            //Game Settings
            desktop_settings = new DesktopSettings();
            dota2_settings = new Dota2Settings();
            csgo_settings = new CSGOSettings();
            gta5_settings = new GTA5Settings();
            rocketleague_settings = new RocketLeagueSettings();
            overwatch_settings = new OverwatchSettings();
            pd2_settings = new PD2Settings();
            division_settings = new TheDivisionSettings();

            //Overlay Settings
            volume_overlay_settings = new VolumeOverlaySettings();
        }
    }

    public class ConfigManager
    {
        private static string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Config");
        private const string ConfigExtension = ".json";

        public static Configuration Load()
        {
            var configPath = ConfigPath + ConfigExtension;

            if (!File.Exists(configPath))
                return CreateDefaultConfigurationFile();

            string content = File.ReadAllText(configPath, Encoding.UTF8);

            if (String.IsNullOrWhiteSpace(content))
                return CreateDefaultConfigurationFile();

            return JsonConvert.DeserializeObject<Configuration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
        }

        public static void Save(Configuration configuration)
        {
            var configPath = ConfigPath + ConfigExtension;
            string content = JsonConvert.SerializeObject(configuration, Formatting.Indented);

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, content, Encoding.UTF8);
        }

        private static Configuration CreateDefaultConfigurationFile()
        {
            Configuration config = new Configuration();
            var configData = JsonConvert.SerializeObject(config, Formatting.Indented);
            var configPath = ConfigPath + ConfigExtension;

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, configData, Encoding.UTF8);

            return config;
        }
    }
}
