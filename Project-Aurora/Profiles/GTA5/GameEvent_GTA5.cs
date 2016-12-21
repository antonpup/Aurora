using System;
using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.GTA5.GSI;
using System.Drawing;
using Aurora.Settings;
using System.Linq;

namespace Aurora.Profiles.GTA5
{
    public class GameEvent_GTA5 : LightEvent
    {
        private static PlayerState curr_state = PlayerState.Undefined;
        private static bool have_cops = false;
        private Color statecol;
        private Color left_siren_color;
        private Color right_siren_color;
        private static int siren_keyframe = 0;
        private int special_mode = 0;

        public GameEvent_GTA5()
        {
            _game_state = new GameState_GTA5();
        }

        public static void SetCurrentState(PlayerState newstate)
        {
            curr_state = newstate;
        }

        public static void SetCopStatus(bool status)
        {
            have_cops = status;
        }

        public static void IncrementSirenKeyframe(int count = 1)
        {
            siren_keyframe += count;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            GTA5Settings settings = (GTA5Settings)this.Profile.Settings;

            if (settings.siren_enabled && have_cops)
            {
                EffectLayer sirens_layer = new EffectLayer("GTA 5 - Police Sirens");

                Color lefts = settings.left_siren_color;
                Color rights = settings.right_siren_color;

                KeySequence left_siren_ks = settings.left_siren_sequence;
                KeySequence right_siren_ks = settings.right_siren_sequence;

                //Switch sirens
                switch (settings.siren_type)
                {
                    case GTA5_PoliceEffects.Alt_Full:
                        switch (siren_keyframe % 2)
                        {
                            case 1:
                                rights = lefts;
                                break;
                            default:
                                lefts = rights;
                                break;
                        }
                        siren_keyframe = siren_keyframe % 2;

                        if (settings.siren_peripheral_use)
                            sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                        break;
                    case GTA5_PoliceEffects.Alt_Half:
                        switch (siren_keyframe % 2)
                        {
                            case 1:
                                rights = lefts;
                                lefts = Color.Black;

                                if (settings.siren_peripheral_use)
                                    sirens_layer.Set(Devices.DeviceKeys.Peripheral, rights);
                                break;
                            default:
                                lefts = rights;
                                rights = Color.Black;

                                if (settings.siren_peripheral_use)
                                    sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                break;
                        }
                        siren_keyframe = siren_keyframe % 2;
                        break;
                    case GTA5_PoliceEffects.Alt_Full_Blink:
                        switch (siren_keyframe % 4)
                        {
                            case 2:
                                rights = lefts;
                                break;
                            case 0:
                                lefts = rights;
                                break;
                            default:
                                lefts = Color.Black;
                                rights = Color.Black;
                                break;
                        }
                        siren_keyframe = siren_keyframe % 4;

                        if (settings.siren_peripheral_use)
                            sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                        break;
                    case GTA5_PoliceEffects.Alt_Half_Blink:
                        switch (siren_keyframe % 8)
                        {
                            case 6:
                                rights = lefts;
                                lefts = Color.Black;

                                if (settings.siren_peripheral_use)
                                    sirens_layer.Set(Devices.DeviceKeys.Peripheral, rights);
                                break;
                            case 4:
                                rights = lefts;
                                lefts = Color.Black;

                                if (settings.siren_peripheral_use)
                                    sirens_layer.Set(Devices.DeviceKeys.Peripheral, rights);
                                break;
                            case 2:
                                lefts = rights;
                                rights = Color.Black;

                                if (settings.siren_peripheral_use)
                                    sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                break;
                            case 0:
                                lefts = rights;
                                rights = Color.Black;

                                if (settings.siren_peripheral_use)
                                    sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                break;
                            default:
                                rights = Color.Black;
                                lefts = Color.Black;

                                if (settings.siren_peripheral_use)
                                    sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                break;
                        }
                        siren_keyframe = siren_keyframe % 8;
                        break;
                    default:
                        switch (siren_keyframe % 2)
                        {
                            case 1:
                                Color tempc = rights;
                                rights = lefts;
                                lefts = tempc;
                                break;
                            default:
                                break;
                        }
                        siren_keyframe = siren_keyframe % 2;

                        if (settings.siren_peripheral_use)
                            sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                        break;
                }

                sirens_layer.Set(left_siren_ks, lefts);
                sirens_layer.Set(right_siren_ks, rights);

                layers.Enqueue(sirens_layer);
            }

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("GTA 5 - Color Zones");
            cz_layer.DrawColorZones(settings.lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            EffectLayer debug_layer = new EffectLayer("GTA 5 - Debug");

            if (special_mode == 16711939)
            {
                debug_layer.Set(Devices.DeviceKeys.SPACE, Color.Red);
            }
            else if (special_mode == 16775939)
            {
                debug_layer.Set(Devices.DeviceKeys.SPACE, Color.Blue);
            }

            layers.Enqueue(debug_layer);

            //Scripts
            this.Profile.UpdateEffectScripts(layers, _game_state);

            foreach (var layer in settings.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            if (new_game_state is GameState_GTA5)
            {
                _game_state = new_game_state;

                GameState_GTA5 newgs = (new_game_state as GameState_GTA5);

                try
                {
                    curr_state = newgs.CurrentState;
                    statecol = newgs.StateColor;

                    have_cops = newgs.HasCops;

                    if (have_cops && left_siren_color != newgs.LeftSirenColor && right_siren_color != newgs.RightSirenColor)
                    {
                        siren_keyframe++;
                    }

                    left_siren_color = newgs.LeftSirenColor;
                    right_siren_color = newgs.RightSirenColor;

                    special_mode = newgs.Command_Data.custom_mode;


                }
                catch (Exception e)
                {
                    Global.logger.LogLine("Exception during OnNewGameState. Error: " + e, Logging_Level.Error);
                    Global.logger.LogLine(newgs.ToString(), Logging_Level.None);
                }

                UpdateLights(frame);
            }
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }
    }
}
