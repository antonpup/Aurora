using System;
using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Payday_2.GSI;
using System.Drawing;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using System.Linq;

namespace Aurora.Profiles.Payday_2
{
    public class GameEvent_PD2 : LightEvent
    {
        private static PlayerState player_state = PlayerState.Undefined;
        private static GameStates game_state = GameStates.Undefined;
        private static float health = 0.0f;
        private static float health_max = 1.0f;
        private static float armor = 0.0f;
        private static float armor_max = 1.0f;
        private static int clip = 0;
        private static int clip_max = 1;
        private static float suspicion_amount = 0.0f;
        private static LevelPhase level_phase = LevelPhase.Undefined;
        private static int down_time = 0;
        private static bool isSwanSong = false;
        private static int no_return_timeleft = 0;
        private static float no_return_flashamount = 1.0f;
        private static float flashbang_amount = 0.0f;

        private long lasttime = 0L;
        private long currenttime = 0L;

        private float assault_yoffset = 0.0f;

        public GameEvent_PD2()
        {
        }

        public static void SetHealth(float health)
        {
            GameEvent_PD2.health = health;
        }

        public static void SetHealthMax(float health_max)
        {
            GameEvent_PD2.health_max = health_max;
        }

        public static void SetArmor(float armor)
        {
            GameEvent_PD2.armor = armor;
        }

        public static void SetArmorMax(float armor_max)
        {
            GameEvent_PD2.armor_max = armor_max;
        }

        public static void SetClip(int clip)
        {
            GameEvent_PD2.clip = clip;
        }

        public static void SetClipMax(int clip_max)
        {
            GameEvent_PD2.clip_max = clip_max;
        }

        public static void SetSuspicion(float amount)
        {
            GameEvent_PD2.suspicion_amount = amount;
        }

        public static void SetLevelPhase(LevelPhase phase)
        {
            GameEvent_PD2.level_phase = phase;
        }

        public static void SetPlayerState(PlayerState state)
        {
            GameEvent_PD2.player_state = state;
        }

        public static void SetGameState(GameStates state)
        {
            GameEvent_PD2.game_state = state;
        }

        public static void SetDownTime(int downtime)
        {
            GameEvent_PD2.down_time = downtime;
        }

        public static void SetSwanSong(bool state)
        {
            GameEvent_PD2.isSwanSong = state;
        }

        public static void SetNoReturnTimeleft(int timeleft)
        {
            if (GameEvent_PD2.no_return_timeleft != timeleft)
            {
                no_return_flashamount = 1.0f;
            }

            GameEvent_PD2.no_return_timeleft = timeleft;
        }

        public static void SetFlashbangAmount(float flashamount)
        {
            GameEvent_PD2.flashbang_amount = flashamount;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            PD2Settings settings = (PD2Settings)this.Profile.Settings;

            //update background
            if (settings.bg_enabled)
            {
                EffectLayer bg_layer = new EffectLayer("Payday 2 - Background");

                Color bg_color = settings.ambient_color;

                if ((level_phase == LevelPhase.Assault || level_phase == LevelPhase.Winters) && game_state == GameStates.Ingame)
                {
                    if (level_phase == LevelPhase.Assault)
                        bg_color = settings.assault_color;
                    else if (level_phase == LevelPhase.Winters)
                        bg_color = settings.winters_color;

                    double blend_percent = Math.Pow(Math.Sin(((currenttime % 1300L) / 1300.0D) * settings.assault_speed_mult * 2.0D * Math.PI), 2.0D);

                    bg_color = Utils.ColorUtils.BlendColors(settings.assault_fade_color, bg_color, blend_percent);

                    if (settings.assault_animation_enabled)
                    {

                        Color effect_contours = Color.FromArgb(200, Color.Black);
                        float animation_stage_yoffset = 20.0f;
                        float animation_repeat_keyframes = 250.0f; //Effects.canvas_width * 2.0f;

                        /* Effect visual:

                        / /  ----  / /

                        */
                        
                        /*
                         * !!!NOTE: TO BE REWORKED INTO ANIMATIONS!!!

                        EffectColorFunction line1_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        EffectColorFunction line2_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 9.0f + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        EffectColorFunction line3_col_func = new EffectColorFunction(
                            new EffectLine(new EffectPoint(Effects.canvas_width + assault_yoffset + 17.0f + animation_stage_yoffset, Effects.canvas_height / 2.0f), new EffectPoint(Effects.canvas_width + assault_yoffset + 34.0f + animation_stage_yoffset, Effects.canvas_height / 2.0f), true),
                            new ColorSpectrum(effect_contours),
                            6);

                        EffectColorFunction line4_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 52.0f + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        EffectColorFunction line5_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 61.0f + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        assault_yoffset -= 0.50f;
                        assault_yoffset = assault_yoffset % animation_repeat_keyframes;

                        bg_layer.AddPostFunction(line1_col_func);
                        bg_layer.AddPostFunction(line2_col_func);
                        //bg_layer.AddPostFunction(line3_col_func);
                        bg_layer.AddPostFunction(line4_col_func);
                        bg_layer.AddPostFunction(line5_col_func);

                        */
                    }

                    bg_layer.Fill(bg_color);

                    if (settings.bg_peripheral_use)
                        bg_layer.Set(Devices.DeviceKeys.Peripheral, bg_color);
                }
                else if (level_phase == LevelPhase.Stealth && game_state == GameStates.Ingame)
                {
                    if (settings.bg_show_suspicion)
                    {
                        double percentSuspicious = ((double)suspicion_amount / (double)1.0);

                        ColorSpectrum suspicion_spec = new ColorSpectrum(settings.low_suspicion_color, settings.high_suspicion_color);
                        suspicion_spec.SetColorAt(0.5f, settings.medium_suspicion_color);

                        Settings.KeySequence suspicionSequence = new Settings.KeySequence(new Settings.FreeFormObject(0, 0, 1.0f / (Effects.editor_to_canvas_width / Effects.canvas_width), 1.0f / (Effects.editor_to_canvas_height / Effects.canvas_height)));

                        bg_layer.PercentEffect(suspicion_spec, suspicionSequence, percentSuspicious, 1.0D, settings.suspicion_effect_type);

                        if (settings.bg_peripheral_use)
                            bg_layer.Set(Devices.DeviceKeys.Peripheral, suspicion_spec.GetColorAt((float)percentSuspicious));
                    }
                }
                else if (level_phase == LevelPhase.Point_of_no_return && game_state == GameStates.Ingame)
                {
                    ColorSpectrum no_return_spec = new ColorSpectrum(Color.Red, Color.Yellow);

                    Color no_return_color = no_return_spec.GetColorAt(no_return_flashamount);
                    no_return_flashamount -= 0.05f;

                    if (no_return_flashamount < 0.0f)
                        no_return_flashamount = 0.0f;

                    bg_layer.Fill(no_return_color);

                    if (settings.bg_peripheral_use)
                        bg_layer.Set(Devices.DeviceKeys.Peripheral, no_return_color);
                }
                else
                {
                    bg_layer.Fill(bg_color);

                    if (settings.bg_peripheral_use)
                        bg_layer.Set(Devices.DeviceKeys.Peripheral, bg_color);
                }

                layers.Enqueue(bg_layer);
            }

            if (game_state == GameStates.Ingame)
            {

                if (
                    player_state == PlayerState.Mask_Off ||
                    player_state == PlayerState.Standard ||
                    player_state == PlayerState.Jerry1 ||
                    player_state == PlayerState.Jerry2 ||
                    player_state == PlayerState.Tased ||
                    player_state == PlayerState.Bipod ||
                    player_state == PlayerState.Carry
                )
                {
                    //Update Health
                    EffectLayer hpbar_layer = new EffectLayer("Payday 2 - HP Bar");
                    if (settings.health_enabled)
                        hpbar_layer.PercentEffect(settings.healthy_color,
                                settings.hurt_color,
                                settings.health_sequence,
                                (double)health,
                                (double)health_max,
                                settings.health_effect_type);

                    layers.Enqueue(hpbar_layer);

                    //Update Health
                    EffectLayer ammobar_layer = new EffectLayer("Payday 2 - Ammo Bar");
                    if (settings.ammo_enabled)
                        hpbar_layer.PercentEffect(settings.ammo_color,
                                settings.noammo_color,
                                settings.ammo_sequence,
                                (double)clip,
                                (double)clip_max,
                                settings.ammo_effect_type);

                    layers.Enqueue(ammobar_layer);
                }
                else if (player_state == PlayerState.Incapacitated || player_state == PlayerState.Bleed_out || player_state == PlayerState.Fatal)
                {
                    int incapAlpha = (int)(down_time > 10 ? 0 : 255 * (1.0D - (double)down_time / 10.0D));

                    if (incapAlpha > 255)
                        incapAlpha = 255;
                    else if (incapAlpha < 0)
                        incapAlpha = 0;

                    Color incapColor = Color.FromArgb(incapAlpha, settings.downed_color);
                    EffectLayer incap_layer = new EffectLayer("Payday 2 - Incapacitated", incapColor);
                    incap_layer.Set(Devices.DeviceKeys.Peripheral, incapColor);

                    layers.Enqueue(incap_layer);
                }
                else if (player_state == PlayerState.Arrested)
                {
                    Color arrstedColor = settings.arrested_color;
                    EffectLayer arrested_layer = new EffectLayer("Payday 2 - Arrested", arrstedColor);
                    arrested_layer.Set(Devices.DeviceKeys.Peripheral, arrstedColor);

                    layers.Enqueue(arrested_layer);
                }

                if (isSwanSong && settings.swansong_enabled)
                {
                    EffectLayer swansong_layer = new EffectLayer("Payday 2 - Swansong");

                    double blend_percent = Math.Pow(Math.Cos((currenttime % 1300L) / 1300.0D * settings.swansong_speed_mult * 2.0D * Math.PI), 2.0D);

                    Color swansongColor = Utils.ColorUtils.MultiplyColorByScalar(settings.swansong_color, blend_percent);

                    swansong_layer.Fill(swansongColor);

                    if (settings.bg_peripheral_use)
                        swansong_layer.Set(Devices.DeviceKeys.Peripheral, swansongColor);

                    layers.Enqueue(swansong_layer);
                }

                //Update Flashed
                if (flashbang_amount > 0)
                {
                    EffectLayer flashed_layer = new EffectLayer("Payday 2 - Flashed");

                    Color flash_color = Utils.ColorUtils.MultiplyColorByScalar(Color.White, flashbang_amount);

                    flashed_layer.Fill(flash_color);

                    layers.Enqueue(flashed_layer);
                }
            }

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Payday 2 - Color Zones");
            cz_layer.DrawColorZones(settings.lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            this.Profile.UpdateEffectScripts(layers, _game_state);

            foreach (var layer in settings.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());

            lasttime = currenttime;
        }

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            if (new_game_state is GameState_PD2)
            {
                _game_state = new_game_state;

                GameState_PD2 gs = (new_game_state as GameState_PD2);

                try
                {
                    SetPlayerState(gs.Players.LocalPlayer.State);
                    SetHealth(gs.Players.LocalPlayer.Health.Current);
                    SetHealthMax(gs.Players.LocalPlayer.Health.Max);
                    SetArmor(gs.Players.LocalPlayer.Armor.Current);
                    SetArmorMax(gs.Players.LocalPlayer.Armor.Max);
                    SetClip(gs.Players.LocalPlayer.Weapons.SelectedWeapon.Current_Clip);
                    SetClipMax(gs.Players.LocalPlayer.Weapons.SelectedWeapon.Max_Clip);
                    SetSuspicion(gs.Players.LocalPlayer.SuspicionAmount);
                    SetLevelPhase(gs.Level.Phase);
                    SetDownTime(gs.Players.LocalPlayer.DownTime);
                    SetSwanSong(gs.Players.LocalPlayer.IsSwanSong);
                    SetFlashbangAmount(gs.Players.LocalPlayer.FlashAmount);
                    SetNoReturnTimeleft(gs.Level.NoReturnTime);
                    SetGameState(gs.Game.State);

                    UpdateLights(frame);
                }
                catch (Exception e)
                {
                    Global.logger.LogLine("Exception during OnNewGameState. Error: " + e, Logging_Level.Error);
                    Global.logger.LogLine(gs.ToString(), Logging_Level.None);
                }
            }
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }
    }
}
