using Aurora.Settings.DeviceLayoutViewer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        //[Description("ANSI")]
        ANSI,
        ISO,
        ABNT,
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

        protected string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        private System.Drawing.Rectangle Region = new System.Drawing.Rectangle(0, 0, 0, 0);

        private DeviceConfig Config;

        public DeviceLayout(DeviceConfig config)
        {
            Config = config;

        }
        private string GetFolder()
        {
            switch (Config.Type)
            {
                case 0:
                    return "Keyboard";
                default:
                    return "Mouse";
            }
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
        public List<DeviceKeyConfiguration> LoadLayout()
        {
                var layoutConfigPath = "";
                string keyboard_preference = Config.SelectedLayout;
                if (keyboard_preference != "" && keyboard_preference != "None")
                {
                    layoutConfigPath = Path.Combine(layoutsPath, GetFolder(), keyboard_preference + ".json");
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

                            foreach (string feature in layoutConfig.included_features)
                            {
                                string feature_path = Path.Combine(layoutsPath, GetFolder(), "Extra Features", feature);

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
                var conf = key.GetConfiguration();
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

            File.WriteAllText(Path.Combine(layoutsPath, GetFolder(), Config.SelectedLayout + ".json"), content, Encoding.UTF8);
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

                Keys[(int)key.tag] = new DeviceKeyConfiguration(key, Config.Id);

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
        public int Id;
        public string SelectedLayout = "";
        public int Type;
        public Point Offset = new Point(0, 0);
        public bool LightingEnabled = true;

        [JsonIgnore]
        protected string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        public DeviceConfig(DeviceConfig config)
        {
            Id = config.Id;
            SelectedLayout = config.SelectedLayout;
            Type = config.Type;
            ConfigurationChanged = config.ConfigurationChanged;
        }

        public DeviceConfig()
        {
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

        public virtual string LayoutPath => Path.Combine(layoutsPath, "Mouse", SelectedLayout + ".json");

    }

    public class KeyboardConfig : DeviceConfig
    {
        public KeyboardPhysicalLayout SelectedKeyboardLayout = KeyboardPhysicalLayout.ANSI;

        public KeyboardConfig(DeviceConfig config) : base(config)
        {
            if(config is KeyboardConfig keyboardConfig)
                SelectedKeyboardLayout = keyboardConfig.SelectedKeyboardLayout;
            Type = 0;
        }

        public KeyboardConfig()
        {
            Type = 0;
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
        public string PhysicalLayoutPath => Path.Combine(layoutsPath, "Keyboard\\Plain Keyboard\\" + ConvertEnumToFileName() + ".json");
        public override string LayoutPath => Path.Combine(layoutsPath, "Keyboard", SelectedLayout + ".json");

    }



    public class DeviceLayoutManager
    {
        public Dictionary<int, DeviceConfig> DevicesConfig = new Dictionary<int, DeviceConfig>();

        public delegate void LayoutUpdatedEventHandler(object sender);

        public event LayoutUpdatedEventHandler DeviceLayoutNumberChanged;

        public double Height = 0;

        public double Width = 0;

        public DeviceLayoutManager()
        {
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
            }
            /*DevicesConfig.Add(new DeviceConfig());
            DevicesConfig[0].Id = 0;
            DevicesConfig[0].Type = 0;
            DevicesConfig[0].SelectedKeyboardLayout = "ansi";
            DevicesConfig[0].SelectedLayout = "asus_strix_flare";
            DevicesConfig.Add(new DeviceConfig());
            DevicesConfig[1].Id = 1;
            DevicesConfig[1].Type = 1;
            DevicesConfig[1].SelectedLayout = "Corsair - Katar";
            Save();*/
            /*DevicesConfig.Add(new DeviceConfig());
            DevicesConfig[0].Id = 0;
            DevicesConfig[0].Type = 0;
            DevicesConfig[0].SelectedKeyboardLayout = "ansi_layout";
            DevicesConfig[0].SelectedLayout = "asus_strix_flare";
            DevicesConfig[2].Id = 2;
            DevicesConfig[2].Type = 0;*/
            //DevicesLayout[1].Region.X = 825;

            foreach (var layout in DevicesConfig.Values)
            {
                /*
                if (layout.Offset.X + layout.Region.Width > current_width)
                    current_width = layout.Region.X + layout.Region.Width;
                else if (layout.Region.X < baseline_x)
                    baseline_x = layout.Region.X;

                if (layout.Region.Y + layout.Region.Height > current_height)
                    current_height = layout.Region.Y + layout.Region.Height;
                else if (layout.Region.Y < baseline_y)
                    baseline_y = layout.Region.Y;*/
            }

            DeviceLayoutNumberChanged?.Invoke(this);
        }
        public void SaveConfiguration(DeviceConfig config)
        {

            //if (DevicesConfig.SelectMany(dc => dc.Id ))
            DevicesConfig[config.Id] = config;
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

            var fileName = "DevicesConfig.json";
            var layoutConfigPath = Path.Combine(Global.AppDataDirectory, fileName);
            var content = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(layoutConfigPath, content, Encoding.UTF8);
            DeviceLayoutNumberChanged?.Invoke(this);
        }
        public void DeleteConfiguration(DeviceConfig config)
        {
            DevicesConfig.Remove(config.Id);

            var fileName = "DevicesConfig.json";
            var layoutConfigPath = Path.Combine(Global.AppDataDirectory, fileName);
            var content = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(layoutConfigPath, content, Encoding.UTF8);
            DeviceLayoutNumberChanged?.Invoke(this);
        }
        public void AddNewDevice()
        {
            var config = new DeviceConfig();
            config.Id = DevicesConfig.Count;
            DevicesConfig[config.Id] = config;
            DeviceLayoutNumberChanged?.Invoke(this);
        }
        private void Layout_DeviceLayoutUpdated(object sender)
        {
            /*var layout = (sender as Control_DeviceLayout);
            DeviceConfig config = DevicesConfig.Where(c => c == layout.DeviceConfig).First();

            var offset = layout.TranslatePoint(new System.Windows.Point(0, 0), (UIElement)VisualTreeHelper.GetParent(layout));


            Save();*/
        }
        public List<Control_DeviceLayout> GetDeviceLayouts(bool abstractKeycaps = false)
        {
            List<Control_DeviceLayout> deviceLayouts = new List<Control_DeviceLayout>();
            foreach (var item in DevicesConfig.Values)
            {
                /*Grid deviceControl = item.CreateUserControl();
                deviceControl.VerticalAlignment = VerticalAlignment.Top;
                deviceControl.HorizontalAlignment = HorizontalAlignment.Left;*/
                //item.Margin = new Thickness(item.Region.Left, item.Region.Top, 0, 0);
                var layout = new Control_DeviceLayout(item);
                //layout.DeviceLayoutUpdated += Layout_DeviceLayoutUpdated;
                deviceLayouts.Add(layout);


            }

            return deviceLayouts;


        }
    }
}
