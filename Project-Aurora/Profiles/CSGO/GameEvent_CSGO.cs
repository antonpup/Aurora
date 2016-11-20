using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO
{
    enum RoundKillType
    {
        None,
        Regular,
        Headshot
    };

    public class GameEvent_CSGO : LightEvent
    {
        private int lastUpdate = 0;
        private int updateRate = 1; //1 second
        private static Dictionary<Devices.DeviceKeys, EffectColor> keyColors = new Dictionary<Devices.DeviceKeys, EffectColor>();
        private static Dictionary<Devices.DeviceKeys, EffectColor> final_keyColors = new Dictionary<Devices.DeviceKeys, EffectColor>();

        private static Stopwatch general_timer = Stopwatch.StartNew();
        private static Random randomizer = new Random();

        private static bool isDimming = false;
        private static double dim_value = 1.0;
        private static long dim_bg_at = 5000;

        //Bomb stuff
        private bool IsPlanted = false;
        private Stopwatch bombtimer = new Stopwatch();
        private bool bombflash = false;
        private int bombflashcount = 0;
        private long bombflashtime = 0;
        private long bombflashedat = 0;

        //Game Integration stuff
        private static PlayerTeam current_team = PlayerTeam.Undefined;
        private static int health = 0;
        private static int health_max = 100;
        private static int clip = 0;
        private static int clip_max = 100;
        private static BombState bombstate = BombState.Undefined;
        private static int flashamount = 0;
        private static int burnamount = 0;
        private static PlayerActivity current_activity = PlayerActivity.Undefined;
        private static bool isLocal = true;
        private static List<RoundKillType> roundKills = new List<RoundKillType>();

        public GameEvent_CSGO()
        {
            profilename = "CSGO";
            _game_state = new GameState_CSGO();
        }

        public static void SetTeam(PlayerTeam team)
        {
            current_team = team;
        }

        public static void SetHealth(int health)
        {
            GameEvent_CSGO.health = health;
        }

        public static void SetClip(int clip)
        {
            GameEvent_CSGO.clip = clip;
        }

        public static void SetClipMax(int clipmax)
        {
            clip_max = clipmax;
        }

        public static void SetBombState(BombState state)
        {
            GameEvent_CSGO.bombstate = state;
        }

        public static void SetFlashAmount(int flash)
        {
            GameEvent_CSGO.flashamount = flash;
        }

        public static void SetBurnAmount(int burn)
        {
            GameEvent_CSGO.burnamount = burn;
        }

        public static void SetPlayerActivity(PlayerActivity activity)
        {
            GameEvent_CSGO.current_activity = activity;
        }

        public static void SetIsLocalPlayer(bool islocal)
        {
            GameEvent_CSGO.isLocal = islocal;
        }

        public static void GotAKill(bool isHS = false)
        {
            if (isHS)
                GameEvent_CSGO.roundKills.Add(RoundKillType.Headshot);
            else
                GameEvent_CSGO.roundKills.Add(RoundKillType.Regular);
        }

        public static void RoundStart()
        {
            GameEvent_CSGO.roundKills.Clear();
        }

        public static void Respawned()
        {
            isDimming = false;
            dim_bg_at = general_timer.ElapsedMilliseconds + ((Global.Configuration.ApplicationProfiles["CSGO"].Settings as CSGOSettings).bg_dim_after * 1000L);
            dim_value = 1.0;

            roundKills.Clear();
        }

        private double getDimmingValue()
        {
            if (isDimming && (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bg_enable_dimming)
            {
                dim_value -= 0.02;
                return dim_value = (dim_value < 0.0 ? 0.0 : dim_value);
            }
            else
                return dim_value = 1.0;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //update background
            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bg_team_enabled)
            {
                EffectLayer bg_layer = new EffectLayer("CSGO - Background");

                Color bg_color = (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ambient_color;

                if (current_team == PlayerTeam.T)
                    bg_color = (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).t_color;
                else if (current_team == PlayerTeam.CT)
                    bg_color = (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ct_color;

                if (current_team != PlayerTeam.Undefined)
                {
                    if (dim_bg_at <= general_timer.ElapsedMilliseconds)
                    {
                        isDimming = true;
                        bg_color = Utils.ColorUtils.MultiplyColorByScalar(bg_color, getDimmingValue());
                    }
                    else
                    {
                        isDimming = false;
                        dim_value = 1.0;
                    }
                }

                bg_layer.Fill(bg_color);

                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bg_peripheral_use)
                    bg_layer.Set(Devices.DeviceKeys.Peripheral, bg_color);
                layers.Enqueue(bg_layer);
            }

            //Not initialized
            if (current_team != PlayerTeam.Undefined)
            {
                //Update Health
                EffectLayer hpbar_layer = new EffectLayer("CSGO - HP Bar");
                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).health_enabled)
                    hpbar_layer.PercentEffect((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).healthy_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).hurt_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).health_sequence,
                            (double)health,
                            (double)health_max,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).health_effect_type);

                layers.Enqueue(hpbar_layer);

                //Update Ammo
                EffectLayer ammobar_layer = new EffectLayer("CSGO - Ammo Bar");
                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ammo_enabled)
                    ammobar_layer.PercentEffect((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ammo_color,
                        (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).noammo_color,
                        (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ammo_sequence,
                        (double)clip,
                        (double)clip_max,
                        (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ammo_effect_type);

                layers.Enqueue(ammobar_layer);


                //Update Bomb
                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_enabled)
                {
                    EffectLayer bomb_effect_layer = new EffectLayer("CSGO - Bomb Effect");

                    Devices.DeviceKeys[] _bombkeys = (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_sequence.keys.ToArray();

                    if (bombstate == BombState.Planted)
                    {
                        if (!bombtimer.IsRunning)
                        {
                            bombtimer.Restart();
                            bombflashcount = 0;
                            bombflashtime = 0;
                            bombflashedat = 0;
                        }

                        double bombflashamount = 1.0;
                        bool isCritical = false;


                        if (bombtimer.ElapsedMilliseconds < 38000)
                        {

                            if (bombtimer.ElapsedMilliseconds >= bombflashtime)
                            {
                                bombflash = true;
                                bombflashedat = bombtimer.ElapsedMilliseconds;
                                bombflashtime = bombtimer.ElapsedMilliseconds + (1000 - (bombflashcount++ * 13));
                            }

                            bombflashamount = Math.Pow(Math.Sin((bombtimer.ElapsedMilliseconds - bombflashedat) / 80.0 + 0.25), 2.0);

                        }
                        else if (bombtimer.ElapsedMilliseconds >= 38000)
                        {
                            isCritical = true;
                            bombflashamount = (double)bombtimer.ElapsedMilliseconds / 40000.0;
                        }
                        else if (bombtimer.ElapsedMilliseconds >= 45000)
                        {
                            bombtimer.Stop();
                            bombstate = BombState.Undefined;
                        }

                        if (!isCritical)
                        {
                            if (bombflashamount <= 0.05 && bombflash)
                                bombflash = false;

                            if (!bombflash)
                                bombflashamount = 0.0;
                        }

                        if (!(Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_gradual)
                            bombflashamount = Math.Round(bombflashamount);

                        foreach (Devices.DeviceKeys key in _bombkeys)
                        {
                            if (isCritical)
                            {
                                Color bombcolor_critical = Utils.ColorUtils.MultiplyColorByScalar((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_primed_color, Math.Min(bombflashamount, 1.0));

                                bomb_effect_layer.Set(key, bombcolor_critical);

                                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_peripheral_use)
                                {
                                    bomb_effect_layer.Set(Devices.DeviceKeys.Peripheral, bombcolor_critical);
                                }
                            }
                            else
                            {
                                Color bombcolor = Utils.ColorUtils.MultiplyColorByScalar((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_flash_color, Math.Min(bombflashamount, 1.0));

                                bomb_effect_layer.Set(key, bombcolor);
                                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_peripheral_use)
                                {
                                    bomb_effect_layer.Set(Devices.DeviceKeys.Peripheral, bombcolor);
                                }
                            }
                        }
                    }
                    else if (bombstate == BombState.Defused)
                    {
                        bombtimer.Stop();
                        if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_display_winner_color)
                        {
                            foreach (Devices.DeviceKeys key in _bombkeys)
                                bomb_effect_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ct_color);

                            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_peripheral_use)
                                bomb_effect_layer.Set(Devices.DeviceKeys.Peripheral, (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).ct_color);
                        }
                    }
                    else if (bombstate == BombState.Exploded)
                    {
                        bombtimer.Stop();
                        if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_display_winner_color)
                        {
                            foreach (Devices.DeviceKeys key in _bombkeys)
                                bomb_effect_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).t_color);
                            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).bomb_peripheral_use)
                                bomb_effect_layer.Set(Devices.DeviceKeys.Peripheral, (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).t_color);
                        }
                    }
                    else
                    {
                        bombtimer.Stop();
                    }

                    layers.Enqueue(bomb_effect_layer);
                }
            }

            //Kills Indicator
            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).kills_indicator && isLocal)
            {
                EffectLayer kills_indicator_layer = new EffectLayer("CSGO - Kills Indicator");
                Devices.DeviceKeys[] _killsKeys = (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).kills_sequence.keys.ToArray();
                for (int pos = 0; pos < _killsKeys.Length; pos++)
                {
                    if (pos < roundKills.Count)
                    {
                        switch (roundKills[pos])
                        {
                            case (RoundKillType.Regular):
                                kills_indicator_layer.Set(_killsKeys[pos], (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).kills_regular_color);
                                break;
                            case (RoundKillType.Headshot):
                                kills_indicator_layer.Set(_killsKeys[pos], (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).kills_headshot_color);
                                break;
                        }
                    }
                }
                layers.Enqueue(kills_indicator_layer);
            }

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("CSGO - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Update Burning

            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).burning_enabled && burnamount > 0)
            {
                EffectLayer burning_layer = new EffectLayer("CSGO - Burning");
                double burning_percent = (double)burnamount / 255.0;
                Color burncolor = (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).burning_color;

                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).burning_animation)
                {
                    int red_adjusted = (int)((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).burning_color.R + (Math.Cos((general_timer.ElapsedMilliseconds + randomizer.Next(75)) / 75.0) * 0.15 * 255));
                    byte red = 0;

                    if (red_adjusted > 255)
                        red = 255;
                    else if (red_adjusted < 0)
                        red = 0;
                    else
                        red = (byte)red_adjusted;

                    int green_adjusted = (int)((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).burning_color.G + (Math.Sin((general_timer.ElapsedMilliseconds + randomizer.Next(150)) / 75.0) * 0.15 * 255));
                    byte green = 0;

                    if (green_adjusted > 255)
                        green = 255;
                    else if (green_adjusted < 0)
                        green = 0;
                    else
                        green = (byte)green_adjusted;

                    int blue_adjusted = (int)((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).burning_color.B + (Math.Cos((general_timer.ElapsedMilliseconds + randomizer.Next(225)) / 75.0) * 0.15 * 255));
                    byte blue = 0;

                    if (blue_adjusted > 255)
                        blue = 255;
                    else if (blue_adjusted < 0)
                        blue = 0;
                    else
                        blue = (byte)blue_adjusted;

                    burncolor = Color.FromArgb(burnamount, red, green, blue);
                }

                burning_layer.Fill(burncolor);

                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).burning_peripheral_use)
                    burning_layer.Set(Devices.DeviceKeys.Peripheral, burncolor);
                layers.Enqueue(burning_layer);
            }

            //Update Flashed
            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).flashbang_enabled && flashamount > 0)
            {
                EffectLayer flashed_layer = new EffectLayer("CSGO - Flashed");
                double flash_percent = (double)flashamount / 255.0;

                Color flash_color = Color.FromArgb(flashamount, (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).flash_color);

                flashed_layer.Fill(flash_color);

                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).flashbang_peripheral_use)
                    flashed_layer.Set(Devices.DeviceKeys.Peripheral, flash_color);
                layers.Enqueue(flashed_layer);
            }

            //Update Typing Keys
            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).typing_enabled && current_activity == PlayerActivity.TextInput)
            {
                EffectLayer typing_keys_layer = new EffectLayer("CSGO - Typing Keys");
                Devices.DeviceKeys[] _typingkeys = (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).typing_sequence.keys.ToArray();
                foreach (Devices.DeviceKeys key in _typingkeys)
                    typing_keys_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).typing_color);

                layers.Enqueue(typing_keys_layer);
            }


            if (general_timer.Elapsed.Seconds % this.updateRate == 0 && (general_timer.Elapsed.Seconds != this.lastUpdate))
            {
                this.lastUpdate = general_timer.Elapsed.Seconds;
            }

            //Scripts
            Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            foreach (var layer in Global.Configuration.ApplicationProfiles[profilename].Settings.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            if (new_game_state is GameState_CSGO)
            {
                _game_state = new_game_state;

                GameState_CSGO gs = (new_game_state as GameState_CSGO);

                try
                {
                    if (!IsPlanted &&
                       gs.Round.Phase == RoundPhase.Live &&
                       gs.Round.Bomb == BombState.Planted &&
                       gs.Previously.Round.Bomb == BombState.Undefined)
                    {
                        IsPlanted = true;
                        SetBombState(BombState.Planted);
                    }
                    else if (IsPlanted && gs.Round.Phase == RoundPhase.FreezeTime)
                    {
                        IsPlanted = false;
                        SetBombState(BombState.Undefined);
                    }
                    else if (IsPlanted && gs.Round.Bomb == BombState.Defused)
                    {
                        SetBombState(BombState.Defused);
                    }
                    else if (IsPlanted && gs.Round.Bomb == BombState.Exploded)
                    {
                        SetBombState(BombState.Exploded);
                    }

                    SetTeam(gs.Player.Team);
                    SetHealth(gs.Player.State.Health);
                    SetFlashAmount(gs.Player.State.Flashed);
                    SetBurnAmount(gs.Player.State.Burning);
                    SetClip(gs.Player.Weapons.ActiveWeapon.AmmoClip);
                    SetClipMax(gs.Player.Weapons.ActiveWeapon.AmmoClipMax);
                    SetPlayerActivity(gs.Player.Activity);
                    SetIsLocalPlayer(gs.Provider.SteamID.Equals(gs.Player.SteamID));
                    if (gs.Round.WinTeam == RoundWinTeam.Undefined && gs.Previously.Round.WinTeam != RoundWinTeam.Undefined)
                        RoundStart();
                    if (gs.Previously.Player.State.RoundKills != -1 && gs.Player.State.RoundKills != -1 && gs.Previously.Player.State.RoundKills < gs.Player.State.RoundKills && gs.Provider.SteamID.Equals(gs.Player.SteamID))
                        GotAKill(gs.Previously.Player.State.RoundKillHS != -1 && gs.Player.State.RoundKillHS != -1 && gs.Previously.Player.State.RoundKillHS < gs.Player.State.RoundKillHS);
                    if (gs.Player.State.Health == 100 && ((gs.Previously.Player.State.Health > -1 && gs.Previously.Player.State.Health < 100) || (gs.Round.WinTeam == RoundWinTeam.Undefined && gs.Previously.Round.WinTeam != RoundWinTeam.Undefined)) && gs.Provider.SteamID.Equals(gs.Player.SteamID))
                        Respawned();

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
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as CSGOSettings).isEnabled;
        }
    }
}
