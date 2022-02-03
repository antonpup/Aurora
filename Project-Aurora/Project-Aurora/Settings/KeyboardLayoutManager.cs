using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Aurora.Devices;
using System.Windows.Media.Imaging;
using Aurora.Settings.Keycaps;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Aurora.Settings.DeviceLayoutViewer;

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
    public class KeyboardKey
    {
        public string visualName = null;
        public Devices.DeviceKeys tag = DeviceKeys.NONE;
        public bool? line_break;
        public double? margin_left;
        public double? margin_top;
        public double? width;
        public double? height;
        public double? font_size;
        public bool? enabled = true;
        public bool? absolute_location = false;
        public String image = "";

        public KeyboardKey()
        {
        }

        public KeyboardKey(string text, Devices.DeviceKeys tag, bool? enabled = true, bool? linebreak = false, double? fontsize = 12, double? margin_left = 7, double? margin_top = 0, double? width = 30, double? height = 30, int? width_bits = 2, int? height_bits = 2, int? margin_left_bits = 0, int? margin_top_bits = 0)
        {
            this.visualName = text;
            this.tag = tag;
            this.line_break = linebreak;
            this.width = width;
            this.height = height;
            this.font_size = fontsize;
            this.margin_left = margin_left;
            this.margin_top = margin_top;
            this.enabled = enabled;
        }

        public KeyboardKey UpdateFromOtherKey(KeyboardKey otherKey)
        {
            if (otherKey != null)
            {
                if (otherKey.visualName != null) this.visualName = otherKey.visualName;
                if (otherKey.tag != DeviceKeys.NONE)
                    this.tag = otherKey.tag;
                if (otherKey.line_break != null) this.line_break = otherKey.line_break;
                if (otherKey.width != null) this.width = otherKey.width;
                if (otherKey.height != null)
                    this.height = otherKey.height;
                if (otherKey.font_size != null) this.font_size = otherKey.font_size;
                if (otherKey.margin_left != null) this.margin_left = otherKey.margin_left;
                if (otherKey.margin_top != null) this.margin_top = otherKey.margin_top;
                if (otherKey.enabled != null) this.enabled = otherKey.enabled;
            }
            return this;
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
    public class KeyboardLayout
    {
        [JsonProperty("key_conversion")]
        public Dictionary<Devices.DeviceKeys, Devices.DeviceKeys> KeyConversion = null;

        [JsonProperty("keys")]
        public KeyboardKey[] Keys = null;
    }

    public enum KeyboardRegion
    {
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4
    }

    public class VirtualGroupConfiguration
    {
        public Devices.DeviceKeys[] keys_to_remove = new Devices.DeviceKeys[] { };

        public Dictionary<DeviceKeys, KeyboardKey> key_modifications = new Dictionary<DeviceKeys, KeyboardKey>();

        [JsonProperty("key_conversion")]
        public Dictionary<DeviceKeys, DeviceKeys> KeyConversion = null;

        /// <summary>
        /// A list of paths for each included group json
        /// </summary>
        public string[] included_features = new string[] { };

        public VirtualGroupConfiguration()
        {

        }
    }

    public class VirtualGroup
    {
        public string group_tag;

        public KeyboardRegion? origin_region;

        public List<KeyboardKey> grouped_keys = new List<KeyboardKey>();

        public Dictionary<DeviceKeys, string> KeyText = new Dictionary<DeviceKeys, string>();

        private System.Drawing.RectangleF _region = new System.Drawing.RectangleF(0, 0, 0, 0);

        public System.Drawing.RectangleF Region { get { return _region; } }

        [JsonProperty("key_conversion")]
        public Dictionary<DeviceKeys, DeviceKeys> KeyConversion = null;

        public VirtualGroup()
        {

        }

        public VirtualGroup(KeyboardKey[] keys)
        {
            double layout_height = 0;
            double layout_width = 0;
            double current_height = 0;
            double current_width = 0;

            /*int width_bit = 0;
            int height_bit = 0;
            int width_bit_max = 1;
            int height_bit_max = 1;*/

            foreach (var key in keys)
            {
                grouped_keys.Add(key);
                KeyText.Add(key.tag, key.visualName);

                if (key.width + key.margin_left > 0)
                    current_width += key.width.Value + key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += key.margin_top.Value;


                if (layout_width < current_width)
                    layout_width = current_width;

                if (key.line_break.Value)
                {
                    current_height += 37;
                    current_width = 0;
                }

                if (layout_height < current_height)
                    layout_height = current_height;


                /*int key_tly = KeyboardLayoutManager.PixelToByte(key.margin_top.Value) + height_bit;
                int key_tlx = KeyboardLayoutManager.PixelToByte(key.margin_left.Value) + width_bit;

                int key_bry = key_tly + KeyboardLayoutManager.PixelToByte(key.height.Value);
                int key_brx = key_tlx + KeyboardLayoutManager.PixelToByte(key.width.Value);

                if (width_bit_max < key_brx) width_bit_max = key_brx;
                if (height_bit_max < key_bry) height_bit_max = key_bry;


                if (key.line_break.Value)
                {
                    height_bit += KeyboardLayoutManager.PixelToByte(37);
                    width_bit = 0;
                }
                else
                {
                    width_bit = key_brx;
                    height_bit = key_tly;
                }*/

            }

            _region.Width = (float)layout_width;
            _region.Height = (float)layout_height;

            /*_region_bitmap.Width = width_bit_max;
            _region_bitmap.Height = height_bit_max;*/

            //NormalizeKeys();
        }

        public void AddFeature(KeyboardKey[] keys, KeyboardRegion insertion_region = KeyboardRegion.TopLeft)
        {
            double location_x = 0.0D;
            double location_y = 0.0D;
            int location_x_bit = 0;
            int location_y_bit = 0;

            if (insertion_region == KeyboardRegion.TopRight)
            {
                location_x = _region.Width;
                //location_x_bit = _region_bitmap.Width;
            }
            else if (insertion_region == KeyboardRegion.BottomLeft)
            {
                location_y = _region.Height;
                //location_y_bit = _region_bitmap.Height;
            }
            else if (insertion_region == KeyboardRegion.BottomRight)
            {
                location_x = _region.Width;
                location_y = _region.Height;
                //location_x_bit = _region_bitmap.Width;
                //location_y_bit = _region_bitmap.Height;
            }

            float added_width = 0.0f;
            float added_height = 0.0f;
            //int added_width_bits = 0;
            //int added_height_bits = 0;

            foreach (var key in keys)
            {
                key.margin_left += location_x;
                key.margin_top += location_y;

                //key.margin_left_bits += location_x_bit;
                //key.margin_top_bits += location_y_bit;

                grouped_keys.Add(key);
                if (KeyText.ContainsKey(key.tag))
                    KeyText.Remove(key.tag);
                KeyText.Add(key.tag, key.visualName);

                if (key.width + key.margin_left > _region.Width)
                    _region.Width = (float)(key.width + key.margin_left);
                else if (key.margin_left + added_width < 0)
                {
                    added_width = -(float)(key.margin_left);
                    _region.Width -= (float)(key.margin_left);
                }

                if (key.height + key.margin_top > _region.Height)
                    _region.Height = (float)(key.height + key.margin_top);
                else if (key.margin_top + added_height < 0)
                {
                    added_height = -(float)(key.margin_top);
                    _region.Height -= (float)(key.margin_top);
                }


                /*if (KeyboardLayoutManager.PixelToByte(key.width.Value) + KeyboardLayoutManager.PixelToByte(key.margin_left.Value) > _region_bitmap.Width)
                    _region_bitmap.Width += KeyboardLayoutManager.PixelToByte(key.width.Value) + KeyboardLayoutManager.PixelToByte(key.margin_left.Value) - location_x_bit;
                else if (KeyboardLayoutManager.PixelToByte(key.margin_left.Value) + added_width_bits < 0)
                {
                    added_width_bits = -KeyboardLayoutManager.PixelToByte(key.margin_left.Value);
                    _region_bitmap.Width -= KeyboardLayoutManager.PixelToByte(key.margin_left.Value);
                }

                if (KeyboardLayoutManager.PixelToByte(key.height.Value) + KeyboardLayoutManager.PixelToByte(key.margin_top.Value) > _region_bitmap.Height)
                    _region_bitmap.Height += KeyboardLayoutManager.PixelToByte(key.height.Value) + KeyboardLayoutManager.PixelToByte(key.margin_top.Value) - location_y_bit;
                else if (KeyboardLayoutManager.PixelToByte(key.margin_top.Value) + added_height_bits < 0)
                {
                    added_height_bits = -KeyboardLayoutManager.PixelToByte(key.margin_top.Value);
                    _region_bitmap.Height -= KeyboardLayoutManager.PixelToByte(key.margin_top.Value);
                }*/

            }

            NormalizeKeys();
        }

        private void NormalizeKeys()
        {
            double x_correction = 0.0D;
            double y_correction = 0.0D;

            //int x_correction_bit = 0;
            //int y_correction_bit = 0;

            foreach (var key in grouped_keys)
            {
                if (!key.absolute_location.Value)
                    continue;

                if (key.margin_left < x_correction)
                    x_correction = key.margin_left.Value;

                if (key.margin_top < y_correction)
                    y_correction = key.margin_top.Value;

                /*if (key.margin_left_bits < x_correction_bit)
                    x_correction_bit = key.margin_left_bits.Value;

                if (key.margin_top_bits < y_correction_bit)
                    y_correction_bit = key.margin_top_bits.Value;*/
            }

            if (grouped_keys.Count > 0)
            {
                grouped_keys[0].margin_top -= y_correction;
                //grouped_keys[0].margin_top_bits -= y_correction_bit;

                bool previous_linebreak = true;
                foreach (var key in grouped_keys)
                {
                    if (key.absolute_location.Value)
                    {
                        key.margin_top -= y_correction;
                        key.margin_left -= x_correction;
                        /*key.margin_top_bits -= y_correction_bit;
                        key.margin_left_bits -= x_correction_bit;*/
                    }
                    else
                    {
                        if (previous_linebreak && !key.line_break.Value)
                        {
                            key.margin_left -= x_correction;
                            //key.margin_left_bits -= x_correction_bit;
                        }

                        previous_linebreak = key.line_break.Value;
                    }
                }

            }
        }

        public void Clear()
        {
            _region = new System.Drawing.RectangleF(0, 0, 0, 0);
            //_region_bitmap = new Rectangle(0, 0, 0, 0);
            grouped_keys.Clear();
        }

        internal void AdjustKeys(Dictionary<DeviceKeys, KeyboardKey> keys)
        {
            var applicable_keys = grouped_keys.FindAll(key => keys.ContainsKey(key.tag));

            foreach (var key in applicable_keys)
            {
                KeyboardKey otherKey = keys[key.tag];
                if (key.tag != otherKey.tag)
                    KeyText.Remove(key.tag);
                key.UpdateFromOtherKey(otherKey);
                if (KeyText.ContainsKey(key.tag))
                    KeyText[key.tag] = key.visualName;
                else
                    KeyText.Add(key.tag, key.visualName);
            }
        }

        internal void RemoveKeys(DeviceKeys[] keys_to_remove)
        {
            var applicable_keys = grouped_keys.RemoveAll(key => keys_to_remove.Contains(key.tag));

            double layout_height = 0;
            double layout_width = 0;
            double current_height = 0;
            double current_width = 0;

            /*int width_bit = 0;
            int height_bit = 0;
            int width_bit_max = 1;
            int height_bit_max = 1;*/

            foreach (var key in grouped_keys)
            {
                if (key.width + key.margin_left > 0)
                    current_width += key.width.Value + key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += key.margin_top.Value;


                if (layout_width < current_width)
                    layout_width = current_width;

                if (key.line_break.Value)
                {
                    current_height += 37;
                    current_width = 0;
                }

                if (layout_height < current_height)
                    layout_height = current_height;

                KeyText.Remove(key.tag);

                /*int key_tly = KeyboardLayoutManager.PixelToByte(key.margin_top.Value) + height_bit;
                int key_tlx = KeyboardLayoutManager.PixelToByte(key.margin_left.Value) + width_bit;

                int key_bry = key_tly + KeyboardLayoutManager.PixelToByte(key.height.Value);
                int key_brx = key_tlx + KeyboardLayoutManager.PixelToByte(key.width.Value);

                if (width_bit_max < key_brx) width_bit_max = key_brx;
                if (height_bit_max < key_bry) height_bit_max = key_bry;


                if (key.line_break.Value)
                {
                    height_bit += 3;
                    width_bit = 0;
                }
                else
                {
                    width_bit = key_brx;
                    height_bit = key_tly;
                }*/

            }

            _region.Width = (float)layout_width;
            _region.Height = (float)layout_height;

            //_region_bitmap.Width = width_bit_max;
            //_region_bitmap.Height = height_bit_max;

        }
    }
    public class DeviceLayout
    {

        public Dictionary<int, DeviceKeyConfiguration> Keys = new Dictionary<int, DeviceKeyConfiguration>();

        protected string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        private System.Drawing.Rectangle Region = new System.Drawing.Rectangle(0, 0, 0, 0);

        public int Width => Region.Width;

        public int Height => Region.Height;

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
        /*public void SaveLayout(List<Control_Keycap> layoutKey, Point offset)
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

            File.WriteAllText(Path.Combine(layoutsPath, GetFolder(), Config.SelectedLayout + ".json"), content, Encoding.UTF8);
        }*/
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

                Keys.Add((int)key.tag, new DeviceKeyConfiguration(key, null));
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
            LightingEnabled = config.LightingEnabled;
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
            if (config is KeyboardConfig keyboardConfig)
                SelectedKeyboardLayout = keyboardConfig.SelectedKeyboardLayout;
            else
                SelectedKeyboardLayout = GetSystemKeyboardCulture();
            Type = 0;
        }

        public KeyboardConfig()
        {
            Type = 0;
            SelectedKeyboardLayout = GetSystemKeyboardCulture();
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
    public class KeyboardLayoutManager
    {
        public Dictionary<DeviceKeys, DeviceKeys> LayoutKeyConversion = new Dictionary<DeviceKeys, DeviceKeys>();

        private Dictionary<Devices.DeviceKeys, IKeycap> _virtualKeyboardMap = new Dictionary<DeviceKeys, IKeycap>();

        public KeyboardConfig keyboardConfig = new KeyboardConfig();
        public DeviceConfig mouseConfig = new DeviceConfig();

        public Grid VirtualDevicesLayout { get; private set; } = new Grid();

        public Grid AbstractVirtualKeyboard => CreateUserControl(true);

        private int layoutWidth = 850;
        private int layoutHeight = 200;

        public delegate void LayoutUpdatedEventHandler(object sender);

        public event LayoutUpdatedEventHandler KeyboardLayoutUpdated;

        private List<Control_Keycap> KeycapLayouts = new List<Control_Keycap>();

        public KeyboardPhysicalLayout Loaded_Localization => keyboardConfig.SelectedKeyboardLayout;

        public KeyboardLayoutManager()
        {
            Global.Configuration.PropertyChanged += Configuration_PropertyChanged;
        }

        public void LoadBrandDefault()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadBrand(Global.Configuration.KeyboardBrand, Global.Configuration.MousePreference, Global.Configuration.MouseOrientation);
            });
        }

        public void LoadBrand(PreferredKeyboard keyboard_preference = PreferredKeyboard.None, PreferredMouse mouse_preference = PreferredMouse.None, MouseOrientationType mouse_orientation = MouseOrientationType.RightHanded)
        {
            //Load keyboard layout
            keyboardConfig.SelectedLayout = GetKeyboardJsonName(keyboard_preference);
            keyboardConfig.SelectedKeyboardLayout = Global.Configuration.KeyboardLocalization;

            var Keys = new List<DeviceKeyConfiguration>();
            if (!String.IsNullOrWhiteSpace(keyboardConfig.SelectedLayout))
            {
                DeviceLayout keyboardLayout = new DeviceLayout(keyboardConfig);
                var keyboardKeys = keyboardLayout.LoadLayout();

                mouseConfig.SelectedLayout = GetMouseJsonName(mouse_preference);
                mouseConfig.Type = 1;
                DeviceLayout mouseLayout = new DeviceLayout(mouseConfig);
                var mouseKeys = mouseLayout.LoadLayout();

                layoutWidth = keyboardLayout.Width + mouseLayout.Width + 20;
                layoutHeight = (keyboardLayout.Height > mouseLayout.Height) ? keyboardLayout.Height : mouseLayout.Height;

                
                
                foreach (var key in keyboardKeys)
                {
                    if (layoutHeight > keyboardLayout.Height)
                        key.Y += (layoutHeight - keyboardLayout.Height) / 2;
                    if (mouse_orientation == MouseOrientationType.LeftHanded)
                        key.X += mouseLayout.Width + 15;
                    Keys.Add(key);
                }
                foreach (var key in mouseKeys)
                {
                    if (layoutHeight > mouseLayout.Height)
                        key.Y += (layoutHeight - mouseLayout.Height) / 2;
                    if (mouse_orientation == MouseOrientationType.RightHanded)
                        key.X += keyboardLayout.Width + 15;
                    Keys.Add(key);
                }

                KeycapLayouts.Clear();
                Keys.ForEach(k => KeycapLayouts.Add(new Control_Keycap(k)));

            }

            CreateUserControl();

            //Calculate Bitmap
            var bitmap = new Dictionary<DeviceKeys, BitmapRectangle>();
            foreach (var key in Keys)
            {

                double width = key.Width;
                double height = key.Height;
                double x_offset = key.X;
                double y_offset = key.Y;

                bitmap[(DeviceKeys)key.Key.Tag] = new BitmapRectangle(PixelToByte(x_offset), PixelToByte(y_offset), PixelToByte(width), PixelToByte(height));

            }
            Global.effengine.SetCanvasSize(PixelToByte(layoutWidth) + 1, PixelToByte(layoutHeight) + 1);
            Global.effengine.SetBitmapping(bitmap);

            KeyboardLayoutUpdated?.Invoke(this);
        }

        public static int PixelToByte(double pixel)
        {
            return (int)Math.Round(pixel / (double)(Global.Configuration.BitmapAccuracy));
        }

        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Configuration.BitmapAccuracy)))
            {
                Global.LightingStateManager.PostUpdate += this.LightingStateManager_PostUpdate;
            }
        }

        private void LightingStateManager_PostUpdate(object sender, EventArgs e)
        {
            this.LoadBrandDefault();
            Global.LightingStateManager.PostUpdate -= this.LightingStateManager_PostUpdate;
        }


        private Grid CreateUserControl(bool abstractKeycaps = false)
        {
            if (!abstractKeycaps)
                _virtualKeyboardMap.Clear();

            Grid new_virtual_keyboard = new Grid();
            if(KeycapLayouts.Any())
            {
                _virtualKeyboardMap.Clear();
                foreach (var keyCap in KeycapLayouts)
                {
                    new_virtual_keyboard.Children.Add(keyCap);
                    if (keyCap.Keycap.GetKey() != DeviceKeys.NONE && !_virtualKeyboardMap.ContainsKey(keyCap.Keycap.GetKey()) && keyCap is IKeycap)
                        _virtualKeyboardMap.Add(keyCap.Keycap.GetKey(), keyCap as IKeycap);
                }
                //Update size
                new_virtual_keyboard.Width = layoutWidth;
                new_virtual_keyboard.Height = layoutHeight;
            }
            else
            {
                //No items, display error
                Label error_message = new Label();

                DockPanel info_panel = new DockPanel();

                TextBlock info_message = new TextBlock()
                {
                    Text = "No keyboard selected\r\nPlease select your keyboard in the settings",
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                };

                DockPanel.SetDock(info_message, Dock.Top);
                info_panel.Children.Add(info_message);

                DockPanel info_instruction = new DockPanel();

                info_instruction.Children.Add(new TextBlock()
                {
                    Text = "Press (",
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                info_instruction.Children.Add(new System.Windows.Controls.Image()
                {
                    Source = new BitmapImage(new Uri(@"Resources/settings_icon.png", UriKind.Relative)),
                    Stretch = Stretch.Uniform,
                    Height = 40.0,
                    VerticalAlignment = VerticalAlignment.Center
                });

                info_instruction.Children.Add(new TextBlock()
                {
                    Text = ") and go into \"Devices & Wrappers\" tab",
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                DockPanel.SetDock(info_instruction, Dock.Bottom);
                info_panel.Children.Add(info_instruction);

                error_message.Content = info_panel;

                error_message.FontSize = 16.0;
                error_message.FontWeight = FontWeights.Bold;
                error_message.HorizontalContentAlignment = HorizontalAlignment.Center;
                error_message.VerticalContentAlignment = VerticalAlignment.Center;

                new_virtual_keyboard.Children.Add(error_message);

                //Update size
                new_virtual_keyboard.Width = 850;
                new_virtual_keyboard.Height = 200;
            }

            if (!abstractKeycaps)
            {
                VirtualDevicesLayout.Children.Clear();
                VirtualDevicesLayout = new_virtual_keyboard;

                Effects.grid_baseline_x = 0.0f;
                Effects.grid_baseline_y = 0.0f;
                Effects.grid_height = (float)new_virtual_keyboard.Height;
                Effects.grid_width = (float)new_virtual_keyboard.Width;
            }
            return new_virtual_keyboard;
        }


        public void SetKeyboardColors(Dictionary<Devices.DeviceKeys, System.Drawing.Color> keylights)
        {
            foreach (var kvp in _virtualKeyboardMap)
            {
                if (keylights.ContainsKey(kvp.Key))
                {
                    System.Drawing.Color key_color = keylights[kvp.Key];
                    kvp.Value.SetColor(Utils.ColorUtils.DrawingColorToMediaColor(System.Drawing.Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(key_color, key_color.A / 255.0D))));
                }
            }
        }
        public string GetKeyboardJsonName(PreferredKeyboard keyboard_preference)
        {
            if (keyboard_preference == PreferredKeyboard.Logitech_G910)
                return "logitech_g910";
            else if (keyboard_preference == PreferredKeyboard.Logitech_G810)
                return "logitech_g810";
            else if (keyboard_preference == PreferredKeyboard.Logitech_GPRO)
                return "logitech_gpro";
            else if (keyboard_preference == PreferredKeyboard.Logitech_G410)
                return "logitech_g410";
            else if (keyboard_preference == PreferredKeyboard.Logitech_G815)
                return "logitech_g815";
            else if (keyboard_preference == PreferredKeyboard.Logitech_G513)
                return "logitech_g513";
            else if (keyboard_preference == PreferredKeyboard.Logitech_G213)
                return "logitech_g213";
            else if (keyboard_preference == PreferredKeyboard.Corsair_K95)
                return "corsair_k95";
            else if (keyboard_preference == PreferredKeyboard.Corsair_K95_PL)
                return "corsair_k95_platinum";
            else if (keyboard_preference == PreferredKeyboard.Corsair_K70)
                return "corsair_k70";
            else if (keyboard_preference == PreferredKeyboard.Corsair_K70MK2)
                return "corsair_k70_mk2";
            else if (keyboard_preference == PreferredKeyboard.Corsair_K65)
                return "corsair_k65";
            else if (keyboard_preference == PreferredKeyboard.Corsair_STRAFE)
                return "corsair_strafe";
            else if (keyboard_preference == PreferredKeyboard.Corsair_STRAFE_MK2)
                return "corsair_strafe_mk2";
            else if (keyboard_preference == PreferredKeyboard.Corsair_K68)
                return "corsair_k68";
            else if (keyboard_preference == PreferredKeyboard.Razer_Blackwidow)
                return "razer_blackwidow";
            else if (keyboard_preference == PreferredKeyboard.Razer_Blackwidow_X)
                return "razer_blackwidow_x";
            else if (keyboard_preference == PreferredKeyboard.Razer_Blackwidow_TE)
                return "razer_blackwidow_te";
            else if (keyboard_preference == PreferredKeyboard.Razer_Blade)
                return "razer_blade";
            else if (keyboard_preference == PreferredKeyboard.Masterkeys_Pro_L)
                return "masterkeys_pro_l";
            else if (keyboard_preference == PreferredKeyboard.Masterkeys_Pro_S)
                return "masterkeys_pro_s";
            else if (keyboard_preference == PreferredKeyboard.Masterkeys_Pro_M)
                return "masterkeys_pro_m";
            else if (keyboard_preference == PreferredKeyboard.Masterkeys_MK750)
                return "masterkeys_mk750";
            else if (keyboard_preference == PreferredKeyboard.Masterkeys_MK730)
                return "masterkeys_mk730";
            else if (keyboard_preference == PreferredKeyboard.Cooler_Master_SK650)
                return "cooler_master_sk650";
            else if (keyboard_preference == PreferredKeyboard.Roccat_Ryos)
                return "roccat_ryos";
            else if (keyboard_preference == PreferredKeyboard.SteelSeries_Apex_M800)
                return "steelseries_apex_m800";
            else if (keyboard_preference == PreferredKeyboard.SteelSeries_Apex_M750)
                return "steelseries_apex_m750";
            else if (keyboard_preference == PreferredKeyboard.SteelSeries_Apex_M750_TKL)
                return "steelseries_apex_m750_tkl";
            else if (keyboard_preference == PreferredKeyboard.Wooting_One)
                return "wooting_one";
            else if (keyboard_preference == PreferredKeyboard.Asus_Strix_Flare)
                return "asus_strix_flare";
            else if (keyboard_preference == PreferredKeyboard.Asus_Strix_Scope)
                return "asus_strix_scope";
            else if (keyboard_preference == PreferredKeyboard.SoundBlasterX_Vanguard_K08)
                return "soundblasterx_vanguardk08";
            else if (keyboard_preference == PreferredKeyboard.GenericLaptop)
                return "generic_laptop";
            else if (keyboard_preference == PreferredKeyboard.GenericLaptopNumpad)
                return "generic_laptop_numpad";
            else if (keyboard_preference == PreferredKeyboard.Drevo_BladeMaster)
                return "drevo_blademaster";
            else if (keyboard_preference == PreferredKeyboard.Wooting_Two)
                return "wooting_two";
            else if (keyboard_preference == PreferredKeyboard.Uniwill2ND_35X)
                return "Uniwill2ND_35X";
            else if (keyboard_preference == PreferredKeyboard.Uniwill2P1_550)
                return "Uniwill2P1_550";
            else if (keyboard_preference == PreferredKeyboard.Uniwill2P2_650)
                return "Uniwill2P2_650";
            else if (keyboard_preference == PreferredKeyboard.Ducky_Shine_7)
                return "ducky_shine_7";
            else if (keyboard_preference == PreferredKeyboard.Ducky_One_2_RGB_TKL)
                return "ducky_one_2_rgb_tkl";
            else if (keyboard_preference == PreferredKeyboard.OMEN_Sequencer)
                return "omen_sequencer";
            else if (keyboard_preference == PreferredKeyboard.OMEN_Four_Zone)
                return "omen_four_zone";
            else if (keyboard_preference == PreferredKeyboard.HyperX_Alloy_Elite_RGB)
                return "hyperx_alloy_elite_rgb";
            else
                return "";

        }
        string GetMouseJsonName(PreferredMouse mouse_preference)
        {
            switch (mouse_preference)
            {
                case PreferredMouse.Generic_Peripheral:
                    return "Generic Peripheral";
                    break;
                case PreferredMouse.Generic_Mousepad:
                    return "Generic Mousepad";
                    break;
                case PreferredMouse.Logitech_G900:
                    return "Logitech - G900";
                    break;
                case PreferredMouse.Logitech_G502:
                    return "Logitech - G502";
                    break;
                case PreferredMouse.Corsair_Sabre:
                    return "Corsair - Sabre";
                    break;
                case PreferredMouse.Corsair_M65:
                    return "Corsair - M65";
                    break;
                case PreferredMouse.Corsair_Katar:
                    return "Corsair - Katar";
                    break;
                case PreferredMouse.Clevo_Touchpad:
                    return "Clevo - Touchpad";
                    break;
                case PreferredMouse.Roccat_Kone_Pure:
                    return "Roccat - Kone Pure";
                    break;
                case PreferredMouse.SteelSeries_Rival_300:
                    return "SteelSeries - Rival 300";
                    break;
                case PreferredMouse.SteelSeries_Rival_300_HP_OMEN_Edition:
                    return "SteelSeries - Rival 300 HP OMEN Edition";
                    break;
                case PreferredMouse.SteelSeries_QcK_Prism:
                    return "SteelSeries - QcK Prism Mousepad + Mouse";
                    break;
                case PreferredMouse.SteelSeries_QcK_2_Zone:
                    return "SteelSeries - Two-zone QcK Mousepad + Mouse";
                    break;
                case PreferredMouse.Asus_Pugio:
                    return "Asus - Pugio";
                    break;
                case PreferredMouse.OMEN_Photon:
                    return "Omen - Photon";
                    break;
                case PreferredMouse.OMEN_Outpost_Plus_Photon:
                    return "Omen - Outpost + Photon";
                    break;
                case PreferredMouse.OMEN_Vector:
                    return "Omen - Vector";
                    break;
                case PreferredMouse.OMEN_Vector_Essentials:
                    return "Omen - Vector Essentials";
                    break;
                case PreferredMouse.Razer_Mamba_TE:
                    return "Razer - Mamba TE";
                    break;
                default:
                    return "";
            }
        }
    }
}
