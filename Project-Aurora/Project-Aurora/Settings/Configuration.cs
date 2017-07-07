using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using Aurora.Profiles.Generic_Application;
using Aurora.Profiles.Overlays;
using Aurora.Profiles.Overlays.SkypeOverlay;
using Aurora.Profiles;

namespace Aurora.Settings
{
    /// <summary>
    /// Enum list for the percent effect type
    /// </summary>
    public enum PercentEffectType
    {
        /// <summary>
        /// All at once
        /// </summary>
        [Description("All at once")]
        AllAtOnce = 0,

        /// <summary>
        /// Progressive
        /// </summary>
        [Description("Progressive")]
        Progressive = 1,

        /// <summary>
        /// Progressive (Gradual)
        /// </summary>
        [Description("Progressive (Gradual)")]
        Progressive_Gradual = 2
    }

    public enum IdleEffects
    {
        [Description("None")]
        None = 0,
        [Description("Dim")]
        Dim = 1,
        [Description("Color Breathing")]
        ColorBreathing = 2,
        [Description("Rainbow Shift (Horizontal)")]
        RainbowShift_Horizontal = 3,
        [Description("Rainbow Shift (Vertical)")]
        RainbowShift_Vertical = 4,
        [Description("Star Fall")]
        StarFall = 5,
        [Description("Rain Fall")]
        RainFall = 6,
        [Description("Blackout")]
        Blackout = 7,
        [Description("Matrix")]
        Matrix = 8
    }

    /// <summary>
    /// Enum list for the layer effects
    /// </summary>
    public enum LayerEffects
    {
        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        None = 0,

        /// <summary>
        /// Single Color Overlay
        /// </summary>
        [Description("Single Color Overlay")]
        ColorOverlay = 1,

        /// <summary>
        /// Color Breathing
        /// </summary>
        [Description("Color Breathing")]
        ColorBreathing = 2,

        /// <summary>
        /// Rainbow Shift (Horizontal)
        /// </summary>
        [Description("Rainbow Shift (Horizontal)")]
        RainbowShift_Horizontal = 3,

        /// <summary>
        /// Rainbow Shift (Vertical)
        /// </summary>
        [Description("Rainbow Shift (Vertical)")]
        RainbowShift_Vertical = 4,

        /// <summary>
        /// Rainbow Shift (Diagonal)
        /// </summary>
        [Description("Rainbow Shift (Diagonal)")]
        RainbowShift_Diagonal = 5,

        /// <summary>
        /// Rainbow Shift (Other Diagonal)
        /// </summary>
        [Description("Rainbow Shift (Other Diagonal)")]
        RainbowShift_Diagonal_Other = 6,

        /// <summary>
        /// Rainbow Shift (Custom Angle)
        /// </summary>
        [Description("Rainbow Shift (Custom Angle)")]
        RainbowShift_Custom_Angle = 7,

        /// <summary>
        /// Gradient Shift (Custom Angle)
        /// </summary>
        [Description("Gradient Shift (Custom Angle)")]
        GradientShift_Custom_Angle = 8,
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
        [Description("Right Handed")]
        RightHanded = 1,
        [Description("Left Handed")]
        LeftHanded = 2
    }

    public enum PreferredKeyboard
    {
        [Description("None")]
        None = 0,
        /*
        [Description("Logitech")]
        Logitech = 1,
        [Description("Corsair")]
        Corsair = 2,
        [Description("Razer")]
        Razer = 3,
        
        [Description("Clevo")]
        Clevo = 4,
        [Description("Cooler Master")]
        CoolerMaster = 5,
        */

        //Logitech range is 100-199
        [Description("Logitech - G910")]
        Logitech_G910 = 100,
        [Description("Logitech - G410")]
        Logitech_G410 = 101,
        [Description("Logitech - G810")]
        Logitech_G810 = 102,

        //Corsair range is 200-299
        [Description("Corsair - K95")]
        Corsair_K95 = 200,
        [Description("Corsair - K70")]
        Corsair_K70 = 201,
        [Description("Corsair - K65")]
        Corsair_K65 = 202,
        [Description("Corsair - STRAFE")]
        Corsair_STRAFE = 203,

        //Razer range is 300-399
        [Description("Razer - Blackwidow")]
        Razer_Blackwidow = 300,
        [Description("Razer - Blackwidow X")]
        Razer_Blackwidow_X = 301,
        [Description("Razer - Blackwidow Tournament Edition")]
        Razer_Blackwidow_TE = 302,

        //Clevo range is 400-499

        //Cooler Master range is 500-599
        [Description("Masterkeys Pro L")]
        Masterkeys_Pro_L = 500,
        [Description("Masterkeys Pro S")]
        Masterkeys_Pro_S = 501,
        [Description("Masterkeys Pro L White")]
        Masterkeys_Pro_L_White = 502,
        [Description("Masterkeys Pro M White")]
        Masterkeys_Pro_M_White = 503,

        //Roccat range is 600-699
        //[Description("Roccat Ryos")]
        //Roccat_Ryos = 600

        //Steelseries range is 700-799
        [Description("SteelSeries Apex M800")]
        SteelSeries_Apex_M800 = 700,
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
        [Description("Japanese")]
        jpn = 7

    }

    public enum PreferredMouse
    {
        [Description("None")]
        None = 0,

        [Description("Generic Peripheral")]
        Generic_Peripheral = 1,

        //Logitech range is 100-199
        [Description("Logitech - G900")]
        Logitech_G900 = 100,

        //Corsair range is 200-299
        [Description("Corsair - Sabre")]
        Corsair_Sabre = 200,
        [Description("Corsair - M65")]
        Corsair_M65 = 201,
        [Description("Corsair - Katar")]
        Corsair_Katar = 202,

        //Razer range is 300-399

        //Clevo range is 400-499
        [Description("Clevo - Touchpad")]
        Clevo_Touchpad = 400

        //Cooler Master range is 500-599

        //Roccat range is 600-699

        //Steelseries range is 700-799
    }

    public enum KeycapType
    {
        [Description("Default")]
        Default = 0,
        [Description("Default (with Backglow)")]
        Default_backglow = 1,
        [Description("Default (Backglow only)")]
        Default_backglow_only = 2,
        [Description("Colorized")]
        Colorized = 3,
        [Description("Colorized (blank)")]
        Colorized_blank = 4
    }

    public enum ApplicationDetectionMode
    {
        [Description("Windows Events (Default)")]
        WindowsEvents = 0,

        [Description("Foreground App Scan")]
        ForegroroundApp = 1
    }

    public class Configuration : Settings
    {
        //First Time Installs
        public bool redist_first_time;
        public bool logitech_first_time;
        public bool corsair_first_time;
        public bool razer_first_time;
        public bool steelseries_first_time;

        //General Program Settings
        public bool allow_peripheral_devices;
        public bool use_volume_as_brightness;
        public bool allow_wrappers_in_background;
        public bool allow_all_logitech_bitmaps;

        private float globalBrightness = 1.0f;
        [JsonProperty(PropertyName = "global_brightness")]
        public float GlobalBrightness { get { return globalBrightness; } set { globalBrightness = value; InvokePropertyChanged(); } }

        private float keyboardBrightness = 1.0f;
        [JsonProperty(PropertyName = "keyboard_brightness_modifier")]
        public float KeyboardBrightness { get { return keyboardBrightness; } set{ keyboardBrightness = value; InvokePropertyChanged(); } }

        private float peripheralBrightness;
        [JsonProperty(PropertyName = "peripheral_brightness_modifier")]
        public float PeripheralBrightness { get { return peripheralBrightness; } set { peripheralBrightness = value; InvokePropertyChanged(); } }

        public bool updates_check_on_start_up;
        public bool updates_allow_silent_minor;
        public bool start_silently;
        public AppExitMode close_mode;
        public MouseOrientationType mouse_orientation;
        public PreferredKeyboard keyboard_brand;
        public PreferredKeyboardLocalization keyboard_localization;
        public PreferredMouse mouse_preference;
        public KeycapType virtualkeyboard_keycap_type;
        public ApplicationDetectionMode detection_mode;
        public HashSet<String> excluded_programs;
        public bool devices_disable_keyboard;
        public bool devices_disable_mouse;
        public bool devices_disable_headset;
        public HashSet<Type> devices_disabled;
        public bool OverlaysInPreview;

        //[JsonIgnoreAttribute]
        //public Dictionary<string, GenericApplicationProfileManager> additional_profiles;

        //Blackout and Night theme
        public bool time_based_dimming_enabled;
        public bool time_based_dimming_affect_games;
        public int time_based_dimming_start_hour;
        public int time_based_dimming_start_minute;
        public int time_based_dimming_end_hour;
        public int time_based_dimming_end_minute;

        public bool nighttime_enabled;
        public int nighttime_start_hour;
        public int nighttime_start_minute;
        public int nighttime_end_hour;
        public int nighttime_end_minute;

        // Idle Effects
        public IdleEffects idle_type;
        public int idle_delay;
        public float idle_speed;
        public Color idle_effect_primary_color;
        public Color idle_effect_secondary_color;
        public int idle_amount;
        public float idle_frequency;

        //Game Settings
        /*[JsonIgnoreAttribute]
        public ProfileManager desktop_profile = new Profiles.Desktop.DesktopProfileManager();*/

        public VariableRegistry VarRegistry;

        /*[JsonIgnoreAttribute]
        public Dictionary<string, ProfileManager> ApplicationProfiles = new Dictionary<string, ProfileManager>()
        {
            { "Dota 2", new Profiles.Dota_2.Dota2ProfileManager() },
            { "CSGO", new Profiles.CSGO.CSGOProfileManager() },
            { "GTA5", new Profiles.GTA5.GTA5ProfileManager() },
            { "RocketLeague", new Profiles.RocketLeague.RocketLeagueProfileManager() },
            { "Overwatch", new Profiles.Overwatch.OverwatchProfileManager() },
            { "Payday 2", new Profiles.Payday_2.PD2ProfileManager() },
            { "The Division", new Profiles.TheDivision.TheDivisionProfileManager() },
            { "League of Legends", new Profiles.LeagueOfLegends.LoLProfileManager() },
            { "Hotline", new Profiles.HotlineMiami.HMProfileManager() },
            { "Talos", new Profiles.TheTalosPrinciple.TalosPrincipleProfileManager() },
            { "BF3", new Profiles.BF3.BF3ProfileManager() },
            { "BLight", new Profiles.Blacklight.BLightProfileManager() },
            { "MagicDuels2012", new Profiles.Magic_Duels_2012.MagicDuels2012ProfileManager() },
            { "ShadowOfMordor", new Profiles.ShadowOfMordor.ShadowOfMordorProfileManager() },
            { "SSam3", new Profiles.Serious_Sam_3.SSam3ProfileManager() },
            { "DiscoDodgeball", new Profiles.DiscoDodgeball.DiscoDodgeballProfileManager() },
            { "XCOM", new Profiles.XCOM.XCOMProfileManager() },
            { "Evolve", new Profiles.Evolve.EvolveProfileManager() },
            { "MetroLL", new Profiles.Metro_Last_Light.MetroLLProfileManager() },
            { "GW2", new Profiles.Guild_Wars_2.GW2ProfileManager() },
            { "WormsWMD", new Profiles.WormsWMD.WormsWMDProfileManager() },
            { "BnS", new Profiles.Blade_and_Soul.BnSProfileManager() }
        };*/

        //Overlay Settings
        public VolumeOverlaySettings volume_overlay_settings;
        public SkypeOverlaySettings skype_overlay_settings;

        public List<string> ProfileOrder { get; set; } = new List<string>();

        public Configuration()
        {
            //First Time Installs
            redist_first_time = true;
            logitech_first_time = true;
            corsair_first_time = true;
            razer_first_time = true;
            steelseries_first_time = true;

            //General Program Settings
            allow_peripheral_devices = true;
            use_volume_as_brightness = false;
            allow_wrappers_in_background = true;
            allow_all_logitech_bitmaps = true;
            GlobalBrightness = 1.0f;
            KeyboardBrightness = 1.0f;
            peripheralBrightness = 1.0f;
            updates_check_on_start_up = true;
            updates_allow_silent_minor = true;
            start_silently = false;
            close_mode = AppExitMode.Ask;
            mouse_orientation = MouseOrientationType.RightHanded;
            keyboard_brand = PreferredKeyboard.None;
            keyboard_localization = PreferredKeyboardLocalization.None;
            mouse_preference = PreferredMouse.None;
            virtualkeyboard_keycap_type = KeycapType.Default;
            detection_mode = ApplicationDetectionMode.WindowsEvents;
            excluded_programs = new HashSet<string>();
            //additional_profiles = new Dictionary<string, GenericApplicationProfileManager>();
            devices_disable_keyboard = false;
            devices_disable_mouse = false;
            devices_disable_headset = false;
            devices_disabled = new HashSet<Type>();
            OverlaysInPreview = false;

            //Blackout and Night theme
            time_based_dimming_enabled = false;
            time_based_dimming_affect_games = false;
            time_based_dimming_start_hour = 21;
            time_based_dimming_start_minute = 0;
            time_based_dimming_end_hour = 8;
            time_based_dimming_end_minute = 0;

            nighttime_enabled = true;
            nighttime_start_hour = 20;
            nighttime_start_minute = 0;
            nighttime_end_hour = 7;
            nighttime_end_minute = 0;

            //// Idle Effects
            idle_type = IdleEffects.None;
            idle_delay = 5;
            idle_speed = 1.0f;
            idle_effect_primary_color = Color.FromArgb(0, 255, 0);
            idle_effect_secondary_color = Color.FromArgb(0, 0, 0);
            idle_amount = 5;
            idle_frequency = 2.5f;

            //Overlay Settings
            volume_overlay_settings = new VolumeOverlaySettings();
            skype_overlay_settings = new SkypeOverlaySettings();

            //ProfileOrder = new List<string>(ApplicationProfiles.Keys);

            VarRegistry = new VariableRegistry();
        }
  }

    public class ConfigManager
    {
        private static string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Config");
        private const string ConfigExtension = ".json";

        private static long _last_save_time = 0L;
        private readonly static long _save_interval = 1000L;

        public static Configuration Load()
        {
            var configPath = ConfigPath + ConfigExtension;

            if (!File.Exists(configPath))
                return CreateDefaultConfigurationFile();

            string content = File.ReadAllText(configPath, Encoding.UTF8);

            if (String.IsNullOrWhiteSpace(content))
                return CreateDefaultConfigurationFile();

            Configuration config = JsonConvert.DeserializeObject<Configuration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder });

            /*foreach (var kvp in config.ApplicationProfiles)
            {
                if (!config.ProfileOrder.Contains(kvp.Key))
                    config.ProfileOrder.Add(kvp.Key);
            }*/


            /*if (Directory.Exists(AdditionalProfilesPath))
            {
                List<string> additionals = new List<string>(Directory.EnumerateDirectories(AdditionalProfilesPath));
                foreach (var dir in additionals)
                {
                    if (File.Exists(Path.Combine(dir, "default.json")))
                    {
                        string proccess_name = Path.GetFileName(dir);
                        config.additional_profiles.Add(proccess_name, new GenericApplicationProfileManager(proccess_name));
                    }
                }
            }*/

            return config;
        }

        public static void Save(Configuration configuration)
        {
            long current_time = Utils.Time.GetMillisecondsSinceEpoch();

            if (_last_save_time + _save_interval > current_time)
                return;
            else
                _last_save_time = current_time;

            var configPath = ConfigPath + ConfigExtension;
            string content = JsonConvert.SerializeObject(configuration, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder });

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, content, Encoding.UTF8);

            Global.LightingStateManager.SaveAll();

            /*configuration.desktop_profile.SaveProfiles();

            foreach (var kvp in configuration.ApplicationProfiles)
                kvp.Value.SaveProfiles();

            foreach (var kvp in configuration.additional_profiles)
                kvp.Value.SaveProfiles();*/
        }

        private static Configuration CreateDefaultConfigurationFile()
        {
            Configuration config = new Configuration();
            var configData = JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder });
            var configPath = ConfigPath + ConfigExtension;

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, configData, Encoding.UTF8);

            return config;
        }
    }
}
