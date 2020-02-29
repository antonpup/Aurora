using Aurora.Settings.Keycaps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public class DeviceKey
    {
        [JsonProperty("visual_name")]
        public string VisualName { get; set; }
        [JsonProperty("tag")]
        public int Tag { get; set; }
        [JsonProperty("device_id")]
        public int? DeviceId { get; set; }

        /*public bool Equals(DeviceKey otherKey)
        {
            return Tag == otherKey.Tag && DeviceId == otherKey.DeviceId;
        }

        public override bool Equals(object obj)
        {
            // Again just optimization
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            // Actually check the type, should not throw exception from Equals override
            if (obj.GetType() != this.GetType()) return false;

            // Call the implementation from IEquatable
            return Equals((DeviceKey)obj);
        }*/

        public bool Equals(DeviceKey key1, DeviceKey key2)
        {
            return key1.Tag == key2.Tag && key1.DeviceId == key2.DeviceId;
        }
    

        public int GetHashCode(DeviceKey obj)
        {
            return obj.Tag;
        }
        public class EqualityComparer : IEqualityComparer<DeviceKey>
        {
            public bool Equals(DeviceKey key1, DeviceKey key2)
            {
                return key1.Tag == key2.Tag && (key2.DeviceId == null || key1.DeviceId == key2.DeviceId);
            }


            public int GetHashCode(DeviceKey obj)
            {
                return obj.Tag;
            }

        }
        public static bool operator==(DeviceKey key1, DeviceKey key2)
        {
            return key1.Tag == key2.Tag && (key2.DeviceId == null || key1.DeviceId == key2.DeviceId);
        }
        public static bool operator !=(DeviceKey key1, DeviceKey key2)
        {
            return !(key1.Tag == key2.Tag && (key2.DeviceId == null || key1.DeviceId == key2.DeviceId));
        }
        public DeviceKey()
        {
            Tag = -1;
            DeviceId = null;
        }
        public DeviceKey(Devices.DeviceKeys key, int? deviceId = null, string visualName = null)
        {
            Tag = (int)key;
            DeviceId = deviceId;
            VisualName = visualName;
        }
        public static implicit operator DeviceKey(Devices.DeviceKeys k) => new DeviceKey(k);

    }
    public class DeviceKeyConfiguration
    {
        public DeviceKey Key = null;
        public System.Drawing.Rectangle Region = new System.Drawing.Rectangle(0, 0, 0, 0);
        public string Image = "";
        public double? FontSize;
        public bool? Enabled = true;

        [JsonIgnore]
        public int Tag => Key.Tag;
        public DeviceKeyConfiguration()
        {
        }
        public DeviceKeyConfiguration(KeyboardKey key, int? deviceId)
        {
            Key = new DeviceKey(key.tag, deviceId, key.visualName);
            if (key.width != null) Region.Width = (int)key.width;
            if (key.height != null) Region.Height = (int)key.height;
            if (key.font_size != null) FontSize = key.font_size;
            if (key.margin_left != null) Region.X = (int)key.margin_left;
            if (key.margin_top != null) Region.Y = (int)key.margin_top;
            if (key.enabled != null) Enabled = key.enabled;
            if (key.image != null) Image = key.image;
        }
        public void UpdateFromOtherKey(KeyboardKey key)
        {
            if (key != null)
            {
                
                if (key.visualName != null) Key.VisualName = key.visualName;
                if ((int)key.tag != -1)
                    Key.Tag = (int)key.tag;
                if (key.width != null) Region.Width = (int)key.width;
                if (key.height != null) Region.Height = (int)key.height;
                if (key.font_size != null) FontSize = key.font_size;
                if (key.margin_left != null) Region.X = (int)key.margin_left;
                if (key.margin_top != null) Region.Y = (int)key.margin_top;
                if (key.enabled != null) Enabled = key.enabled;
                if (key.image != null) Image = key.image;
            }
        }
        public void UpdateFromOtherKey(DeviceKeyConfiguration key)
        {
            if (key.Key.VisualName != null) Key.VisualName = key.Key.VisualName;
            if ((int)key.Key.Tag != -1)
                Key.Tag = (int)key.Key.Tag;
            if (key.Region != null)
            {
                Region.Width = (int)key.Region.Width;
                Region.Height = (int)key.Region.Height;
                Region.X = (int)key.Region.X;
                Region.Y = (int)key.Region.Y;
            }
            if (key.FontSize != null) FontSize = key.FontSize;
            if (key.Enabled != null) Enabled = key.Enabled;
            if (key.Image != null) Image = key.Image;
        }
        public static bool operator ==(DeviceKeyConfiguration key1, DeviceKeyConfiguration key2)
        {
            const int epsilon = 3;
            return key1.Tag == key2.Tag && key1.Image == key2.Image && key1.Enabled == key2.Enabled && key1.FontSize == key2.FontSize && key1.Region.Width == key2.Region.Width && key1.Region.Height == key2.Region.Height &&
                key1.Region.X > key2.Region.X - epsilon && key1.Region.X < key2.Region.X + epsilon && key1.Region.Y > key2.Region.Y - epsilon && key1.Region.Y < key2.Region.Y + epsilon;
        }
        public static bool operator !=(DeviceKeyConfiguration key1, DeviceKeyConfiguration key2)
        {
            return !(key1 == key2);
        }
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
            [JsonProperty("layout_width")]
            public int Width = 0;

            [JsonProperty("layout_height")]
            public int Height = 0;

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
                var fileName = "Plain Keyboard\\" + Config.SelectedKeyboardLayout + ".json";
                var layoutPath = Path.Combine(layoutsPath, GetFolder(), fileName);
                if (File.Exists(layoutPath))
                {
                    string c = File.ReadAllText(layoutPath, Encoding.UTF8);
                    NewKeyboardLayout keyboard = JsonConvert.DeserializeObject<NewKeyboardLayout>(c, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                    Keys = keyboard.Keys.ToDictionary(k => k.Tag, k => k);
                    Keys.Values.ToList().ForEach(k => k.Key.DeviceId = Config.Id);

                    /*Region.Width = keyboard.Width;
                    Region.Height = keyboard.Height;*/
                }


                /*
                var fileName = "Plain Keyboard\\layout." + "ansi" + ".json";
                var layoutPath = Path.Combine(layoutsPath, "Keyboard", fileName);
                string content = File.ReadAllText(layoutPath, Encoding.UTF8);
                KeyboardLayout keyboard = JsonConvert.DeserializeObject<KeyboardLayout>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                LoadFromKeys(keyboard.Keys.ToList());
               
                NewKeyboardLayout saved = new NewKeyboardLayout();
                saved.KeyConversion = keyboard.KeyConversion;
                saved.Keys = Keys.ToArray();
                content = JsonConvert.SerializeObject(saved);

                fileName = "Plain Keyboard\\" + "ansi" + "_layout.json";
                File.WriteAllText(Path.Combine(layoutsPath, "Keyboard", fileName), content, Encoding.UTF8);*/

                try
                {
                    if (Config.Type == 0)
                    {
                        string content = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                        VirtualKeyboardConfiguration layoutConfig = JsonConvert.DeserializeObject<VirtualKeyboardConfiguration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                        if (layoutConfig.IsNewFormat == true)
                            throw new Exception();

                        //AdjustKeys
                        var adjustKeys = layoutConfig.key_modifications;
                        Keys.Values.ToList().FindAll(key => adjustKeys.ContainsKey((int)key.Tag)).ForEach(k => k.UpdateFromOtherKey(adjustKeys[k.Tag]));
                        foreach (var keyTag in layoutConfig.keys_to_remove)
                        {
                            Keys.Remove(keyTag);
                        }


                        NormalizeKeys();

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

                        NormalizeKeys();
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(layoutConfigPath) && File.Exists(layoutConfigPath))
                        {
                            string mouseConfigContent = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                            VirtualGroup mouseConfig = JsonConvert.DeserializeObject<VirtualGroup>(mouseConfigContent, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                            if (mouseConfig.origin_region == null)
                                throw new Exception();
                            AddFeature(mouseConfig.grouped_keys.ToArray(), mouseConfig.origin_region);
                            NormalizeKeys();
                        }
                    }
                }
                catch (Exception)
                {
                    string content = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                    KeycapGroupConfiguration layoutConfig = JsonConvert.DeserializeObject<KeycapGroupConfiguration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                    //AdjustKeys
                    foreach (var keyTag in layoutConfig.keys_to_remove)
                    {
                        Keys.Remove(keyTag);
                    }
                    foreach (var key in layoutConfig.key_modifications)
                    {
                        if (Keys.ContainsKey(key.Key))
                        {
                            Keys[key.Key].UpdateFromOtherKey(key.Value);
                        }
                        else
                        {
                            Keys[key.Key] = key.Value;
                        }
                    }

                    NormalizeKeys();
                }

            }

            return Keys.Values.ToList();
        }
        public void SaveLayout(List<Control_Keycap> layoutKey)
        {
            List<DeviceKeyConfiguration> saveKeys = new List<DeviceKeyConfiguration>();
            layoutKey.ForEach(key => saveKeys.Add(key.GetConfiguration()));

            KeycapGroupConfiguration config = new KeycapGroupConfiguration();
            Dictionary<int, DeviceKeyConfiguration> defaultLayout = new Dictionary<int, DeviceKeyConfiguration>();
            if (!String.IsNullOrWhiteSpace(Config.SelectedKeyboardLayout))
            {
                var fileName = "Plain Keyboard\\" + Config.SelectedKeyboardLayout + ".json";
                var layoutPath = Path.Combine(layoutsPath, GetFolder(), fileName);
                if (File.Exists(layoutPath))
                {
                    string c = File.ReadAllText(layoutPath, Encoding.UTF8);
                    NewKeyboardLayout keyboard = JsonConvert.DeserializeObject<NewKeyboardLayout>(c, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                    defaultLayout = keyboard.Keys.ToDictionary(k => k.Tag, k => k);
                    //defaultLayout.Values.ToList().ForEach(k => k.Key.DeviceId = DeviceId);
                }
            }
            foreach (var key in saveKeys)
            {
                if (defaultLayout.ContainsKey(key.Tag))
                {
                    if (defaultLayout[key.Tag] != key)
                    {
                        config.key_modifications[key.Tag] = key;
                    }
                    defaultLayout.Remove(key.Tag);
                }
                else
                {
                    config.key_modifications[key.Tag] = key;
                }
            }
            config.keys_to_remove = defaultLayout.Keys.ToArray();
            var content = JsonConvert.SerializeObject(config);

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
        protected void NormalizeKeys()
        {
            int x_correction = 0;
            int y_correction = 0;
            int layout_height = 0;
            int layout_width = 0;

            foreach (var key in Keys.Values)
            {
                if (key.Region.X < x_correction)
                    x_correction = key.Region.X;

                if (key.Region.Y < y_correction)
                    y_correction = key.Region.Y;
            }
            foreach (var key in Keys.Values)
            {
                key.Region.Y -= y_correction;
                key.Region.X -= x_correction;

                if (key.Region.Width + key.Region.X > layout_width)
                    layout_width = key.Region.Width + key.Region.X;

                if (key.Region.Height + key.Region.Y > layout_height)
                    layout_height = key.Region.Height + key.Region.Y;
            }
            Region.Width = layout_width;
            Region.Height = layout_height;
        }
    }
    public class KeycapGroupConfiguration
    {
        public bool IsNewFormat = true;
        public int[] keys_to_remove = new int[] { };

        [JsonProperty("key_modifications")]
        public Dictionary<int, DeviceKeyConfiguration> key_modifications = new Dictionary<int, DeviceKeyConfiguration>();

        [JsonProperty("key_to_add")]
        public Dictionary<int, DeviceKeyConfiguration> key_to_add = new Dictionary<int, DeviceKeyConfiguration>();

        public KeycapGroupConfiguration()
        {

        }
    }
    


    public class DeviceConfig
    {
        public int Id;
        public string SelectedLayout = "";
        public string SelectedKeyboardLayout = null;
        public int Type;
        public Point Offset = new Point(0, 0);
        public bool LightingEnabled = true;

        public DeviceConfig(DeviceConfig config)
        {
            Id = config.Id;
            SelectedLayout = config.SelectedLayout;
            SelectedKeyboardLayout = config.SelectedKeyboardLayout;
            Type = config.Type;
        }

        public DeviceConfig()
        {
        }

        public delegate void ConfigChangedEventHandler();
        public delegate void SaveConfigEventHandler(DeviceConfig sender);
        public delegate void DeleteConfigEventHandler(DeviceConfig sender);

        public event ConfigChangedEventHandler ConfigurationChanged;
        public event SaveConfigEventHandler SaveConfiguration;
        public event DeleteConfigEventHandler DeleteConfiguration;

        public void Save()
        {
            SaveConfiguration?.Invoke(this);
            ConfigurationChanged?.Invoke();
        }
        public void Delete()
        {
            DeleteConfiguration?.Invoke(this);
        }
    }



    public class DeviceLayoutManager
    {
        public List<DeviceConfig> DevicesConfig = new List<DeviceConfig>();

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

                DevicesConfig = manager?.DevicesConfig ?? new List<DeviceConfig>();
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

            foreach (var layout in DevicesConfig)
            {
                layout.SaveConfiguration += SaveConfiguration;
                layout.DeleteConfiguration += DeleteConfiguration;
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
        private void SaveConfiguration(DeviceConfig config)
        {
            //if (DevicesConfig.SelectMany(dc => dc.Id ))
            var fileName = "DevicesConfig.json";
            var layoutConfigPath = Path.Combine(Global.AppDataDirectory, fileName);
            var content = JsonConvert.SerializeObject(this);
            File.WriteAllText(layoutConfigPath, content, Encoding.UTF8);
        }
        private void DeleteConfiguration(DeviceConfig config)
        {
            DevicesConfig.Remove(config);

            var fileName = "DevicesConfig.json";
            var layoutConfigPath = Path.Combine(Global.AppDataDirectory, fileName);
            var content = JsonConvert.SerializeObject(this);
            File.WriteAllText(layoutConfigPath, content, Encoding.UTF8);
            DeviceLayoutNumberChanged?.Invoke(this);
        }
        public void AddNewDevice()
        {
            var config = new DeviceConfig();
            config.Id = DevicesConfig.Count;
            config.SaveConfiguration += SaveConfiguration;
            config.DeleteConfiguration += DeleteConfiguration;
            DevicesConfig.Add(config);
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
            foreach (var item in DevicesConfig)
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
