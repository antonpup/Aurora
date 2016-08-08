using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.EffectsEngine;
using System.Drawing;

namespace Aurora.Profiles.Aurora_Wrapper
{
    public class GameEvent_Aurora_Wrapper : LightEvent
    {
        internal byte[] bitmap = new byte[LedCSharp.LogitechGSDK.LOGI_LED_BITMAP_SIZE];
        internal int[] logo = new int[4];
        internal int[] peripheral = new int[4];
        internal int[] g1 = new int[4];
        internal int[] g2 = new int[4];
        internal int[] g3 = new int[4];
        internal int[] g4 = new int[4];
        internal int[] g5 = new int[4];
        internal Color last_fill_color = Color.Black;
        internal Dictionary<Devices.DeviceKeys, KeyEffect> key_effects = new Dictionary<Devices.DeviceKeys, KeyEffect>();
        internal EntireEffect current_effect = null;

        internal Dictionary<Devices.DeviceKeys, Color> colors = new Dictionary<Devices.DeviceKeys, Color>();

        public override void UpdateLights(EffectFrame frame)
        {
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //Scripts
            if(!String.IsNullOrWhiteSpace(profilename))
                Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            frame.AddLayers(layers.ToArray());
        }

        internal virtual void UpdateWrapperLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer colorfill_layer = new EffectLayer("Aurora Wrapper - Color Fill", last_fill_color);

            layers.Enqueue(colorfill_layer);

            EffectLayer bitmap_layer = new EffectLayer("Aurora Wrapper - Bitmap");

            Devices.DeviceKeys[] allkeys = Enum.GetValues(typeof(Devices.DeviceKeys)).Cast<Devices.DeviceKeys>().ToArray();
            foreach (var key in allkeys)
            {
                if (key == Devices.DeviceKeys.LOGO && logo.Length == 4)
                    bitmap_layer.Set(key, Color.FromArgb(logo[3], logo[2], logo[1], logo[0]));
                else if (key == Devices.DeviceKeys.Peripheral && peripheral.Length == 4)
                    bitmap_layer.Set(key, Color.FromArgb(peripheral[3], peripheral[2], peripheral[1], peripheral[0]));
                else if (key == Devices.DeviceKeys.G1 && g1.Length == 4)
                    bitmap_layer.Set(key, Color.FromArgb(g1[3], g1[2], g1[1], g1[0]));
                else if (key == Devices.DeviceKeys.G2 && g2.Length == 4)
                    bitmap_layer.Set(key, Color.FromArgb(g2[3], g2[2], g2[1], g2[0]));
                else if (key == Devices.DeviceKeys.G3 && g3.Length == 4)
                    bitmap_layer.Set(key, Color.FromArgb(g3[3], g3[2], g3[1], g3[0]));
                else if (key == Devices.DeviceKeys.G4 && g4.Length == 4)
                    bitmap_layer.Set(key, Color.FromArgb(g4[3], g4[2], g4[1], g4[0]));
                else if (key == Devices.DeviceKeys.G5 && g5.Length == 4)
                    bitmap_layer.Set(key, Color.FromArgb(g5[3], g5[2], g5[1], g5[0]));
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

                        bitmap_layer.Set(key, Color.FromArgb(a, r, g, b));
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
                        effects_layer.Set((key_effects[key] as LogiFlashSingleKey).key, (key_effects[key] as LogiFlashSingleKey).GetColor(currentTime - (key_effects[key] as LogiFlashSingleKey).timeStarted));
                    else if (key_effects[key] is LogiPulseSingleKey)
                        effects_layer.Set((key_effects[key] as LogiPulseSingleKey).key, (key_effects[key] as LogiPulseSingleKey).GetColor(currentTime - (key_effects[key] as LogiPulseSingleKey).timeStarted));
                    else
                        effects_layer.Set((key_effects[key] as KeyEffect).key, (key_effects[key] as KeyEffect).GetColor(currentTime - (key_effects[key] as KeyEffect).timeStarted));

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

        public override void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            UpdateWrapperLights(new_game_state);
            UpdateLights(frame);
        }

        internal virtual void UpdateWrapperLights(GameState new_game_state)
        {
            if (new_game_state is GameState_Wrapper)
            {
                _game_state = new_game_state;

                GameState_Wrapper ngw_state = (new_game_state as GameState_Wrapper);

                if (ngw_state.Sent_Bitmap.Length > 0)
                    bitmap = ngw_state.Sent_Bitmap;
                if (ngw_state.Extra_Keys.logo.Length > 0)
                    logo = ngw_state.Extra_Keys.logo;
                if (ngw_state.Extra_Keys.G1.Length > 0)
                    g1 = ngw_state.Extra_Keys.G1;
                if (ngw_state.Extra_Keys.G2.Length > 0)
                    g2 = ngw_state.Extra_Keys.G2;
                if (ngw_state.Extra_Keys.G3.Length > 0)
                    g3 = ngw_state.Extra_Keys.G3;
                if (ngw_state.Extra_Keys.G4.Length > 0)
                    g4 = ngw_state.Extra_Keys.G4;
                if (ngw_state.Extra_Keys.G5.Length > 0)
                    g5 = ngw_state.Extra_Keys.G5;
                if (ngw_state.Extra_Keys.peripheral.Length > 0)
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
                else if (ngw_state.Command.Equals("SetLightingForKeyWithKeyName"))
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

                        for (int i = 0; i < bitmap.Length; i += 4)
                        {
                            bitmap[i] = (byte)ngw_state.Command_Data.blue_start;
                            bitmap[i + 1] = (byte)ngw_state.Command_Data.green_start;
                            bitmap[i + 2] = (byte)ngw_state.Command_Data.red_start;
                            bitmap[i + 3] = (byte)255;
                        }

                        if (logo.Length == 4)
                        {
                            logo[0] = (byte)ngw_state.Command_Data.blue_start;
                            logo[1] = (byte)ngw_state.Command_Data.green_start;
                            logo[2] = (byte)ngw_state.Command_Data.red_start;
                            logo[3] = (byte)255;
                        }

                        if (peripheral.Length == 4)
                        {
                            peripheral[0] = (byte)ngw_state.Command_Data.blue_start;
                            peripheral[1] = (byte)ngw_state.Command_Data.green_start;
                            peripheral[2] = (byte)ngw_state.Command_Data.red_start;
                            peripheral[3] = (byte)255;
                        }
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
