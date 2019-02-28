using Aurora.Settings;
using Aurora.Settings.Keycaps;
using Aurora.Utils;
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
using LEDINT = System.Int16;

namespace Aurora.Devices.Layout.Layouts
{
    /// <summary>
    /// Enum definition, representing everysingle supported device key
    /// </summary>
    public enum MouseLights : LEDINT
    {
        /// <summary>
        /// Peripheral Logo
        /// </summary>
        [Description("Peripheral Logo")]
        Peripheral_Logo = 160,

        /// <summary>
        /// Peripheral Scroll Wheel
        /// </summary>
        [Description("Peripheral Scroll Wheel")]
        Peripheral_ScrollWheel = 161,

        /// <summary>
        /// Peripheral Front-facing lights
        /// </summary>
        [Description("Peripheral Front Lights")]
        Peripheral_FrontLight = 162,

        /// <summary>
        /// Peripheral Back-facing lights
        /// </summary>
        [Description("Peripheral Back Lights")]
        Peripheral_BackLight = 163,

        // Extra lights - intended to be used as an index, so the next is ExtraLightIndex+1 etc.

        /// <summary>
        /// Peripheral Extra lights
        /// </summary>
        [Description("Peripheral Extra Lights Index")]
        Peripheral_ExtraLightIndex = 200,

        /// <summary>
        /// Peripheral Left Extra lights
        /// </summary>
        [Description("Peripheral Extra Lights Index")]
        Peripheral_ExtraLightLeftIndex = 300,

        /// <summary>
        /// Peripheral Right Extra lights
        /// </summary>
        [Description("Peripheral Extra Lights Index")]
        Peripheral_ExtraLightRightIndex = 400,

        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        NONE = -1,
    };

    public class MouseDeviceLayout : DeviceLayout
    {
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

        [JsonIgnore]
        public new static readonly byte DeviceTypeID = 1;

        [JsonIgnore]
        public override byte GetDeviceTypeID { get { return DeviceTypeID; } }

        private PreferredMouse style = PreferredMouse.None;
        //[JsonIgnore]
        public PreferredMouse Style { get { return style; } set { UpdateVar(ref style, value); } }

        private static string cultures_folder = "kb_layouts";
        private static string layoutsPath = "";

        static MouseDeviceLayout()
        {
            layoutsPath = Path.Combine(Global.ExecutingDirectory, cultures_folder);
        }

        private class MouseLayout
        {
            [JsonProperty("grouped_keys")]
            public VirtualLight[] Keys = null;
        }

        public override void GenerateLayout()
        {
            string mouse_feature_path = "";

            switch (Style)
            {
                case PreferredMouse.Generic_Peripheral:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "generic_peripheral.json");
                    break;
                case PreferredMouse.Generic_Mousepad:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "generic_mousepad.json");
                    break;
                case PreferredMouse.Logitech_G900:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "logitech_g900_features.json");
                    break;
                case PreferredMouse.Logitech_G502:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "logitech_g502_features.json");
                    break;
                case PreferredMouse.Corsair_Sabre:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "corsair_sabre_features.json");
                    break;
                case PreferredMouse.Corsair_M65:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "corsair_m65_features.json");
                    break;
                case PreferredMouse.Corsair_Katar:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "corsair_katar_features.json");
                    break;
                case PreferredMouse.Clevo_Touchpad:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "clevo_touchpad_features.json");
                    break;
                case PreferredMouse.SteelSeries_Rival_300:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "steelseries_rival_300_features.json");
                    break;
                case PreferredMouse.SteelSeries_Rival_300_HP_OMEN_Edition:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "steelseries_rival_300_hp_omen_edition_features.json");
                    break;
                case PreferredMouse.Asus_Pugio:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "asus_pugio_features.json");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(mouse_feature_path))
            {
                string feature_content = File.ReadAllText(mouse_feature_path, Encoding.UTF8);
                MouseLayout mouse = JsonConvert.DeserializeObject<MouseLayout>(feature_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                virtualGroup = new VirtualGroup(this, mouse.Keys);
                virtualGroup.CalculateBitmap();
                /*if (mouse_orientation == MouseOrientationType.LeftHanded)
                {
                    if (featureConfig.origin_region == KeyboardRegion.TopRight)
                        featureConfig.origin_region = KeyboardRegion.TopLeft;
                    else if (featureConfig.origin_region == KeyboardRegion.BottomRight)
                        featureConfig.origin_region = KeyboardRegion.BottomLeft;

                    double outlineWidth = 0.0;
                    int outlineWidthBits = 0;

                    foreach (var key in featureConfig.grouped_keys)
                    {
                        if (outlineWidth == 0.0 && outlineWidthBits == 0) //We found outline (NOTE: Outline has to be first in the grouped keys)
                        {
                            if (key.tag == MouseKeys.NONE)
                            {
                                outlineWidth = key.width.Value + 2 * key.margin_left.Value;
                                //outlineWidthBits = key.width_bits.Value + 2 * key.margin_left_bits.Value;
                            }
                        }

                        key.margin_left -= outlineWidth;
                        //key.margin_left_bits -= outlineWidthBits;
                    }

                }

                virtualKeyboardGroup.AddFeature(featureConfig.grouped_keys.ToArray(), featureConfig.origin_region);*/
            }
        }

        public override string GetLEDName(short ledID)
        {
            return ((MouseLights)ledID).GetDescription();
        }

        protected override void loadLayouts()
        {
            throw new NotImplementedException();
        }
    }

}
