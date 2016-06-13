using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using System.Drawing;

namespace Aurora.Profiles.Logitech_Wrapper
{
    public class GameEvent_Logitech_Wrapper : GameEvent
    {
        object lock_updater;

        byte[] bitmap = new byte[LedCSharp.LogitechGSDK.LOGI_LED_BITMAP_SIZE];
        int[] logo = new int[4];
        Color last_fill_color = Color.Transparent;
        Dictionary<Devices.DeviceKeys, KeyEffect> key_effects = new Dictionary<Devices.DeviceKeys, KeyEffect>();
        EntireEffect current_effect = null;

        Dictionary<Devices.DeviceKeys, Color> colors = new Dictionary<Devices.DeviceKeys, Color>();

        public void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer bitmap_layer = new EffectLayer("Logitech Wrapper - Bitmap");

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

                    bitmap_layer.Set(key, Color.FromArgb(a, r, g, b));
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

                        bitmap_layer.Set(key, Color.FromArgb(a, r, g, b));
                    }
                }
            }

            layers.Enqueue(bitmap_layer);

            EffectLayer effects_layer = new EffectLayer("Logitech Wrapper - Effects");

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

            EffectLayer entire_effect_layer = new EffectLayer("Logitech Wrapper - EntireKB effect");

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


            frame.SetLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            if (new_game_state is GameState_Wrapper)
            {
                GameState_Wrapper ngw_state = (new_game_state as GameState_Wrapper);

                if(ngw_state.Sent_Bitmap.Length > 0)
                    bitmap = ngw_state.Sent_Bitmap;
                logo = ngw_state.Extra_Keys.logo;


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

                    if(bitmap_key != Devices.Logitech.Logitech_keyboardBitmapKeys.UNKNOWN)
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
                else if(ngw_state.Command.Equals("SetLightingFromBitmap"))
                {

                }
                //LightFX
                if (ngw_state.Command.Equals("LFX_Update"))
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
                //Razer
                else if (ngw_state.Command.Equals("CreateKeyboardEffect"))
                {

                }
                else
                {
                    Global.logger.LogLine("Unknown Wrapper Command: " + ngw_state.Command, Logging_Level.Info, false);
                }
            }

            UpdateLights(frame);
        }

        public bool IsEnabled()
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

        public virtual void SetEffect(EffectLayer layer, long time)
        {
            layer.Fill(Color.Red);
        }
    }

    class LogiFlashLighting : EntireEffect
    {
        public LogiFlashLighting(Color color, long duration, long interval) : base(color, duration, interval)
        {
        }

        public override void SetEffect(EffectLayer layer, long time)
        {
            layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(color, Math.Round(Math.Pow(Math.Sin((time / (double)interval) * Math.PI), 2.0))));
        }
    }

    class LogiPulseLighting : EntireEffect
    {
        public LogiPulseLighting(Color color, long duration, long interval) : base(color, duration, interval)
        {
        }

        public override void SetEffect(EffectLayer layer, long time)
        {
            layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(color, Math.Pow(Math.Sin((time / 1000.0D) * Math.PI), 2.0)));
        }
    }

}
