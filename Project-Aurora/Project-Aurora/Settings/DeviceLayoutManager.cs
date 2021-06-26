using Aurora.Devices;
using Aurora.Settings.DeviceLayoutViewer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Aurora.Settings
{
    public enum KeyboardPhysicalLayout
    {
        [Description("ANSI")]
        ANSI,
        [Description("ISO")]
        ISO,
        [Description("ABNT")]
        ABNT,
        [Description("JIS")]
        JIS
    }
    public class VirtualKeyboardConfiguration
    {
        public bool IsNewFormat = false;

        public int[] keys_to_remove = new int[] { };

        public Dictionary<int, KeyboardKey> key_modifications = new Dictionary<int, KeyboardKey>();

        [JsonProperty("key_conversion")]
        public Dictionary<int, int> KeyConversion = null;

        /// <summary>
        /// A list of paths for each included group json
        /// </summary>
        public string[] included_features = new string[] { };

    }
    public class KeyboardLayout
    {
        [JsonProperty("key_conversion")]
        public Dictionary<Devices.DeviceKeys, Devices.DeviceKeys> KeyConversion = null;

        [JsonProperty("keys")]
        public KeyboardKey[] Keys = null;
    }

    public class DeviceLayout
    {

        public Dictionary<int, DeviceKeyConfiguration> Keys = new Dictionary<int, DeviceKeyConfiguration>();

        private System.Drawing.Rectangle Region = new System.Drawing.Rectangle(0, 0, 0, 0);

        private DeviceConfig Config;

        public DeviceLayout(DeviceConfig config)
        {
            Config = config;

        }
        protected class NewKeyboardLayout
        {
            /*[JsonProperty("layout_width")]
            public int Width = 0;

            [JsonProperty("layout_height")]
            public int Height = 0;*/

            [JsonProperty("keys")]
            public DeviceKeyConfiguration[] Keys = null;
        }
        private DeviceConfig convertDeviceConfig(DeviceConfig config)
        {
            switch (config.Type)
            {
                case 0:
                    return new KeyboardConfig(config);
                case 1:
                    return new MouseConfig(config);
                default:
                    return config;
            }
        }
        public List<DeviceKeyConfiguration> LoadLayout()
        {
                var layoutConfigPath = "";
                string keyboard_preference = Config.SelectedLayout;
                Config = convertDeviceConfig(Config);
                if (keyboard_preference != "" && keyboard_preference != "None")
                {
                    layoutConfigPath = Config.LayoutPath;
                }

                if (!String.IsNullOrWhiteSpace(layoutConfigPath) && File.Exists(layoutConfigPath))
                {
                    //Load keyboard layout
                    //LoadCulture();

                    //TODO
                    //if (!File.Exists(layoutPath))
                    //    LoadDefault();



               /* var layoutType = "abnt2";
                var fileName = "Plain Keyboard\\layout." + layoutType + ".json";
                var layoutPath = Path.Combine(layoutsPath, "Keyboard", fileName);
                string keyboardContent = File.ReadAllText(layoutPath, Encoding.UTF8);
                KeyboardLayout keyboard = JsonConvert.DeserializeObject<KeyboardLayout>(keyboardContent, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                LoadFromKeys(keyboard.Keys);
               
                NewKeyboardLayout saved = new NewKeyboardLayout();
                saved.Keys = Keys.Values.ToArray();
                keyboardContent = JsonConvert.SerializeObject(saved, Formatting.Indented);

                    fileName = "Plain Keyboard\\" + layoutType + "_layout.json";
                    File.WriteAllText(Path.Combine(layoutsPath, "Keyboard", fileName), keyboardContent, Encoding.UTF8);*/

                    if (Config.Type == 0)
                    {
                        var keyboardConfig = new KeyboardConfig(Config);
                        string content = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                        VirtualKeyboardConfiguration layoutConfig = JsonConvert.DeserializeObject<VirtualKeyboardConfiguration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                        if (layoutConfig.IsNewFormat == true)
                        {
                            var keys = LoadKeyboardPhysicalLayout(keyboardConfig.PhysicalLayoutPath);

                            var keyboardLayout = LoadDeviceLayout(keyboardConfig.LayoutPath);
                            keyboardLayout.ApplyConfig(keys);

                            if (keyboardConfig.SelectedKeyboardLayout == KeyboardPhysicalLayout.JIS)
                            {
                                foreach (var key in keyboardLayout.jis_key_modifications)
                                {
                                    if (keys.ContainsKey(key.Key))
                                    {
                                        keys[key.Key].ApplyModifier(key.Value);
                                    }
                                }
                            }

                            NormalizeKeys(keys);
                            Keys = keys;
                        }
                        else
                        {

                            Keys = LoadKeyboardPhysicalLayout(keyboardConfig.PhysicalLayoutPath);

                            //AdjustKeys
                            var adjustKeys = layoutConfig.key_modifications;
                            Keys.Values.ToList().FindAll(key => adjustKeys.ContainsKey((int)key.Tag)).ForEach(k => k.UpdateFromOtherKey(adjustKeys[k.Tag]));
                            foreach (var keyTag in layoutConfig.keys_to_remove)
                            {
                                Keys.Remove(keyTag);
                            }


                            NormalizeKeys(Keys);
                            
                            //Old way of serializing the deviceLayouts
                            foreach (string feature in layoutConfig.included_features)
                            {
                                string feature_path = Path.Combine(Global.ExecutingDirectory, "DeviceLayouts", "Keyboard", "Extra Features", feature);

                                if (File.Exists(feature_path))
                                {
                                    string feature_content = File.ReadAllText(feature_path, Encoding.UTF8);
                                    VirtualGroup feature_config = JsonConvert.DeserializeObject<VirtualGroup>(feature_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                                    AddFeature(feature_config.grouped_keys.ToArray(), feature_config.origin_region);

                                }
                            }

                            NormalizeKeys(Keys);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(layoutConfigPath) && File.Exists(layoutConfigPath))
                        {
                            string mouseConfigContent = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                            VirtualGroup mouseConfig = JsonConvert.DeserializeObject<VirtualGroup>(mouseConfigContent, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                            if (mouseConfig.origin_region == null)
                            {
                                Dictionary<int, DeviceKeyConfiguration> keys = new Dictionary<int, DeviceKeyConfiguration>();
                                var deviceConfig = LoadDeviceLayout(Config.LayoutPath);
                                deviceConfig.ApplyConfig(keys);
                                Keys = keys;
                            }
                            AddFeature(mouseConfig.grouped_keys.ToArray(), mouseConfig.origin_region);
                            NormalizeKeys(Keys);
                        }
                    }


                }

            return Keys.Values.ToList();
        }

        private Dictionary<int, DeviceKeyConfiguration> LoadKeyboardPhysicalLayout(string physicalLayoutPath)
        {
            Dictionary<int, DeviceKeyConfiguration> keys = new Dictionary<int, DeviceKeyConfiguration>();
            if (File.Exists(physicalLayoutPath))
            {
                string c = File.ReadAllText(physicalLayoutPath, Encoding.UTF8);
                NewKeyboardLayout keyboard = JsonConvert.DeserializeObject<NewKeyboardLayout>(c, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                keys = keyboard.Keys.ToDictionary(k => k.Tag, k => k);
            }
            return keys;
        }
        private KeycapGroupConfiguration LoadDeviceLayout(string layoutConfigPath)
        {
            if (File.Exists(layoutConfigPath))
            {
                string content = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                var layoutConfig = JsonConvert.DeserializeObject<KeycapGroupConfiguration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                if (layoutConfig.IsNewFormat == true)
                    return layoutConfig;
            }
            return new KeycapGroupConfiguration();
        }

        private KeycapGroupConfiguration CalcKeyboardKeyConfiguration(List<DeviceKeyConfiguration> saveKeys)
        {

            KeycapGroupConfiguration config = LoadDeviceLayout(Config.LayoutPath);
            var keyboardConfig = Config as KeyboardConfig;
            var defaultLayout = LoadKeyboardPhysicalLayout(keyboardConfig.PhysicalLayoutPath);

            if (keyboardConfig.SelectedKeyboardLayout != KeyboardPhysicalLayout.JIS)
            {
                config.UpdateConfig(saveKeys, defaultLayout);
            }
            else
            {
                config.ApplyConfig(defaultLayout);
                config.jis_key_modifications.Clear();
                foreach (var key in saveKeys)
                {
                    if (defaultLayout[key.Tag] != key)
                    {
                        config.jis_key_modifications[key.Tag] = new DeviceKeyModifier(defaultLayout[key.Tag], key);
                    }
                }

            }

            return config;
        }
        public void SaveLayout(List<Control_Keycap> layoutKey, Point offset)
        {
            List<DeviceKeyConfiguration> saveKeys = new List<DeviceKeyConfiguration>();
            foreach (var key in layoutKey)
            {
                var conf = key.Config;
                conf.Key.DeviceId = null;
                conf.X += (int)offset.X;
                conf.Y += (int)offset.Y;
                saveKeys.Add(conf);
            }
            KeycapGroupConfiguration config = new KeycapGroupConfiguration();
            if (Config.Type == 0)
            {
                config = CalcKeyboardKeyConfiguration(saveKeys);
            }
            else
            {
                config.key_to_add = saveKeys.ToDictionary(k => k.Tag, k => k);
            }

            var content = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Config.CustomLayoutPath, content, Encoding.UTF8);
        }
        public void AddFeature(KeyboardKey[] keys, KeyboardRegion? insertion_region = KeyboardRegion.TopLeft)
        {
            double location_x = 0.0D;
            double location_y = 0.0D;

            if (insertion_region == KeyboardRegion.TopRight)
            {
                location_x = Region.Width;
            }
            else if (insertion_region == KeyboardRegion.BottomLeft)
            {
                location_y = Region.Height + 7;
            }
            else if (insertion_region == KeyboardRegion.BottomRight)
            {
                location_x = Region.Width;
                location_y = Region.Height + 7;

            }

            int added_width = 0;
            int added_height = 0;

            foreach (var key in keys)
            {
                key.margin_left += location_x;
                key.margin_top += location_y;

                Keys[(int)key.tag] = new DeviceKeyConfiguration(key, Config.Id.ViewPort);

                if (key.width + key.margin_left > Region.Width)
                    Region.Width = (int)(key.width + key.margin_left);
                else if (key.margin_left + added_width < 0)
                {
                    added_width = -(int)(key.margin_left);
                }

                if (key.height + key.margin_top > Region.Height)
                    Region.Height = (int)(key.height + key.margin_top);
                else if (key.margin_top + added_height < 0)
                {
                    added_height = -(int)(key.margin_top);
                }

            }
            Region.Width += added_width;
            Region.Height += added_height;
            //NormalizeKeys();
        }
        protected void NormalizeKeys(Dictionary<int, DeviceKeyConfiguration> keys)
        {
            int x_correction = 0;
            int y_correction = 0;
            int layout_height = 0;
            int layout_width = 0;

            foreach (var key in keys.Values)
            {
                if (key.X < x_correction)
                    x_correction = key.X;

                if (key.Y < y_correction)
                    y_correction = key.Y;
            }
            foreach (var key in keys.Values)
            {
                key.Y -= y_correction;
                key.X -= x_correction;

                if (key.Width + key.X > layout_width)
                    layout_width = key.Width + key.X;

                if (key.Height + key.Y > layout_height)
                    layout_height = key.Height + key.Y;
            }
            Region.Width = layout_width;
            Region.Height = layout_height;
        }
        private void LoadFromKeys(KeyboardKey[] JsonKeys)
        {
            double layout_height = 0;
            double layout_width = 0;
            double current_height = 0;
            double current_width = 0;

            foreach (var key in JsonKeys)
            {

                if (key.width + key.margin_left > 0)
                    current_width += key.width.Value + key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += key.margin_top.Value;

                key.margin_left = current_width - key.width.Value;
                key.margin_top = current_height + key.margin_top.Value;

                if (layout_width < current_width)
                    layout_width = current_width;

                if (key.line_break ?? false)
                {
                    current_height += 37;
                    current_width = 0;
                }

                if (layout_height < current_height)
                    layout_height = current_height;

                Keys.Add((int)key.tag ,new DeviceKeyConfiguration(key, null));
            }

            Region.Width = (int)layout_width;
            Region.Height = (int)layout_height;
        }
    }
    public class KeycapGroupConfiguration
    {
        public bool IsNewFormat = true;
        public int[] keys_to_remove = new int[] { };

        [JsonProperty("key_modifications")]
        public Dictionary<int, DeviceKeyModifier> key_modifications = new Dictionary<int, DeviceKeyModifier>();

        [JsonProperty("key_to_add")]
        public Dictionary<int, DeviceKeyConfiguration> key_to_add = new Dictionary<int, DeviceKeyConfiguration>();

        [JsonProperty("jis_key_modifications")]
        public Dictionary<int, DeviceKeyModifier> jis_key_modifications = new Dictionary<int, DeviceKeyModifier>();

        public KeycapGroupConfiguration()
        {

        }
        public void UpdateConfig(List<DeviceKeyConfiguration> saveKeys, Dictionary<int, DeviceKeyConfiguration> baseKeys)
        {
            key_modifications.Clear();
            key_to_add.Clear();
            foreach (var key in saveKeys)
            {
                if (baseKeys.ContainsKey(key.Tag))
                {
                    if (baseKeys[key.Tag] != key)
                    {
                        key_modifications[key.Tag] = new DeviceKeyModifier(baseKeys[key.Tag], key);
                    }
                    baseKeys.Remove(key.Tag);
                }
                else
                {
                    key_to_add[key.Tag] = key;
                }
            }
            keys_to_remove = baseKeys.Keys.ToArray();
        }
        public void ApplyConfig(Dictionary<int, DeviceKeyConfiguration> keys)
        {
            //AdjustKeys
            foreach (var keyTag in keys_to_remove)
            {
                keys.Remove(keyTag);
            }
            foreach (var key in key_modifications)
            {
                if (keys.ContainsKey(key.Key))
                {
                    keys[key.Key].ApplyModifier(key.Value);
                }
            }
            foreach (var key in key_to_add)
            {
                keys[key.Key] = key.Value;
            } 
        }
    }

    public class DeviceConfig
    {
        public Devices.UniqueDeviceId Id;
        public string SelectedLayout = "";
        public int Type;
        public Point Offset = new Point(0, 0);
        public bool LightingEnabled = true;
        public bool TypeChangeEnabled = true;
        public bool InvisibleBackgroundEnabled = false;

        [JsonIgnore]
        public virtual string LayoutTypeDir => "OtherDevices";

        public DeviceConfig(DeviceConfig config)
        {
            Id = config.Id;
            SelectedLayout = config.SelectedLayout;
            Type = config.Type;
            ConfigurationChanged = config.ConfigurationChanged;
            LightingEnabled = config.LightingEnabled;
            TypeChangeEnabled = config.TypeChangeEnabled;
            InvisibleBackgroundEnabled = config.InvisibleBackgroundEnabled;
        }

        public DeviceConfig(int viewPort)
        {
            Id = new Devices.UniqueDeviceId();
            Id.ViewPort = viewPort;
        }
        protected DeviceConfig()
        {
            // Private parameterless constructor. Leave private so that it forces everyone to use the Mandate parameter but allows serialization to work. 
        }

        public delegate void ConfigChangedEventHandler();

        public event ConfigChangedEventHandler ConfigurationChanged;

        /*public void UpdateConfig(DeviceConfig config)
        {
            Id = config.Id;
            SelectedLayout = config.SelectedLayout;
            Type = config.Type;
            SaveConfiguration?.Invoke(this);
            ConfigurationChanged?.Invoke();
        }*/
        public void RefreshConfig()
        {
            ConfigurationChanged?.Invoke();
        }

        [JsonIgnore]
        public string LayoutPath => Global.devicesLayout.calcLayoutPath(LayoutTypeDir, SelectedLayout + ".json");

        [JsonIgnore]
        public string CustomLayoutPath => Path.Combine(Global.devicesLayout.customLayoutsPath, LayoutTypeDir, SelectedLayout + ".json");

    }
    public class MouseConfig : DeviceConfig
    {
        public override string LayoutTypeDir => "Mouse";

        public MouseConfig(DeviceConfig config) : base(config)
        {
            Type = 1;
        }
    }

    public class KeyboardConfig : DeviceConfig
    {
        public KeyboardPhysicalLayout SelectedKeyboardLayout = KeyboardPhysicalLayout.ANSI;

        public override string LayoutTypeDir => "Keyboard";
        public KeyboardConfig(DeviceConfig config) : base(config)
        {
            if (config is KeyboardConfig keyboardConfig)
                SelectedKeyboardLayout = keyboardConfig.SelectedKeyboardLayout;
            else
                SelectedKeyboardLayout = GetSystemKeyboardCulture();
            Type = 0;
        }

        public KeyboardConfig(int viewPort) : base(viewPort)
        {
            Type = 0;
            SelectedKeyboardLayout = GetSystemKeyboardCulture();
        }
        private KeyboardConfig()
        {

        }
        private string ConvertEnumToFileName()
        {
            switch (SelectedKeyboardLayout)
            {
                case KeyboardPhysicalLayout.ANSI:
                    return "ansi_layout";
                case KeyboardPhysicalLayout.ISO:
                    return "iso_layout";
                case KeyboardPhysicalLayout.ABNT:
                    return "abnt2_layout";
                case KeyboardPhysicalLayout.JIS:
                    return "jpn_layout";
                default:
                    return "";
            }
        }
        public string PhysicalLayoutPath => Global.devicesLayout.calcLayoutPath("Keyboard\\Plain Keyboard", ConvertEnumToFileName() + ".json");
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);
        [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint thread);
        private CultureInfo GetCurrentKeyboardLayout()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                uint foregroundProcess = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                int keyboardLayout = GetKeyboardLayout(foregroundProcess).ToInt32() & 0xFFFF;
                return new CultureInfo(keyboardLayout);
            }
            catch (Exception _)
            {
                return new CultureInfo(1033); // Assume English if something went wrong.
            }
        }
        private KeyboardPhysicalLayout GetSystemKeyboardCulture()
        {
            string culture = GetCurrentKeyboardLayout().Name;
            switch (culture)
            {
                case ("tr-TR"):
                    return KeyboardPhysicalLayout.ISO;
                case ("ja-JP"):
                    return KeyboardPhysicalLayout.JIS;
                case ("de-DE"):
                case ("hsb-DE"):
                case ("dsb-DE"):
                case ("fr-CH"):
                case ("de-CH"):
                case ("fr-FR"):
                case ("br-FR"):
                case ("oc-FR"):
                case ("co-FR"):
                case ("gsw-FR"):
                case ("cy-GB"):
                case ("gd-GB"):
                case ("en-GB"):
                case ("da-DK"):
                case ("se-SE"):
                case ("nb-NO"):
                case ("nn-NO"):
                case ("nordic"):
                    return KeyboardPhysicalLayout.ISO;
                case ("ru-RU"):
                case ("tt-RU"):
                case ("ba-RU"):
                case ("sah-RU"):
                case ("en-US"):
                case ("pt-BR"):
                case ("dvorak"):
                    return KeyboardPhysicalLayout.ANSI;
                case ("dvorak_int"):
                case ("hu-HU"):
                case ("it-IT"):
                case ("es-AR"):
                case ("es-BO"):
                case ("es-CL"):
                case ("es-CO"):
                case ("es-CR"):
                case ("es-EC"):
                case ("es-MX"):
                case ("es-PA"):
                case ("es-PY"):
                case ("es-PE"):
                case ("es-UY"):
                case ("es-VE"):
                case ("es-419"):
                case ("es-ES"):
                case ("iso"):
                    return KeyboardPhysicalLayout.ISO;
                case ("ansi"):
                    return KeyboardPhysicalLayout.ANSI;
                default:
                    return KeyboardPhysicalLayout.ISO;

            }
        }

    }



    public class DeviceLayoutManager
    {
        public Dictionary<int, DeviceConfig> DevicesConfig = new Dictionary<int, DeviceConfig>();

        public double Height = 0;

        public double Width = 0;

        public delegate void ConfigChangedEventHandler(DeviceConfig changedConf);

        public event ConfigChangedEventHandler DevicesConfigChanged;
        [JsonIgnore]
        protected string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");
        [JsonIgnore]
        public string customLayoutsPath = System.IO.Path.Combine(Global.AppDataDirectory, "DeviceLayouts");

        private string[] deviceDirs = {"Images","Keyboard", "Mouse", "OtherDevices"};

        public string calcLayoutPath(string LayoutTypeDir, string file)
        {
            if (File.Exists(Path.Combine(Global.devicesLayout.customLayoutsPath, LayoutTypeDir, file)))
            {
                return Path.Combine(Global.devicesLayout.customLayoutsPath, LayoutTypeDir, file);
            }
            return Path.Combine(layoutsPath, LayoutTypeDir, file);
        }

        public DeviceLayoutManager()
        {
            foreach (var item in deviceDirs)
            {
                var dict = new DirectoryInfo(Path.Combine(customLayoutsPath, item));
                if (!dict.Exists)
                    dict.Create();
            }
        }
        private int FindUnusedViewPort(int viewPort = 0)
        {
            if (DevicesConfig.Where(c => c.Value.Id.ViewPort == viewPort).Any())
            {
                return FindUnusedViewPort(viewPort + 1);
            }
            return viewPort;
        }
        public void AddNewDeviceLayout()
        {
            int viewPort = FindUnusedViewPort();
            var devConf = new DeviceConfig(viewPort);
            if (devConf.Id.ViewPort == 0)
            {
                devConf.Type = 0;
                devConf.TypeChangeEnabled = false;
            }
            DevicesConfig[viewPort] = devConf;
            DevicesConfigChanged.Invoke(devConf);
            Save();
        }
        public void RemoveDeviceLayout(DeviceConfig conf)
        {
            DevicesConfig.Remove((int)conf.Id.ViewPort);
            DevicesConfigChanged.Invoke(conf);
            Save();
        }
       
        public void Load()
        {
            var fileName = "DevicesConfig.json";
            var layoutConfigPath = Path.Combine(Global.AppDataDirectory, fileName);
            if (File.Exists(layoutConfigPath))
            {
                string devicesConfigContent = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                DeviceLayoutManager manager = JsonConvert.DeserializeObject<DeviceLayoutManager>(devicesConfigContent, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                DevicesConfig = manager?.DevicesConfig ?? new Dictionary<int, DeviceConfig>();

                foreach ( var conf in DevicesConfig)
                {
                    //conf.Value.Id.ViewPort = conf.Key;
                    Global.dev_manager.RegisterViewPort(ref conf.Value.Id, conf.Key);
                    DevicesConfigChanged.Invoke(conf.Value);
                }
                
            }
        }
        private void Save()
        {
            var fileName = "DevicesConfig.json";
            var layoutConfigPath = Path.Combine(Global.AppDataDirectory, fileName);
            var content = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(layoutConfigPath, content, Encoding.UTF8);
        }
        public void LayoutPositionChanged(DeviceConfig config)
        {
            double baseline_x = double.MaxValue;
            double baseline_y = double.MaxValue;
            foreach (DeviceConfig dc in DevicesConfig.Values)
            {
                if (dc.Offset.X < baseline_x)
                    baseline_x = dc.Offset.X;

                if (dc.Offset.Y < baseline_y)
                    baseline_y = dc.Offset.Y;
            }
            foreach (DeviceConfig dc in DevicesConfig.Values)
            {
                dc.Offset = new Point((int)(dc.Offset.X - baseline_x), (int)(dc.Offset.Y - baseline_y));
            }

            DevicesConfigChanged.Invoke(config);
            Save();
        }

        public void SaveConfiguration(DeviceConfig config)
        {

            //if (DevicesConfig.SelectMany(dc => dc.Id ))
            if (config.Id == null)
                
                return;
            DevicesConfig[(int)config.Id.ViewPort] = config;
            //DeviceLayouts.Where(dl => dl.DeviceConfig.Id.ViewPort == config.Id.ViewPort).FirstOrDefault().DeviceConfig = config;
            
            double baseline_x = double.MaxValue;
            double baseline_y = double.MaxValue;
            foreach (DeviceConfig dc in DevicesConfig.Values)
            {
                if (dc.Offset.X < baseline_x)
                    baseline_x = dc.Offset.X;

                if (dc.Offset.Y < baseline_y)
                    baseline_y = dc.Offset.Y;
            }
            foreach (DeviceConfig dc in DevicesConfig.Values)
            {
                dc.Offset = new Point((int)(dc.Offset.X - baseline_x), (int)(dc.Offset.Y - baseline_y));
            }

            DevicesConfigChanged.Invoke(config);
            Save();
        }
        public List<string> GetLayoutsForType(AuroraDeviceType type)
        {
            string customLayouts = Path.Combine(customLayoutsPath, deviceDirs[(int)type+1]);
            List<string> FilesName = new List<string>() { "None" };
            if (Directory.Exists(customLayouts))
            {
                foreach (var name in Directory.GetFiles(customLayouts))
                {
                    FilesName.Add(name.Split('\\').Last().Split('.').First());
                }
            }
            else
            {
                Directory.CreateDirectory(customLayouts);
            }
            string layouts = Path.Combine(layoutsPath, deviceDirs[(int)type + 1]);
            if (Directory.Exists(layouts))
            {
                foreach (var name in Directory.GetFiles(layouts))
                {
                    string lname = name.Split('\\').Last().Split('.').First();
                    if (!FilesName.Where(n => n.Equals(lname)).Any())
                        FilesName.Add(lname);
                }
            }
            return FilesName;
        }
    }
}
