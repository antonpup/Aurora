using Aurora.Devices.Layout.Layouts;
using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOBombLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBombLayerHandlerProperties>
    {
        public Color? _CTColor { get; set; }

        [JsonIgnore]
        public Color CTColor { get { return Logic._CTColor ?? _CTColor ?? Color.Empty; } }

        public Color? _TColor { get; set; }

        [JsonIgnore]
        public Color TColor { get { return Logic._TColor ?? _TColor ?? Color.Empty; } }

        public Color? _FlashColor { get; set; }

        [JsonIgnore]
        public Color FlashColor { get { return Logic._FlashColor ?? _FlashColor ?? Color.Empty; } }

        public Color? _PrimedColor { get; set; }

        [JsonIgnore]
        public Color PrimedColor { get { return Logic._PrimedColor ?? _PrimedColor ?? Color.Empty; } }

        public bool? _DisplayWinningTeamColor { get; set; }

        [JsonIgnore]
        public bool DisplayWinningTeamColor { get { return Logic._DisplayWinningTeamColor ?? _DisplayWinningTeamColor ?? false; } }

        public bool? _GradualEffect { get; set; }

        [JsonIgnore]
        public bool GradualEffect { get { return Logic._GradualEffect ?? _GradualEffect ?? false; } }

        public bool? _PeripheralUse { get; set; }

        [JsonIgnore]
        public bool PeripheralUse { get { return Logic._PeripheralUse ?? _PeripheralUse ?? false; } }

        public CSGOBombLayerHandlerProperties() : base() { }

        public CSGOBombLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.NUM_LOCK, KeyboardKeys.NUM_SLASH, KeyboardKeys.NUM_ASTERISK, KeyboardKeys.NUM_MINUS, KeyboardKeys.NUM_SEVEN, KeyboardKeys.NUM_EIGHT, KeyboardKeys.NUM_NINE, KeyboardKeys.NUM_PLUS, KeyboardKeys.NUM_FOUR, KeyboardKeys.NUM_FIVE, KeyboardKeys.NUM_SIX, KeyboardKeys.NUM_ONE, KeyboardKeys.NUM_TWO, KeyboardKeys.NUM_THREE, KeyboardKeys.NUM_ZERO, KeyboardKeys.NUM_PERIOD, KeyboardKeys.NUM_ENTER });
            this._CTColor = Color.FromArgb(158, 205, 255);
            this._TColor = Color.FromArgb(221, 99, 33);
            this._FlashColor = Color.FromArgb(255, 0, 0);
            this._PrimedColor = Color.FromArgb(0, 255, 0);
            this._DisplayWinningTeamColor = true;
            this._GradualEffect = true;
            this._PeripheralUse = true;
        }

    }

    public class CSGOBombLayerHandler : LayerHandler<CSGOBombLayerHandlerProperties>
    {
        private static Stopwatch bombtimer = new Stopwatch();
        private static bool bombflash = false;
        private static int bombflashcount = 0;
        private static long bombflashtime = 0;
        private static long bombflashedat = 0;

        public CSGOBombLayerHandler() : base()
        {
            _ID = "CSGOBomb";
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bomb_effect_layer = new EffectLayer("CSGO - Bomb Effect");

            if (state is GameState_CSGO)
            {
                GameState_CSGO csgostate = state as GameState_CSGO;

                if (csgostate.Round.Bomb == BombState.Planted)
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

                        if (bombtimer.ElapsedMilliseconds < bombflashedat || bombtimer.ElapsedMilliseconds > bombflashedat + 220)
                            bombflashamount = 0.0;
                        else
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
                        csgostate.Round.Bomb = BombState.Undefined;
                    }

                    if (!isCritical)
                    {
                        if (bombflashamount <= 0.05 && bombflash)
                            bombflash = false;

                        if (!bombflash)
                            bombflashamount = 0.0;
                    }

                    if (!Properties.GradualEffect)
                        bombflashamount = Math.Round(bombflashamount);

                    Color bombcolor = Properties.FlashColor;

                    if (isCritical)
                        bombcolor = Utils.ColorUtils.MultiplyColorByScalar(Properties.PrimedColor, Math.Min(bombflashamount, 1.0));
                    else
                        bombcolor = Utils.ColorUtils.MultiplyColorByScalar(Properties.FlashColor, Math.Min(bombflashamount, 1.0));

                    bomb_effect_layer.Set(Properties.Sequence, bombcolor);

                    if (Properties.PeripheralUse)
                        bomb_effect_layer.SetGlobal(bombcolor);
                }
                else if (csgostate.Round.Bomb == BombState.Defused)
                {
                    bombtimer.Stop();
                    if (Properties.DisplayWinningTeamColor)
                    {
                        bomb_effect_layer.Set(Properties.Sequence, Properties.CTColor);

                        if (Properties.PeripheralUse)
                            bomb_effect_layer.SetGlobal(Properties.CTColor);
                    }
                }
                else if (csgostate.Round.Bomb == BombState.Exploded)
                {
                    bombtimer.Stop();
                    if (Properties.DisplayWinningTeamColor)
                    {
                        bomb_effect_layer.Set(Properties.Sequence, Properties.TColor);

                        if (Properties.PeripheralUse)
                            bomb_effect_layer.SetGlobal(Properties.TColor);
                    }
                }
                else
                {
                    bombtimer.Stop();
                }
            }

            return bomb_effect_layer;
        }
    }
}