using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Dota_2
{
    public class Dota2Settings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Background
        public bool bg_team_enabled;
        public Color radiant_color;
        public Color dire_color;
        public Color ambient_color;
        public bool bg_enable_dimming;
        public int bg_dim_after;
        public bool bg_respawn_glow;
        public Color bg_respawn_glow_color;
        public bool bg_display_killstreaks;
        public bool bg_killstreaks_lines;
        public List<Color> bg_killstreakcolors;
        public bool bg_peripheral_use;

        //// Health
        public bool health_enabled;
        public Color healthy_color;
        public Color hurt_color;
        public PercentEffectType health_effect_type;
        public KeySequence health_sequence;
        public double health_blink_threshold;

        //// Mana
        public bool mana_enabled;
        public Color mana_color;
        public Color nomana_color;
        public PercentEffectType mana_effect_type;
        public KeySequence mana_sequence;
        public double mana_blink_threshold;

        public bool mimic_respawn_timer;
        public Color mimic_respawn_timer_color;
        public Color mimic_respawn_timer_respawning_color;

        ////Abilities
        public bool abilitykeys_enabled;
        public Color ability_can_use_color;
        public Color ability_can_not_use_color;
        public List<Devices.DeviceKeys> ability_keys { get; set; }

        ////Items
        public bool items_enabled;
        public Color items_empty_color;
        public Color items_on_cooldown_color;
        public Color items_no_charges_color;
        public Color items_color;
        public bool items_use_item_color;

        public List<Devices.DeviceKeys> items_keys { get; set; }

        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public Dota2Settings()
        {
            //General
            first_time_installed = false;
            IsEnabled = true;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                
                new Settings.Layers.Layer("Dota 2 Respawn", new Layers.Dota2RespawnLayerHandler()),
                new Settings.Layers.Layer("Health Indicator", new Settings.Layers.PercentLayerHandler()
                {
                    Properties = new Settings.Layers.PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 255, 0),
                        _SecondaryColor = Color.FromArgb(0, 60, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                            Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8,
                            Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Hero/Health",
                        _MaxVariablePath = "Hero/MaxHealth"
                    },
                })
                {
                    Logics = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.LogicItem>()
                    {
                    }
                },
                new Settings.Layers.Layer("Mana Indicator", new Settings.Layers.PercentLayerHandler()
                {
                    Properties = new Settings.Layers.PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 125, 255),
                        _SecondaryColor = Color.FromArgb(0, 0, 60),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR,
                            Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT,
                            Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Hero/Mana",
                        _MaxVariablePath = "Hero/MaxMana"
                    },
                })
                {
                    Logics = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.LogicItem>()
                    {
                    }
                },
                new Settings.Layers.Layer("Dota 2 Ability Keys", new Layers.Dota2AbilityLayerHandler()),
                new Settings.Layers.Layer("Dota 2 Item Keys", new Layers.Dota2ItemLayerHandler()),
                new Settings.Layers.Layer("Dota 2 Hero Ability Effects", new Layers.Dota2HeroAbilityEffectsLayerHandler()),
                new Settings.Layers.Layer("Dota 2 Killstreaks", new Layers.Dota2KillstreakLayerHandler()),
                new Settings.Layers.Layer("Dota 2 Background", new Layers.Dota2BackgroundLayerHandler())
            };

            //Effects
            //// Background
            bg_team_enabled = true;
            radiant_color = Color.FromArgb(0, 140, 30);
            dire_color = Color.FromArgb(200, 0, 0);
            ambient_color = Color.FromArgb(140, 190, 230);
            bg_enable_dimming = true;
            bg_dim_after = 15; //seconds
            bg_respawn_glow = true;
            bg_respawn_glow_color = Color.FromArgb(255, 255, 255);
            bg_display_killstreaks = true;
            bg_killstreaks_lines = true;
            bg_killstreakcolors = new List<Color>() {Color.FromArgb(0, 0, 0), //No Streak
                                            Color.FromArgb(0, 0, 0), //First kill
                                            Color.FromArgb(255, 255, 255), //Double Kill
                                            Color.FromArgb(0, 255, 0), //Killing Spree
                                            Color.FromArgb(128, 0, 255),  //Dominating
                                            Color.FromArgb(255, 100, 100),  //Mega Kill
                                            Color.FromArgb(255, 80, 0),  //Unstoppable
                                            Color.FromArgb(130, 180, 130),  //Wicked Sick
                                            Color.FromArgb(255, 0, 255),  //Monster Kill
                                            Color.FromArgb(255, 0, 0),  //Godlike
                                            Color.FromArgb(255, 80, 0)  //Godlike+
                                            };
            bg_peripheral_use = true;


            //// Health
            health_enabled = true;
            healthy_color = Color.FromArgb(0, 255, 0);
            hurt_color = Color.FromArgb(0, 60, 0);
            health_effect_type = PercentEffectType.Progressive_Gradual;
            health_sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12 });
            health_blink_threshold = 0.0D;

            //// Mana
            mana_enabled = true;
            mana_color = Color.FromArgb(0, 125, 255);
            nomana_color = Color.FromArgb(0, 0, 60);
            mana_effect_type = PercentEffectType.Progressive_Gradual;
            mana_sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS });
            mana_blink_threshold = 0.0D;

            mimic_respawn_timer = true;
            mimic_respawn_timer_color = Color.FromArgb(255, 0, 0);
            mimic_respawn_timer_respawning_color = Color.FromArgb(255, 170, 0);

            //// Abilities
            abilitykeys_enabled = true;
            ability_can_use_color = Color.FromArgb(0, 255, 0);
            ability_can_not_use_color = Color.FromArgb(255, 0, 0);
            ability_keys = new List<Devices.DeviceKeys>() { Devices.DeviceKeys.Q, Devices.DeviceKeys.W, Devices.DeviceKeys.E, Devices.DeviceKeys.D, Devices.DeviceKeys.F, Devices.DeviceKeys.R };

            //// Items
            items_enabled = true;
            items_empty_color = Color.FromArgb(0, 0, 0);
            items_on_cooldown_color = Color.FromArgb(0, 0, 0);
            items_no_charges_color = Color.FromArgb(150, 150, 150);
            items_color = Color.FromArgb(255, 255, 255);
            items_use_item_color = true;
            items_keys = new List<Devices.DeviceKeys>() { Devices.DeviceKeys.Z, Devices.DeviceKeys.X, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.B, Devices.DeviceKeys.N, Devices.DeviceKeys.INSERT, Devices.DeviceKeys.HOME, Devices.DeviceKeys.PAGE_UP, Devices.DeviceKeys.DELETE, Devices.DeviceKeys.END, Devices.DeviceKeys.PAGE_DOWN };

            // Lighting Areas
            lighting_areas = new List<ColorZone>();

        }
    }
}
