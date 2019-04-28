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
using Newtonsoft.Json.Serialization;

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
        [Description("Always Ask")]
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

        [Description("Generic Laptop")]
        GenericLaptop = 1,

        [Description("Generic Laptop (Numpad)")]
        GenericLaptopNumpad = 2,
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
        [Description("Logitech - GPRO")]
        Logitech_GPRO = 103,
		[Description("Logitech - G213")]
        Logitech_G213 = 104,

        //Corsair range is 200-299
        [Description("Corsair - K95")]
        Corsair_K95 = 200,
        [Description("Corsair - K70")]
        Corsair_K70 = 201,
        [Description("Corsair - K65")]
        Corsair_K65 = 202,
        [Description("Corsair - STRAFE")]
        Corsair_STRAFE = 203,
        [Description("Corsair - K95 Platinum")]
        Corsair_K95_PL = 204,
        [Description("Corsair - K68")]
        Corsair_K68 = 205,
        [Description("Corsair - K70 MK2")]
        Corsair_K70MK2 = 206
            ,
        //Razer range is 300-399
        [Description("Razer - Blackwidow")]
        Razer_Blackwidow = 300,
        [Description("Razer - Blackwidow X")]
        Razer_Blackwidow_X = 301,
        [Description("Razer - Blackwidow Tournament Edition")]
        Razer_Blackwidow_TE = 302,
        [Description("Razer - Blade")]
        Razer_Blade = 303,

        //Clevo range is 400-499

        //Cooler Master range is 500-599
        [Description("Cooler Master - Masterkeys Pro L")]
        Masterkeys_Pro_L = 500,
        [Description("Cooler Master - Masterkeys Pro S")]
        Masterkeys_Pro_S = 501,
        [Description("Cooler Master - Masterkeys Pro M")]
        Masterkeys_Pro_M = 502,
        [Description("Cooler Master - Masterkeys MK750")]
        Masterkeys_MK750 = 503,

        //Roccat range is 600-699
        [Description("Roccat Ryos")]
        Roccat_Ryos = 600,

        //Steelseries range is 700-799
        [Description("SteelSeries Apex M800")]
        SteelSeries_Apex_M800 = 700,
        [Description("SteelSeries Apex M750")]
        SteelSeries_Apex_M750 = 701,
        [Description("SteelSeries Apex M750 TKL")]
        SteelSeries_Apex_M750_TKL = 702,

        [Description("Wooting One")]
        Wooting_One = 800,

        [Description("Asus Strix Flare")]
        Asus_Strix_Flare = 900,

        //Drevo range is 1000-1099
        [Description("Drevo BladeMaster")]
        Drevo_BladeMaster = 1000,

	//Creative range is 1100-1199
        [Description("SoundBlasterX VanguardK08")]
        SoundBlasterX_Vanguard_K08 = 1100,
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
        jpn = 7,
        [Description("Turkish")]
        tr = 8,
        [Description("Nordic")]
        nordic = 9,
        [Description("Swiss")]
        swiss = 10,
        [Description("Portuguese (Brazilian ABNT2)")]
        abnt2 = 11,
        [Description("DVORAK (US)")]
        dvorak = 12,
        [Description("DVORAK (INT)")]
        dvorak_int = 13,
        [Description("Hungarian")]
        hu = 14
    }

    public enum PreferredMouse
    {
        [Description("None")]
        None = 0,

        [Description("Generic Peripheral")]
        Generic_Peripheral = 1,
        [Description("Razer/Corsair Mousepad + Mouse")]
        Generic_Mousepad = 2,

        //Logitech range is 100-199
        [Description("Logitech - G900")]
        Logitech_G900 = 100,
        [Description("Logitech - G502")]
        Logitech_G502 = 101,

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
        Clevo_Touchpad = 400,

        //Cooler Master range is 500-599

        //Roccat range is 600-699

        //Steelseries range is 700-799
        [Description("SteelSeries - Rival 300")]
        SteelSeries_Rival_300 = 700,
        [Description("SteelSeries - Rival 300 HP OMEN Edition")]
        SteelSeries_Rival_300_HP_OMEN_Edition = 701,

        //Asus range is 900-999
        [Description("Asus - Pugio")]
        Asus_Pugio = 900
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

    public enum BitmapAccuracy
    {
        Best = 1,
        Great = 3,
        Good = 6,
        Okay = 9,
        Fine = 12
    }

    public class Configuration : Settings
    {
        //First Time Installs
        public bool redist_first_time;
        public bool logitech_first_time;
        public bool corsair_first_time;
        public bool razer_first_time;
        public bool steelseries_first_time;
        public bool dualshock_first_time;
        public bool roccat_first_time;

        //General Program Settings
        public bool allow_peripheral_devices;
        public bool allow_wrappers_in_background;
        public bool allow_all_logitech_bitmaps;

        private bool useVolumeAsBrightness = false;
        [JsonProperty(PropertyName = "use_volume_as_brightness")]
        public bool UseVolumeAsBrightness { get { return useVolumeAsBrightness; } set { useVolumeAsBrightness = value; InvokePropertyChanged(); } }

        private float globalBrightness = 1.0f;
        [JsonProperty(PropertyName = "global_brightness")]
        public float GlobalBrightness { get { return globalBrightness; } set { globalBrightness = value; InvokePropertyChanged(); } }

        private float keyboardBrightness = 1.0f;
        [JsonProperty(PropertyName = "keyboard_brightness_modifier")]
        public float KeyboardBrightness { get { return keyboardBrightness; } set { keyboardBrightness = value; InvokePropertyChanged(); } }

        private float peripheralBrightness = 1.0f;
        [JsonProperty(PropertyName = "peripheral_brightness_modifier")]
        public float PeripheralBrightness { get { return peripheralBrightness; } set { peripheralBrightness = value; InvokePropertyChanged(); } }

        private bool getDevReleases = false;
        public bool GetDevReleases { get { return getDevReleases; } set { getDevReleases = value; InvokePropertyChanged(); } }

        private bool highPriority = false;
        public bool HighPriority { get { return highPriority; } set { highPriority = value; InvokePropertyChanged(); } }

        private BitmapAccuracy bitmapAccuracy = BitmapAccuracy.Okay;
        public BitmapAccuracy BitmapAccuracy { get { return bitmapAccuracy; } set { bitmapAccuracy = value; InvokePropertyChanged(); } }

        public bool updates_check_on_start_up;
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
        public bool unified_hid_disabled = false;
        public HashSet<Type> devices_disabled;
        public bool OverlaysInPreview;

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

        public VariableRegistry VarRegistry;

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
            dualshock_first_time = true;

            //General Program Settings
            allow_peripheral_devices = true;
            UseVolumeAsBrightness = false;
            allow_wrappers_in_background = true;
            allow_all_logitech_bitmaps = true;
            GlobalBrightness = 1.0f;
            KeyboardBrightness = 1.0f;
            peripheralBrightness = 1.0f;
            updates_check_on_start_up = true;
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
            devices_disabled.Add(typeof(Devices.Dualshock.DualshockDevice));
            devices_disabled.Add(typeof(Devices.AtmoOrbDevice.AtmoOrbDevice));
            OverlaysInPreview = false;

            //Blackout and Night theme
            time_based_dimming_enabled = false;
            time_based_dimming_affect_games = false;
            time_based_dimming_start_hour = 21;
            time_based_dimming_start_minute = 0;
            time_based_dimming_end_hour = 8;
            time_based_dimming_end_minute = 0;

            nighttime_enabled = false;
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

            Configuration config = JsonConvert.DeserializeObject<Configuration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All, SerializationBinder = Aurora.Utils.JSONUtils.SerializationBinder, Error = DeserializeErrorHandler });

            if (!config.unified_hid_disabled)
            {
                config.devices_disabled.Add(typeof(Devices.UnifiedHID.UnifiedHIDDevice));
                config.unified_hid_disabled = true;
            }

            return config;
        }

        private static void DeserializeErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            if (e.ErrorContext.Error.Message.Contains("Aurora.Devices.SteelSeriesHID.SteelSeriesHIDDevice") && e.CurrentObject is HashSet<Type> dd)
            {
                dd.Add(typeof(Aurora.Devices.UnifiedHID.UnifiedHIDDevice));
                e.ErrorContext.Handled = true;
            }
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
