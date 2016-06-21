using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Desktop
{
    public enum InteractiveEffects
    {
        [Description("None")]
        None = 0,
        [Description("Key Wave Effect")]
        Wave = 1,
        [Description("Key Fade")]
        KeyPress = 2
    }

    public enum IdleEffects
    {
        [Description("None")]
        None = 0,
        [Description("Dim")]
        Dim = 1,
        [Description("Color Breathing")]
        ColorBreathing = 2,
        [Description("Rainbow Shift (Horizontal)")]
        RainbowShift_Horizontal = 3,
        [Description("Rainbow Shift (Vertical)")]
        RainbowShift_Vertical = 4,
        [Description("Star Fall")]
        StarFall = 5,
        [Description("Rain Fall")]
        RainFall = 6,
    }

    public class DesktopSettings : ProfileSettings
    {
        public bool time_based_dimming_enabled;
        public bool time_based_dimming_affect_games;
        public int time_based_dimming_start_hour;
        public int time_based_dimming_start_minute;
        public int time_based_dimming_end_hour;
        public int time_based_dimming_end_minute;

        public bool nighttime_enabled;
        public int nighttime_start_hour;
        public int nighttime_start_minute;
        public int nighttime_end_hour;
        public int nighttime_end_minute;

        //Effects
        ////CPU
        public bool cpu_usage_enabled;
        public Color cpu_used_color;
        public Color cpu_free_color;
        public bool cpu_free_color_transparent;
        public PercentEffectType cpu_usage_effect_type;
        public KeySequence cpu_sequence;

        ////Ram
        public bool ram_usage_enabled;
        public Color ram_used_color;
        public Color ram_free_color;
        public bool ram_free_color_transparent;
        public PercentEffectType ram_usage_effect_type;
        public KeySequence ram_sequence;

        //// Lighting Areas
        public List<ColorZone> lighting_areas;

        //// Interactive Effects
        public bool interactive_effects_enabled;
        public InteractiveEffects interactive_effect_type;
        public bool interactive_effects_random_primary_color;
        public Color interactive_effect_primary_color;
        public bool interactive_effects_random_secondary_color;
        public Color interactive_effect_secondary_color;
        public float interactive_effect_speed;
        public int interactive_effect_width;
        public bool interactive_effects_mouse_clicking;

        //// Shortcut Assistant
        public bool shortcuts_assistant_enabled;
        public Color ctrl_key_color;
        public KeySequence ctrl_key_sequence;
        public Color win_key_color;
        public KeySequence win_key_sequence;
        public Color alt_key_color;
        public KeySequence alt_key_sequence;

        //// Idle Effects
        public IdleEffects idle_type;
        public int idle_delay;
        public float idle_speed;
        public Color idle_effect_primary_color;
        public Color idle_effect_secondary_color;
        public int idle_amount;
        public float idle_frequency;


        public DesktopSettings()
        {
            isEnabled = true;

            time_based_dimming_enabled = false;
            time_based_dimming_affect_games = false;
            time_based_dimming_start_hour = 21;
            time_based_dimming_start_minute = 0;
            time_based_dimming_end_hour = 8;
            time_based_dimming_end_minute = 0;

            nighttime_enabled = true;
            nighttime_start_hour = 20;
            nighttime_start_minute = 0;
            nighttime_end_hour = 7;
            nighttime_end_minute = 0;

            //Effects
            //// CPU
            cpu_usage_enabled = true;
            cpu_used_color = Color.FromArgb(0, 205, 255);
            cpu_free_color = Color.FromArgb(0, 65, 80);
            cpu_free_color_transparent = false;
            cpu_usage_effect_type = PercentEffectType.Progressive_Gradual;
            cpu_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8,
                Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
            });

            //// Ram
            ram_usage_enabled = true;
            ram_used_color = Color.FromArgb(255, 80, 0);
            ram_free_color = Color.FromArgb(90, 30, 0);
            ram_free_color_transparent = false;
            ram_usage_effect_type = PercentEffectType.Progressive_Gradual;
            ram_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR,
                Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT,
                Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
            });

            //// Lighting Areas
            lighting_areas = new List<ColorZone>();

            //// Interactive Effects
            interactive_effects_enabled = false;
            interactive_effect_type = InteractiveEffects.None;
            interactive_effect_primary_color = Color.FromArgb(0, 255, 0);
            interactive_effects_random_primary_color = false;
            interactive_effect_secondary_color = Color.FromArgb(255, 0, 0);
            interactive_effects_random_secondary_color = false;
            interactive_effect_speed = 1.0f;
            interactive_effect_width = 2;
            interactive_effects_mouse_clicking = false;

            //// Shortcuts Assistant
            shortcuts_assistant_enabled = true;
            ctrl_key_color = Color.Red;
            ctrl_key_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.X, Devices.DeviceKeys.Y,
                Devices.DeviceKeys.LEFT_ALT, Devices.DeviceKeys.RIGHT_ALT, Devices.DeviceKeys.A, Devices.DeviceKeys.Z
            });
            win_key_color = Color.Blue;
            win_key_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.R, Devices.DeviceKeys.E, Devices.DeviceKeys.M, Devices.DeviceKeys.D,
                Devices.DeviceKeys.ARROW_UP, Devices.DeviceKeys.ARROW_DOWN, Devices.DeviceKeys.ARROW_LEFT, Devices.DeviceKeys.ARROW_RIGHT,
                Devices.DeviceKeys.TAB
            });
            alt_key_color = Color.Yellow;
            alt_key_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F4, Devices.DeviceKeys.E, Devices.DeviceKeys.V, Devices.DeviceKeys.LEFT_CONTROL,
                Devices.DeviceKeys.RIGHT_CONTROL, Devices.DeviceKeys.TAB
            });

            //// Idle Effects
            idle_type = IdleEffects.None;
            idle_delay = 5;
            idle_speed = 1.0f;
            idle_effect_primary_color = Color.FromArgb(0, 255, 0);
            idle_effect_secondary_color = Color.FromArgb(0, 0, 0);
            idle_amount = 5;
            idle_frequency = 2.5f;
        }
    }
}