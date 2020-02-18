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
    }

    public class VirtualKeyboardConfiguration
    {
        public int[] keys_to_remove = new int[] { };

        public Dictionary<int, KeyboardKey> key_modifications = new Dictionary<int, KeyboardKey>();

        [JsonProperty("key_conversion")]
        public Dictionary<int, int> KeyConversion = null;

        /// <summary>
        /// A list of paths for each included group json
        /// </summary>
        public string[] included_features = new string[] { };

    }

    public abstract class DeviceLayout
    {
        
        public Dictionary<int, int> LayoutKeyConversion = new Dictionary<int, int>();

        public List<DeviceKeyConfiguration> Keys = new List<DeviceKeyConfiguration>();

        public System.Drawing.Rectangle Region = new System.Drawing.Rectangle(0, 0, 0, 0);

        protected string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        protected string SelectedLayout = "";
        protected string SelectedKeyboardLayout = null;
        protected int DeviceId { get; set; }

        public DeviceLayout(DeviceConfig config)
        {
            DeviceId = config.Id;
            SelectedLayout = config.SelectedLayout;
            SelectedKeyboardLayout = config.SelectedKeyboardLayout;

        }
        //private Grid LayoutCanvas = new Grid();
        public abstract List<DeviceKeyConfiguration> LoadLayout();
        

        public void AddFeature(KeyboardKey[] keys, KeyboardRegion insertion_region = KeyboardRegion.TopLeft)
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

                Keys.Add(new DeviceKeyConfiguration(key, DeviceId));

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

            foreach (var key in Keys)
            {
                if (key.Region.X < x_correction)
                    x_correction = key.Region.X;

                if (key.Region.Y < y_correction)
                    y_correction = key.Region.Y;
            }

            if (Keys.Count > 0)
            {
                //grouped_keys[0].margin_top -= y_correction;

                foreach (var key in Keys)
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
    }
    class KeyboardDeviceLayout : DeviceLayout
    {
        public KeyboardDeviceLayout(DeviceConfig config) : base(config){}

        private PreferredKeyboardLocalization GetSystemKeyboardCulture()
        {
            string culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            switch (culture)
            {
                case ("tr-TR"):
                    return PreferredKeyboardLocalization.tr;
                case ("ja-JP"):
                    return PreferredKeyboardLocalization.jpn;
                case ("de-DE"):
                case ("hsb-DE"):
                case ("dsb-DE"):
                    return PreferredKeyboardLocalization.de;
                case ("fr-CH"):
                case ("de-CH"):
                    return PreferredKeyboardLocalization.swiss;
                case ("fr-FR"):
                case ("br-FR"):
                case ("oc-FR"):
                case ("co-FR"):
                case ("gsw-FR"):
                    return PreferredKeyboardLocalization.fr;
                case ("cy-GB"):
                case ("gd-GB"):
                case ("en-GB"):
                    return PreferredKeyboardLocalization.uk;
                case ("ru-RU"):
                case ("tt-RU"):
                case ("ba-RU"):
                case ("sah-RU"):
                    return PreferredKeyboardLocalization.ru;
                case ("en-US"):
                    return PreferredKeyboardLocalization.us;
                case ("da-DK"):
                case ("se-SE"):
                case ("nb-NO"):
                case ("nn-NO"):
                case ("nordic"):
                    return PreferredKeyboardLocalization.nordic;
                case ("pt-BR"):
                    return PreferredKeyboardLocalization.abnt2;
                case ("dvorak"):
                    return PreferredKeyboardLocalization.dvorak;
                case ("dvorak_int"):
                    return PreferredKeyboardLocalization.dvorak_int;
                case ("hu-HU"):
                    return PreferredKeyboardLocalization.hu;
                case ("it-IT"):
                    return PreferredKeyboardLocalization.it;
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
                    return PreferredKeyboardLocalization.la;
                case ("es-ES"):
                    return PreferredKeyboardLocalization.es;
                case ("iso"):
                    return PreferredKeyboardLocalization.iso;
                case ("ansi"):
                    return PreferredKeyboardLocalization.ansi;
                default:
                    return PreferredKeyboardLocalization.intl;

            }
        }
        private string GetKeyboardCulture()
        {
            PreferredKeyboardLocalization layout = (PreferredKeyboardLocalization)Enum.Parse(typeof(PreferredKeyboardLocalization), SelectedKeyboardLayout);
            
            if (layout == PreferredKeyboardLocalization.None)
            {
                layout = GetSystemKeyboardCulture();
            }
            //_loaded_localization = layout;
            return layout.ToString();

        }
        private class NewKeyboardLayout
        {
            [JsonProperty("layout_width")]
            public int Width = 0;

            [JsonProperty("layout_height")]
            public int Height = 0;

            [JsonProperty("keys")]
            public DeviceKeyConfiguration[] Keys = null;
        }
        public override List<DeviceKeyConfiguration> LoadLayout() 
        {
            var layoutConfigPath = "";
            string keyboard_preference = SelectedLayout;
            if (keyboard_preference != "" && keyboard_preference != "None")
            {
                layoutConfigPath = Path.Combine(layoutsPath, "Keyboard", keyboard_preference + ".json");
            }

            if (!String.IsNullOrWhiteSpace(layoutConfigPath) && File.Exists(layoutConfigPath))
            {
                //Load keyboard layout
                //LoadCulture();
                
                //TODO
                //if (!File.Exists(layoutPath))
                //    LoadDefault();
                var fileName = "Plain Keyboard\\" +SelectedKeyboardLayout + ".json";
                var layoutPath = Path.Combine(layoutsPath, "Keyboard", fileName);
                if (File.Exists(layoutPath))
                {
                    string c = File.ReadAllText(layoutPath, Encoding.UTF8);
                    NewKeyboardLayout keyboard = JsonConvert.DeserializeObject<NewKeyboardLayout>(c, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                    Keys = keyboard.Keys.ToList();
                    Keys.ForEach(k => k.Key.DeviceId = DeviceId);
                    Region.Width = keyboard.Width;
                    Region.Height = keyboard.Height;
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
                

                string content = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                VirtualKeyboardConfiguration layoutConfig = JsonConvert.DeserializeObject<VirtualKeyboardConfiguration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });


                //AdjustKeys
                var adjustKeys = layoutConfig.key_modifications;
                Keys.FindAll(key => adjustKeys.ContainsKey((int)key.Tag)).ForEach(k => k.UpdateFromOtherKey(adjustKeys[k.Tag]));
                var removeKeys = layoutConfig.keys_to_remove;
                Keys.RemoveAll(key => removeKeys.Contains((int)key.Tag));


                NormalizeKeys();


                if (layoutConfig.KeyConversion != null)
                {
                    foreach (var key in layoutConfig.KeyConversion)
                    {
                        if (!this.LayoutKeyConversion.ContainsKey(key.Key))
                            this.LayoutKeyConversion.Add(key.Key, key.Value);
                    }
                }
                foreach (string feature in layoutConfig.included_features)
                {
                    string feature_path = Path.Combine(layoutsPath, "Keyboard", "Extra Features", feature);

                    if (File.Exists(feature_path))
                    {
                        string feature_content = File.ReadAllText(feature_path, Encoding.UTF8);
                        VirtualGroup feature_config = JsonConvert.DeserializeObject<VirtualGroup>(feature_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                        AddFeature(feature_config.grouped_keys.ToArray(), feature_config.origin_region);
                        if (feature_config.KeyConversion != null)
                        {
                            foreach (var key in feature_config.KeyConversion)
                            {
                                if (!this.LayoutKeyConversion.ContainsKey((int)key.Key))
                                    this.LayoutKeyConversion.Add((int)key.Key, (int)key.Value);
                            }
                        }
                    }
                }

                NormalizeKeys();
            }
            else
            {
                //LoadNone();
            }
            return Keys;
        }
  
        private void LoadFromKeys(List<KeyboardKey> JsonKeys)
        {
            int layout_height = 0;
            int layout_width = 0;
            int current_height = 0;
            int current_width = 0;

            foreach (var key in JsonKeys)
            {

                if (key.width + key.margin_left > 0)
                    current_width += (int)key.width.Value + (int)key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += (int)key.margin_top.Value;

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

                Keys.Add(new DeviceKeyConfiguration(key, null));
            }

            Region.Width = layout_width;
            Region.Height = layout_height;
        }

        private class KeyboardLayout
        {
            [JsonProperty("key_conversion")]
            public Dictionary<int, int> KeyConversion = null;

            [JsonProperty("keys")]
            public KeyboardKey[] Keys = null;
        }
        private void LoadCulture(String culture)
        {
            var fileName = "Plain Keyboard\\layout." + culture + ".json";
            var layoutPath = Path.Combine(layoutsPath, "Keyboard", fileName);

            //if (!File.Exists(layoutPath))
            //    LoadDefault();

            string content = File.ReadAllText(layoutPath, Encoding.UTF8);
            KeyboardLayout keyboard = JsonConvert.DeserializeObject<KeyboardLayout>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

            //virtualKeyboardGroup = new VirtualGroup(keyboard.Keys);

            LayoutKeyConversion = keyboard.KeyConversion ?? new Dictionary<int, int>();

        }
    }

    class GeneralDeviceLayout : DeviceLayout
    {
        public GeneralDeviceLayout(DeviceConfig config) : base(config) { }
        public override List<DeviceKeyConfiguration> LoadLayout()
        {
            string layoutConfigPath = "";
            string mouse_preference = SelectedLayout;
            if (mouse_preference != "" && mouse_preference != "None")
            {
                layoutConfigPath = Path.Combine(layoutsPath, "Mouse", mouse_preference + ".json");
            }

            if (!string.IsNullOrWhiteSpace(layoutConfigPath) && File.Exists(layoutConfigPath))
            {
                string mouseConfigContent = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                VirtualGroup mouseConfig = JsonConvert.DeserializeObject<VirtualGroup>(mouseConfigContent, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                AddFeature(mouseConfig.grouped_keys.ToArray(), mouseConfig.origin_region);
                NormalizeKeys();
            }
            return Keys;

        }
        private void LoadFromKeys(List<KeyboardKey> JsonKeys)
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

                Keys.Add(new DeviceKeyConfiguration(key, DeviceId));
            }

            Region.Width = (int)layout_width;
            Region.Height = (int)layout_height;
        }
        private class KeyboardLayout
        {
            [JsonProperty("key_conversion")]
            public Dictionary<int, int> KeyConversion = null;

            [JsonProperty("keys")]
            public KeyboardKey[] Keys = null;
        }
        private void LoadCulture(String culture)
        {
            var fileName = "Plain Keyboard\\layout." + culture + ".json";
            var layoutPath = Path.Combine(layoutsPath, "Keyboard", fileName);

            //if (!File.Exists(layoutPath))
            //    LoadDefault();

            string content = File.ReadAllText(layoutPath, Encoding.UTF8);
            KeyboardLayout keyboard = JsonConvert.DeserializeObject<KeyboardLayout>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

            //virtualKeyboardGroup = new VirtualGroup(keyboard.Keys);

            LayoutKeyConversion = keyboard.KeyConversion ?? new Dictionary<int, int>();

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

        public delegate void SaveConfigEventHandler(DeviceConfig sender);

        public event SaveConfigEventHandler SaveConfiguration;
        public void Save()
        {
            SaveConfiguration?.Invoke(this);
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
            double baseline_x = 0.0;
            double baseline_y = 0.0;
            double current_height = 0;
            double current_width = 0;
            foreach (var layout in DevicesConfig)
            {
                layout.SaveConfiguration += SaveConfiguration;
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
            Width = current_width - baseline_x;
            Height = current_height - baseline_y;

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
