﻿using Aurora.Settings;
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
        //TODO: Deal with this
        All = 0,

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

            [Description("Generic Mouse")]
            Generic_Mouse = 1,

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

        public new const byte DeviceTypeID = 1;

        private PreferredMouse style = PreferredMouse.None;
        [JsonIgnore]
        public PreferredMouse Style { get { return style; } set { UpdateVar(ref style, value); } }

        private static string cultures_folder = "kb_layouts";
        private static string layoutsPath = "";

        static MouseDeviceLayout()
        {
            layoutsPath = Path.Combine(Global.ExecutingDirectory, cultures_folder);
        }

        public override void GenerateLayout()
        {
            string mouse_feature_path = "";

            switch (this.Style)
            {
                case PreferredMouse.Generic_Mouse:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "generic_peripheral.json");
                    break;
                case PreferredMouse.Logitech_G900:
                    mouse_feature_path = Path.Combine(layoutsPath, "Extra Features", "logitech_g900_features.json");
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
            }

            if (!string.IsNullOrWhiteSpace(mouse_feature_path))
            {
                string feature_content = File.ReadAllText(mouse_feature_path, Encoding.UTF8);
                VirtualGroup featureConfig = JsonConvert.DeserializeObject<VirtualGroup>(feature_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                this.virtualGroup = featureConfig;
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