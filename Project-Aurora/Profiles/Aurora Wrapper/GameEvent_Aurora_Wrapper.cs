using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.EffectsEngine;
using System.Drawing;

namespace Aurora.Profiles.Aurora_Wrapper
{
    public class GameEvent_Aurora_Wrapper : LightEvent
    {
        internal int[] bitmap = new int[126];
        internal Color logo = Color.Empty;
        internal Color peripheral = Color.Empty;
        internal Color g1 = Color.Empty;
        internal Color g2 = Color.Empty;
        internal Color g3 = Color.Empty;
        internal Color g4 = Color.Empty;
        internal Color g5 = Color.Empty;
        internal Color last_fill_color = Color.Black;
        internal Dictionary<Devices.DeviceKeys, KeyEffect> key_effects = new Dictionary<Devices.DeviceKeys, KeyEffect>();
        internal EntireEffect current_effect = null;

        internal Dictionary<Devices.DeviceKeys, Color> colors = new Dictionary<Devices.DeviceKeys, Color>();

        internal bool colorEnhance_Enabled = false;
        internal int colorEnhance_Mode = 0;
        internal int colorEnhance_color_factor = 90;
        internal float colorEnhance_color_simple = 1.2f;
        internal float colorEnhance_color_gamma = 2.5f;

        protected virtual void UpdateExtraLights(Queue<EffectLayer> layers)
        {

        }

        public override sealed void UpdateLights(EffectFrame frame)
        {
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            if (this.Profile != null)
            {
                foreach (var layer in this.Profile.Settings.Layers.Reverse().ToArray())
                {
                    if (layer.Enabled && layer.LogicPass)
                        layers.Enqueue(layer.Render(_game_state));
                }

                //No need to repeat the code around this everytime this is inherited
                this.UpdateExtraLights(layers);

                //Scripts
                this.Profile.UpdateEffectScripts(layers, _game_state);
            }

            frame.AddLayers(layers.ToArray());
        }

        internal virtual void UpdateWrapperLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer colorfill_layer = new EffectLayer("Aurora Wrapper - Color Fill", GetBoostedColor(last_fill_color));

            layers.Enqueue(colorfill_layer);

            EffectLayer bitmap_layer = new EffectLayer("Aurora Wrapper - Bitmap");

            Devices.DeviceKeys[] allkeys = Enum.GetValues(typeof(Devices.DeviceKeys)).Cast<Devices.DeviceKeys>().ToArray();
            foreach (var key in allkeys)
            {
                if (key == Devices.DeviceKeys.LOGO)
                    bitmap_layer.Set(key, GetBoostedColor(logo));
                else if (key == Devices.DeviceKeys.Peripheral)
                    bitmap_layer.Set(key, GetBoostedColor(peripheral));
                else if (key == Devices.DeviceKeys.G1)
                    bitmap_layer.Set(key, GetBoostedColor(g1));
                else if (key == Devices.DeviceKeys.G2)
                    bitmap_layer.Set(key, GetBoostedColor(g2));
                else if (key == Devices.DeviceKeys.G3)
                    bitmap_layer.Set(key, GetBoostedColor(g3));
                else if (key == Devices.DeviceKeys.G4)
                    bitmap_layer.Set(key, GetBoostedColor(g4));
                else if (key == Devices.DeviceKeys.G5)
                    bitmap_layer.Set(key, GetBoostedColor(g5));
                else
                {
                    Devices.Logitech.Logitech_keyboardBitmapKeys logi_key = Devices.Logitech.LogitechDevice.ToLogitechBitmap(key);

                    if (logi_key != Devices.Logitech.Logitech_keyboardBitmapKeys.UNKNOWN && bitmap.Length > 0)
                    {
                        bitmap_layer.Set(key, GetBoostedColor(Utils.ColorUtils.GetColorFromInt(bitmap[(int)logi_key / 4])));
                    }
                }
            }

            layers.Enqueue(bitmap_layer);

            EffectLayer effects_layer = new EffectLayer("Aurora Wrapper - Effects");

            Devices.DeviceKeys[] effect_keys = key_effects.Keys.ToArray();
            long currentTime = Utils.Time.GetMillisecondsSinceEpoch();

            foreach (var key in effect_keys)
            {
                if (key_effects[key].duration != 0 && key_effects[key].timeStarted + key_effects[key].duration <= currentTime)
                {
                    key_effects.Remove(key);
                }
                else
                {
                    if (key_effects[key] is LogiFlashSingleKey)
                        effects_layer.Set((key_effects[key] as LogiFlashSingleKey).key, GetBoostedColor((key_effects[key] as LogiFlashSingleKey).GetColor(currentTime - (key_effects[key] as LogiFlashSingleKey).timeStarted)));
                    else if (key_effects[key] is LogiPulseSingleKey)
                        effects_layer.Set((key_effects[key] as LogiPulseSingleKey).key, GetBoostedColor((key_effects[key] as LogiPulseSingleKey).GetColor(currentTime - (key_effects[key] as LogiPulseSingleKey).timeStarted)));
                    else
                        effects_layer.Set((key_effects[key] as KeyEffect).key, GetBoostedColor((key_effects[key] as KeyEffect).GetColor(currentTime - (key_effects[key] as KeyEffect).timeStarted)));

                }
            }

            layers.Enqueue(effects_layer);

            EffectLayer entire_effect_layer = new EffectLayer("Aurora Wrapper - EntireKB effect");

            if (current_effect != null)
            {
                if (current_effect is LogiFlashLighting)
                    (current_effect as LogiFlashLighting).SetEffect(entire_effect_layer, currentTime - (current_effect as LogiFlashLighting).timeStarted);
                else if (current_effect is LogiPulseLighting)
                    (current_effect as LogiPulseLighting).SetEffect(entire_effect_layer, currentTime - (current_effect as LogiPulseLighting).timeStarted);
                else
                    current_effect.SetEffect(entire_effect_layer, currentTime - current_effect.timeStarted);
            }

            layers.Enqueue(entire_effect_layer);

            frame.AddLayers(layers.ToArray());
        }

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            UpdateWrapperLights(new_game_state);
            UpdateLights(frame);
        }

        internal virtual void UpdateWrapperLights(IGameState new_game_state)
        {
            if (new_game_state is GameState_Wrapper)
            {
                _game_state = new_game_state;

                GameState_Wrapper ngw_state = (new_game_state as GameState_Wrapper);

                    bitmap = ngw_state.Sent_Bitmap;
                    logo = ngw_state.Extra_Keys.logo;
                    g1 = ngw_state.Extra_Keys.G1;
                    g2 = ngw_state.Extra_Keys.G2;
                    g3 = ngw_state.Extra_Keys.G3;
                    g4 = ngw_state.Extra_Keys.G4;
                    g5 = ngw_state.Extra_Keys.G5;
                    peripheral = ngw_state.Extra_Keys.peripheral;

                if (ngw_state.Command.Equals("SetLighting"))
                {
                    Color newfill = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);

                    if (!last_fill_color.Equals(newfill))
                    {
                        last_fill_color = newfill;

                        for (int i = 0; i < bitmap.Length; i += 4)
                        {
                            bitmap[i] = (byte)ngw_state.Command_Data.blue_start;
                            bitmap[i + 1] = (byte)ngw_state.Command_Data.green_start;
                            bitmap[i + 2] = (byte)ngw_state.Command_Data.red_start;
                            bitmap[i + 3] = (byte)255;
                        }
                    }
                }
                else if (ngw_state.Command.Equals("SetLightingForKeyWithKeyName") || ngw_state.Command.Equals("SetLightingForKeyWithScanCode"))
                {
                    var bitmap_key = Devices.Logitech.LogitechDevice.ToLogitechBitmap((LedCSharp.keyboardNames)(ngw_state.Command_Data.key));

                    if (bitmap_key != Devices.Logitech.Logitech_keyboardBitmapKeys.UNKNOWN)
                    {
                        bitmap[(int)bitmap_key] = (byte)ngw_state.Command_Data.blue_start;
                        bitmap[(int)bitmap_key + 1] = (byte)ngw_state.Command_Data.green_start;
                        bitmap[(int)bitmap_key + 2] = (byte)ngw_state.Command_Data.red_start;
                        bitmap[(int)bitmap_key + 3] = (byte)255;
                    }
                }
                else if (ngw_state.Command.Equals("FlashSingleKey"))
                {
                    Devices.DeviceKeys dev_key = Devices.Logitech.LogitechDevice.ToDeviceKey((LedCSharp.keyboardNames)(ngw_state.Command_Data.key));
                    LogiFlashSingleKey neweffect = new LogiFlashSingleKey(dev_key, Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start),
                            ngw_state.Command_Data.duration,
                            ngw_state.Command_Data.interval
                            );

                    if (key_effects.ContainsKey(dev_key))
                        key_effects[dev_key] = neweffect;
                    else
                        key_effects.Add(dev_key, neweffect);

                }
                else if (ngw_state.Command.Equals("PulseSingleKey"))
                {
                    Devices.DeviceKeys dev_key = Devices.Logitech.LogitechDevice.ToDeviceKey((LedCSharp.keyboardNames)(ngw_state.Command_Data.key));
                    long duration = ngw_state.Command_Data.interval == 0 ? 0 : ngw_state.Command_Data.duration;

                    LogiPulseSingleKey neweffect = new LogiPulseSingleKey(dev_key, Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start),
                        Color.FromArgb(ngw_state.Command_Data.red_end, ngw_state.Command_Data.green_end, ngw_state.Command_Data.blue_end),
                            duration
                            );

                    if (key_effects.ContainsKey(dev_key))
                        key_effects[dev_key] = neweffect;
                    else
                        key_effects.Add(dev_key, neweffect);
                }
                else if (ngw_state.Command.Equals("PulseLighting"))
                {
                    current_effect = new LogiPulseLighting(
                        Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start),
                        ngw_state.Command_Data.duration,
                        ngw_state.Command_Data.interval
                        );
                }
                else if (ngw_state.Command.Equals("FlashLighting"))
                {
                    current_effect = new LogiFlashLighting(
                        Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start),
                        ngw_state.Command_Data.duration,
                        ngw_state.Command_Data.interval
                        );
                }
                else if (ngw_state.Command.Equals("StopEffects"))
                {
                    key_effects.Clear();
                    current_effect = null;
                }
                else if (ngw_state.Command.Equals("SetLightingFromBitmap"))
                {

                }
                //LightFX
                else if (ngw_state.Command.Equals("LFX_GetNumDevices"))
                {

                }
                else if (ngw_state.Command.Equals("LFX_Light"))
                {

                }
                else if (ngw_state.Command.Equals("LFX_SetLightColor"))
                {

                }
                else if (ngw_state.Command.Equals("LFX_Update"))
                {
                    Color newfill = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);

                    if (!last_fill_color.Equals(newfill))
                    {
                        last_fill_color = newfill;

                        for (int i = 0; i < bitmap.Length; i++)
                        {
                            bitmap[i] = (int)(((int)ngw_state.Command_Data.red_start << 16) | ((int)ngw_state.Command_Data.green_start << 8) | ((int)ngw_state.Command_Data.blue_start));
                        }

                        logo = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);
                        peripheral = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);
                    }
                }
                else if (ngw_state.Command.Equals("LFX_SetLightActionColor") || ngw_state.Command.Equals("LFX_ActionColor"))
                {
                    Color primary = Color.Transparent;
                    Color secondary = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);

                    if (current_effect != null)
                        primary = current_effect.GetCurrentColor(Utils.Time.GetMillisecondsSinceEpoch() - current_effect.timeStarted);

                    switch (ngw_state.Command_Data.effect_type)
                    {
                        case "LFX_ACTION_COLOR":
                            current_effect = new LFX_Color(primary);
                            break;
                        case "LFX_ACTION_PULSE":
                            current_effect = new LFX_Pulse(primary, secondary, ngw_state.Command_Data.duration);
                            break;
                        case "LFX_ACTION_MORPH":
                            current_effect = new LFX_Morph(primary, secondary, ngw_state.Command_Data.duration);
                            break;
                        default:
                            current_effect = null;
                            break;
                    }
                }
                else if (ngw_state.Command.Equals("LFX_SetLightActionColorEx") || ngw_state.Command.Equals("LFX_ActionColorEx"))
                {
                    Color primary = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);
                    Color secondary = Color.FromArgb(ngw_state.Command_Data.red_end, ngw_state.Command_Data.green_end, ngw_state.Command_Data.blue_end);

                    switch (ngw_state.Command_Data.effect_type)
                    {
                        case "LFX_ACTION_COLOR":
                            current_effect = new LFX_Color(primary);
                            break;
                        case "LFX_ACTION_PULSE":
                            current_effect = new LFX_Pulse(primary, secondary, ngw_state.Command_Data.duration);
                            break;
                        case "LFX_ACTION_MORPH":
                            current_effect = new LFX_Morph(primary, secondary, ngw_state.Command_Data.duration);
                            break;
                        default:
                            current_effect = null;
                            break;
                    }
                }
                else if (ngw_state.Command.Equals("LFX_Reset"))
                {
                    current_effect = null;
                }
                //Razer
                else if(ngw_state.Command.Equals("CreateMouseEffect"))
                {

                }
                else if (ngw_state.Command.Equals("CreateKeyboardEffect"))
                {
                    Color primary = Color.Red;
                    Color secondary = Color.Blue;

                    if (ngw_state.Command_Data.red_start >= 0 &&
                        ngw_state.Command_Data.green_start >= 0 &&
                        ngw_state.Command_Data.blue_start >= 0
                        )
                        primary = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);

                    if (ngw_state.Command_Data.red_end >= 0 &&
                        ngw_state.Command_Data.green_end >= 0 &&
                        ngw_state.Command_Data.blue_end >= 0
                        )
                        secondary = Color.FromArgb(ngw_state.Command_Data.red_end, ngw_state.Command_Data.green_end, ngw_state.Command_Data.blue_end);

                    switch (ngw_state.Command_Data.effect_type)
                    {
                        case "CHROMA_BREATHING":
                            current_effect = new CHROMA_BREATHING(primary, secondary, ngw_state.Command_Data.effect_config);
                            break;
                        default:
                            current_effect = null;
                            break;
                    }
                }
                else
                {
                    Global.logger.LogLine("Unknown Wrapper Command: " + ngw_state.Command, Logging_Level.Info, false);
                }
            }
        }

        public override bool IsEnabled()
        {
            return Global.Configuration.allow_all_logitech_bitmaps;
        }

        private Color GetBoostedColor(Color color)
        {
            if (!colorEnhance_Enabled)
                return color;

            switch (colorEnhance_Mode)
            {
                case 0:
                    float boost_amount = 0.0f;
                    boost_amount += (1.0f - (color.R / colorEnhance_color_factor));
                    boost_amount += (1.0f - (color.G / colorEnhance_color_factor));
                    boost_amount += (1.0f - (color.B / colorEnhance_color_factor));

                    boost_amount = boost_amount <= 1.0f ? 1.0f : boost_amount;

                    return Utils.ColorUtils.MultiplyColorByScalar(color, boost_amount);

                case 1:
                    float redComp = color.R * colorEnhance_color_simple;
                    float greenComp = color.G * colorEnhance_color_simple;
                    float blueComp = color.B * colorEnhance_color_simple;

                    float maxComp = Math.Max(Math.Max(redComp, greenComp), blueComp);
                    if (maxComp < 256)
                        return Color.FromArgb(color.A, (int)redComp, (int)greenComp, (int)blueComp);
                    float sumComp = redComp + greenComp + blueComp;
                    if (sumComp >= 768)
                        return Color.FromArgb(color.A, 255, 255, 255);
                    float x = (3 * 255.999f - sumComp) / (3 * maxComp - sumComp);
                    float gray = 255.999f - x * maxComp;

                    return Color.FromArgb(color.A, (int)(gray + x * redComp), (int)(gray + x * greenComp), (int)(gray + x * blueComp));

                case 2:
                    byte colorRed = (byte)Math.Min(255, (int)((255.0 * Math.Pow(color.R / 255.0, 1.0 / colorEnhance_color_gamma)) + 0.5));
                    byte colorGreen = (byte)Math.Min(255, (int)((255.0 * Math.Pow(color.G / 255.0, 1.0 / colorEnhance_color_gamma)) + 0.5));
                    byte colorBlue = (byte)Math.Min(255, (int)((255.0 * Math.Pow(color.B / 255.0, 1.0 / colorEnhance_color_gamma)) + 0.5));

                    return Color.FromArgb(color.A, colorRed, colorGreen, colorBlue);
                    
                default:
                    return color;
            }
            // initial_factor * (1 - (x / color_factor))
        }
    }

    class KeyEffect
    {
        public Devices.DeviceKeys key;
        public Color color;
        public long duration;
        public long interval;
        public long timeStarted;

        public KeyEffect(Devices.DeviceKeys key, Color color, long duration, long interval)
        {
            this.key = key;
            this.color = color;
            this.duration = duration;
            this.interval = interval;
            this.timeStarted = Utils.Time.GetMillisecondsSinceEpoch();
        }

        public virtual Color GetColor(long time)
        {
            return Color.Red; //Red for debug
        }
    }

    class LogiFlashSingleKey : KeyEffect
    {
        public LogiFlashSingleKey(Devices.DeviceKeys key, Color color, long duration, long interval) : base(key, color, duration, interval)
        {
        }

        public override Color GetColor(long time)
        {
            return Utils.ColorUtils.MultiplyColorByScalar(color, Math.Round(Math.Pow(Math.Sin((time / (double)interval) * Math.PI), 2.0)));
        }
    }

    class LogiPulseSingleKey : KeyEffect
    {
        public Color color_end;

        public LogiPulseSingleKey(Devices.DeviceKeys key, Color color, Color color_end, long duration) : base(key, color, duration, 1)
        {
            this.color_end = color_end;
        }

        public override Color GetColor(long time)
        {
            return Utils.ColorUtils.BlendColors(color, color_end, Math.Pow(Math.Sin((time / (double)(duration == 0 ? 1000.0D : duration)) * Math.PI), 2.0));
        }
    }

    class EntireEffect
    {
        public Color color;
        public long duration;
        public long interval;
        public long timeStarted;

        public EntireEffect(Color color, long duration, long interval)
        {
            this.color = color;
            this.duration = duration;
            this.interval = interval;
            this.timeStarted = Utils.Time.GetMillisecondsSinceEpoch();
        }

        public virtual Color GetCurrentColor(long time)
        {
            return color;
        }

        public void SetEffect(EffectLayer layer, long time)
        {
            layer.Fill(GetCurrentColor(time));
        }
    }

    class LogiFlashLighting : EntireEffect
    {
        public LogiFlashLighting(Color color, long duration, long interval) : base(color, duration, interval)
        {
        }

        public override Color GetCurrentColor(long time)
        {
            return Utils.ColorUtils.MultiplyColorByScalar(color, Math.Round(Math.Pow(Math.Sin((time / (double)interval) * Math.PI), 2.0)));
        }
    }

    class LogiPulseLighting : EntireEffect
    {
        public LogiPulseLighting(Color color, long duration, long interval) : base(color, duration, interval)
        {
        }

        public override Color GetCurrentColor(long time)
        {
            return Utils.ColorUtils.MultiplyColorByScalar(color, Math.Pow(Math.Sin((time / 1000.0D) * Math.PI), 2.0));
        }
    }

    /*
        if (lfx_actiontype == LFX_ActionType.Color)
                    lfx_action_layer.Fill(lfx_action_spectrum.GetColorAt(1.0f));
                else if(lfx_actiontype == LFX_ActionType.Morph)
                {

                }
                else if(lfx_actiontype == LFX_ActionType.Pulse)
                {
                    lfx_action_layer.Fill(
                        lfx_action_spectrum.GetColorAt((float)Math.Pow(Math.Sin((time / 1000.0D) * Math.PI), 2.0))
                    );

                        Utils.ColorUtils.MultiplyColorByScalar(color, Math.Pow(Math.Sin((time / 1000.0D) * Math.PI), 2.0))
                    );
                }
     */

    class LFX_Color : EntireEffect
    {
        public LFX_Color(Color color) : base(color, 0, 0)
        {
        }
    }

    class LFX_Pulse : EntireEffect
    {
        private Color secondary;

        public LFX_Pulse(Color primary, Color secondary, int duration) : base(primary, duration, 0)
        {
            this.secondary = secondary;
        }

        public override Color GetCurrentColor(long time)
        {
            return Utils.ColorUtils.MultiplyColorByScalar(color, Math.Pow(Math.Sin((time / 1000.0D) * Math.PI), 2.0));
        }
    }

    class LFX_Morph : EntireEffect
    {
        private Color secondary;

        public LFX_Morph(Color primary, Color secondary, int duration) : base(primary, duration, 0)
        {
            this.secondary = secondary;
        }

        public override Color GetCurrentColor(long time)
        {
            if (time - timeStarted >= duration)
                return secondary;
            else
                return Utils.ColorUtils.BlendColors(color, secondary, (time - timeStarted) % duration);
        }
    }

    class CHROMA_BREATHING : EntireEffect
    {
        private Color secondary;
        private enum BreathingType
        {
            TWO_COLORS,
            RANDOM_COLORS,
            INVALID
        }
        private BreathingType type;

        public CHROMA_BREATHING(Color primary, Color secondary, string config) : base(primary, 0, 0)
        {
            this.secondary = secondary;

            switch (config)
            {
                case "TWO_COLORS":
                    type = BreathingType.TWO_COLORS;
                    break;
                case "RANDOM_COLORS":
                    type = BreathingType.RANDOM_COLORS;
                    color = Utils.ColorUtils.GenerateRandomColor();
                    secondary = Utils.ColorUtils.GenerateRandomColor();
                    break;
                default:
                    type = BreathingType.INVALID;
                    break;
            }

        }

        public override Color GetCurrentColor(long time)
        {
            double blend_val = Math.Pow(Math.Sin((time / 1000.0D) * Math.PI), 2.0);

            if (type == BreathingType.RANDOM_COLORS)
            {
                if (blend_val >= 0.95 && blend_val >= 1.0)
                    color = Utils.ColorUtils.GenerateRandomColor();
                else if (blend_val >= 0.5 && blend_val >= 0.0)
                    secondary = Utils.ColorUtils.GenerateRandomColor();
            }

            return Utils.ColorUtils.BlendColors(color, secondary, blend_val);
        }
    }
}
