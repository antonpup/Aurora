using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Functions;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Desktop
{
    public class Event_Idle : GameEvent
    {
        private long previoustime = 0;
        private long currenttime = 0;

        private Random randomizer;

        private LayerEffectConfig effect_cfg = new LayerEffectConfig();

        private Devices.DeviceKeys[] allKeys = Enum.GetValues(typeof(Devices.DeviceKeys)).Cast<Devices.DeviceKeys>().ToArray();
        private Dictionary<Devices.DeviceKeys, float> stars = new Dictionary<Devices.DeviceKeys, float>();
        private Dictionary<Devices.DeviceKeys, float> raindrops = new Dictionary<Devices.DeviceKeys, float>();
        long nextstarset = 0L;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        public Event_Idle()
        {
            randomizer = new Random();

            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();
        }

        public void UpdateLights(EffectFrame frame)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            Queue<EffectLayer> layers = new Queue<EffectLayer>();
            EffectLayer layer;

            effect_cfg.speed = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_speed;

            switch ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_type)
            {
                case IdleEffects.Dim:
                    layer = new EffectLayer("Idle - Dim");

                    Color dim_color = Color.FromArgb(125, 0, 0, 0);
                    layer.Fill(dim_color);

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.ColorBreathing:
                    layer = new EffectLayer("Idle - Color Breathing");

                    Color breathe_bg_color = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_secondary_color;
                    layer.Fill(breathe_bg_color);

                    float sine = (float)Math.Pow(Math.Sin((double)((currenttime % 10000L) / 10000.0f) * 2 * Math.PI * (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_speed), 2);

                    layer.Fill(Color.FromArgb((byte)(sine * 255), (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_primary_color));

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.RainbowShift_Horizontal:
                    layer = new EffectLayer("Idle - Rainbow Shift (Horizontal)", LayerEffects.RainbowShift_Horizontal, effect_cfg);

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.RainbowShift_Vertical:
                    layer = new EffectLayer("Idle - Rainbow Shift (Vertical)", LayerEffects.RainbowShift_Vertical, effect_cfg);

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.StarFall:
                    layer = new EffectLayer("Idle - Starfall");

                    if (nextstarset < currenttime)
                    {
                        for (int x = 0; x < (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_amount; x++)
                        {
                            Devices.DeviceKeys star = allKeys[randomizer.Next(allKeys.Length)];
                            if (stars.ContainsKey(star))
                                stars[star] = 1.0f;
                            else
                                stars.Add(star, 1.0f);
                        }

                        nextstarset = currenttime + (long)(1000L * (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_frequency);
                    }

                    layer.Fill((Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_secondary_color);

                    Devices.DeviceKeys[] stars_keys = stars.Keys.ToArray();

                    foreach (Devices.DeviceKeys star in stars_keys)
                    {
                        layer.Set(star, Utils.ColorUtils.MultiplyColorByScalar((Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_primary_color, stars[star]));
                        stars[star] -= getDeltaTime() * 0.05f * (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_speed;
                    }

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.RainFall:
                    layer = new EffectLayer("Idle - Rainfall");

                    if (nextstarset < currenttime)
                    {
                        for (int x = 0; x < (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_amount; x++)
                        {
                            Devices.DeviceKeys star = allKeys[randomizer.Next(allKeys.Length)];
                            if (raindrops.ContainsKey(star))
                                raindrops[star] = 1.0f;
                            else
                                raindrops.Add(star, 1.0f);
                        }

                        nextstarset = currenttime + (long)(1000L * (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_frequency);
                    }

                    layer.Fill((Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_secondary_color);

                    Devices.DeviceKeys[] raindrops_keys = raindrops.Keys.ToArray();

                    ColorSpectrum drop_spec = new ColorSpectrum((Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_primary_color, Color.FromArgb(0, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_effect_primary_color));

                    foreach (Devices.DeviceKeys raindrop in raindrops_keys)
                    {
                        EffectPoint pt = Effects.GetBitmappingFromDeviceKey(raindrop).GetCenter();

                        float transition_value = 1.0f - raindrops[raindrop];
                        float radius = transition_value * Effects.canvas_biggest;

                        layer.GetGraphics().DrawEllipse(new Pen(drop_spec.GetColorAt(transition_value), 2),
                            pt.X - radius,
                            pt.Y - radius,
                            2 * radius,
                            2 * radius);

                        raindrops[raindrop] -= getDeltaTime() * 0.05f * (Global.Configuration.dekstop_profile.Settings as DesktopSettings).idle_speed;
                    }

                    layers.Enqueue(layer);
                    break;
                default:
                    break;
            }

            frame.AddOverlayLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            //This event does not take a game state
            UpdateLights(frame);
        }

        public bool IsEnabled()
        {
            return true;
        }
    }
}
