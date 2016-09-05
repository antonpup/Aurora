using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.WormsWMD
{
    public class GameEvent_WormsWMD : GameEvent_Aurora_Wrapper
    {
        public GameEvent_WormsWMD()
        {
            profilename = "WormsWMD";
        }

        public override bool IsEnabled()
        {
            return Global.Configuration.ApplicationProfiles[profilename].Settings.isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer bitmap_layer = new EffectLayer("WormsWMD - Bitmap");

            Devices.DeviceKeys[] allkeys = Enum.GetValues(typeof(Devices.DeviceKeys)).Cast<Devices.DeviceKeys>().ToArray();
            foreach (var key in allkeys)
            {
                if (key == Devices.DeviceKeys.LOGO && logo.Length == 4)
                {
                    int a, r, g, b;

                    b = logo[0];
                    g = logo[1];
                    r = logo[2];
                    a = logo[3];

                    bitmap_layer.Set(key, GetBoostedColor(Color.FromArgb(a, r, g, b)));
                }
                else if (key == Devices.DeviceKeys.Peripheral && peripheral.Length == 4)
                {
                    int a, r, g, b;

                    b = peripheral[0];
                    g = peripheral[1];
                    r = peripheral[2];
                    a = peripheral[3];

                    bitmap_layer.Set(key, GetBoostedColor(Color.FromArgb(a, r, g, b)));
                }
                else
                {
                    Devices.Logitech.Logitech_keyboardBitmapKeys logi_key = Devices.Logitech.LogitechDevice.ToLogitechBitmap(key);

                    if (logi_key != Devices.Logitech.Logitech_keyboardBitmapKeys.UNKNOWN)
                    {
                        int a, r, g, b;

                        b = bitmap[(int)logi_key];
                        g = bitmap[(int)logi_key + 1];
                        r = bitmap[(int)logi_key + 2];
                        a = bitmap[(int)logi_key + 3];

                        bitmap_layer.Set(key, GetBoostedColor(Color.FromArgb(a, r, g, b)));
                    }
                }
            }

            layers.Enqueue(bitmap_layer);

            //Scripts
            Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            frame.AddLayers(layers.ToArray());
        }

        private Color GetBoostedColor(Color color)
        {
            if (!(Global.Configuration.ApplicationProfiles[profilename].Settings as WormsWMDSettings).colorEnhance_Enabled)
                return color;

            // initial_factor * (1 - (x / color_factor))
            float initial_factor = (Global.Configuration.ApplicationProfiles[profilename].Settings as WormsWMDSettings).colorEnhance_initial_factor;
            float color_factor = (Global.Configuration.ApplicationProfiles[profilename].Settings as WormsWMDSettings).colorEnhance_color_factor;

            float boost_amount = 0.0f;
            boost_amount += initial_factor * (1.0f - (color.R / color_factor));
            boost_amount += initial_factor * (1.0f - (color.G / color_factor));
            boost_amount += initial_factor * (1.0f - (color.B / color_factor));
            boost_amount /= initial_factor;

            boost_amount = boost_amount <= 1.0f ? 1.0f : boost_amount;

            return Utils.ColorUtils.MultiplyColorByScalar(color, boost_amount);
        }
    }
}
