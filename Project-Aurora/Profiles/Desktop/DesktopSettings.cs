using Aurora.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Aurora.Profiles.Desktop
{
    public enum InteractiveEffects
    {
        [Description("None")]
        None = 0,
        [Description("Key Wave")]
        Wave = 1,
        [Description("Key Wave (Filled)")]
        Wave_Filled = 3,
        [Description("Key Fade")]
        KeyPress = 2,
        [Description("Arrow Flow")]
        ArrowFlow = 4,
        [Description("Key Wave (Rainbow)")]
        Wave_Rainbow = 5,
    }

    public class DesktopSettings : ProfileSettings
    {
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

        //// Shortcut Assistant
        public bool shortcuts_assistant_enabled;
        public bool shortcuts_assistant_bim_bg;
        public Color ctrl_key_color;
        public KeySequence ctrl_key_sequence;
        public Color win_key_color;
        public KeySequence win_key_sequence;
        public Color alt_key_color;
        public KeySequence alt_key_sequence;

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

        public DesktopSettings()
        {
            IsEnabled = true;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                new Settings.Layers.Layer("Ctrl Shortcuts", new Settings.Layers.ShortcutAssistantLayerHandler()
                {
                    Properties = new Settings.Layers.ShortcutAssistantLayerHandlerProperties(System.Windows.Forms.Keys.LControlKey)
                }),
                new Settings.Layers.Layer("Win Shortcuts", new Settings.Layers.ShortcutAssistantLayerHandler()
                {
                    Properties = new Settings.Layers.ShortcutAssistantLayerHandlerProperties(System.Windows.Forms.Keys.LWin)
                }),
                new Settings.Layers.Layer("Alt Shortcuts", new Settings.Layers.ShortcutAssistantLayerHandler()
                {
                    Properties = new Settings.Layers.ShortcutAssistantLayerHandlerProperties(System.Windows.Forms.Keys.LMenu)
                }),
                new Settings.Layers.Layer("CPU Usage", new Settings.Layers.PercentLayerHandler()
                {
                    Properties = new Settings.Layers.PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 205, 255),
                        _SecondaryColor = Color.FromArgb(0, 65, 80),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                            Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8,
                            Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "LocalPCInfo/CPUUsage",
                        _MaxVariablePath = "100"
                    },
                }),
                new Settings.Layers.Layer("RAM Usage", new Settings.Layers.PercentLayerHandler()
                {
                    Properties = new Settings.Layers.PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(255, 80, 0),
                        _SecondaryColor = Color.FromArgb(90, 30, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR,
                            Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT,
                            Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "LocalPCInfo/MemoryUsed",
                        _MaxVariablePath = "LocalPCInfo/MemoryTotal"
                    },
                }),
                new Settings.Layers.Layer("Interactive Layer", new Settings.Layers.InteractiveLayerHandler()
                {
                    Properties = new Settings.Layers.InteractiveLayerHandlerProperties()
                    {
                        _InteractiveEffect = InteractiveEffects.Wave_Filled,
                        _PrimaryColor = Color.FromArgb(0, 255, 0),
                        _RandomPrimaryColor = true,
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _RandomSecondaryColor = true,
                        _EffectSpeed = 5.0f,
                        _EffectWidth = 2
                    }
                }
                )
            };

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

            //// Shortcuts Assistant
            shortcuts_assistant_enabled = true;
            shortcuts_assistant_bim_bg = false;
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
        }
    }
}