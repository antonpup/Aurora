using Aurora.Profiles.CSGO;
using Aurora.Profiles.Desktop;
using Aurora.Profiles.Dota_2;
using Aurora.Profiles.Generic_Application;
using Aurora.Profiles.GTA5;
using Aurora.Profiles.HotlineMiami;
using Aurora.Profiles.LeagueOfLegends;
using Aurora.Profiles.Logitech_Wrapper;
using Aurora.Profiles.Overlays;
using Aurora.Profiles.Overlays.SkypeOverlay;
using Aurora.Profiles.Overwatch;
using Aurora.Profiles.Payday_2;
using Aurora.Profiles.RocketLeague;
using Aurora.Profiles.TheDivision;
using Aurora.Profiles.TheTalosPrinciple;
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
using System.Windows.Media.Imaging;

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

    public enum AppExitMode
    {
        [Description("Let user decide to minimize or exit")]
        Ask = 0,
        [Description("Always Minimize")]
        Minimize = 1,
        [Description("Always Exit")]
        Exit = 2
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
        [Description("Deutsch")]
        de = 6,
        [Description("Japanese (Logitech Only)")]
        jpn = 7

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
        public AppExitMode close_mode;
        public MouseOrientationType mouse_orientation;
        public PreferredKeyboard keyboard_brand;
        public PreferredKeyboardLocalization keyboard_localization;
        public HashSet<String> excluded_programs;
        public Dictionary<String, GenericApplicationSettings> additional_profiles;

        //Game Settings
        [JsonIgnoreAttribute]
        public ProfileManager dekstop_profile = new DesktopProfileManager();

        [JsonIgnoreAttribute]
        public Dictionary<string, ProfileManager> ApplicationProfiles = new Dictionary<string, ProfileManager>()
        {
            { "Dota 2", new Dota2ProfileManager() },
            { "CSGO", new CSGOProfileManager() },
            { "GTA5", new GTA5ProfileManager() },
            { "RocketLeague", new RocketLeagueProfileManager() },
            { "Overwatch", new OverwatchProfileManager() },
            { "Payday 2", new PD2ProfileManager() },
            { "The Division", new TheDivisionProfileManager() },
            { "League of Legends", new LoLProfileManager() },
            { "Hotline", new HMProfileManager() },
            { "Talos", new TalosPrincipleProfileManager() }
        };

        //Overlay Settings
        public VolumeOverlaySettings volume_overlay_settings;
        public SkypeOverlaySettings skype_overlay_settings;

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
            close_mode = AppExitMode.Ask;
            mouse_orientation = MouseOrientationType.RightHanded;
            keyboard_brand = PreferredKeyboard.None;
            keyboard_localization = PreferredKeyboardLocalization.None;
            excluded_programs = new HashSet<string>();
            additional_profiles = new Dictionary<string, GenericApplicationSettings>();

            //Overlay Settings
            volume_overlay_settings = new VolumeOverlaySettings();
            skype_overlay_settings = new SkypeOverlaySettings();
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

            Configuration config = JsonConvert.DeserializeObject<Configuration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

            config.dekstop_profile.LoadProfiles();

            foreach (var kvp in config.ApplicationProfiles)
                kvp.Value.LoadProfiles();

            return config;
        }

        public static T Load<T>(string path = "") where T : class
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.Filter = "JSON File|*.json";
                    dialog.Title = "Open a profile";
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                    if (result != System.Windows.Forms.DialogResult.OK)
                        return null;

                    path = dialog.FileName;
                }

                string content = File.ReadAllText(path, Encoding.UTF8);

                if (String.IsNullOrWhiteSpace(content))
                    return null;

                return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception during ConfigManager.Save<T>(). Error: " + exc, Logging_Level.Error);
                System.Windows.MessageBox.Show("Exception during ConfigManager.Save<T>().Error: " + exc.Message, "Aurora - Error");

                return null;
            }
        }

        public static void Save(Configuration configuration)
        {
            var configPath = ConfigPath + ConfigExtension;
            string content = JsonConvert.SerializeObject(configuration, Formatting.Indented);

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, content, Encoding.UTF8);

            configuration.dekstop_profile.SaveProfiles();

            foreach (var kvp in configuration.ApplicationProfiles)
                kvp.Value.SaveProfiles();
        }

        public static bool Save<T>(T configuration, string path = "") where T : class
        {
            if (configuration == null)
                return false;

            try
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    var dialog = new System.Windows.Forms.SaveFileDialog();
                    dialog.Filter = "JSON File|*.json";
                    dialog.Title = "Save a profile";
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                    if (result != System.Windows.Forms.DialogResult.OK)
                        return false;

                    path = dialog.FileName;
                }

                string content = JsonConvert.SerializeObject(configuration, Formatting.Indented);

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.WriteAllText(path, content, Encoding.UTF8);
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception during ConfigManager.Save<T>(). Error: " + exc, Logging_Level.Error);
                System.Windows.MessageBox.Show("Exception during ConfigManager.Save<T>().Error: " + exc.Message, "Aurora - Error");

                return false;
            }

            return true;
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
