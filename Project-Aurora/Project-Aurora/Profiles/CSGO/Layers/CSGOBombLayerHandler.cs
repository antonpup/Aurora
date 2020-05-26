using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOBombLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBombLayerHandlerProperties>
    {
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

            this._Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.NUM_LOCK, Devices.DeviceKeys.NUM_SLASH, Devices.DeviceKeys.NUM_ASTERISK, Devices.DeviceKeys.NUM_MINUS, Devices.DeviceKeys.NUM_SEVEN, Devices.DeviceKeys.NUM_EIGHT, Devices.DeviceKeys.NUM_NINE, Devices.DeviceKeys.NUM_PLUS, Devices.DeviceKeys.NUM_FOUR, Devices.DeviceKeys.NUM_FIVE, Devices.DeviceKeys.NUM_SIX, Devices.DeviceKeys.NUM_ONE, Devices.DeviceKeys.NUM_TWO, Devices.DeviceKeys.NUM_THREE, Devices.DeviceKeys.NUM_ZERO, Devices.DeviceKeys.NUM_PERIOD, Devices.DeviceKeys.NUM_ENTER });
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

        protected override UserControl CreateControl()
        {
            return new Control_CSGOBombLayer(this);
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
                        bomb_effect_layer.Set(Devices.DeviceKeys.Peripheral, bombcolor);
                }
                else
                {
                    bombtimer.Stop();
                }
            }

            return bomb_effect_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOBombLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}