using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOBombLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBombLayerHandlerProperties>
    {
        public Color? _FlashColor { get; set; }

        [JsonIgnore]
        public Color FlashColor => Logic._FlashColor ?? _FlashColor ?? Color.Empty;

        public Color? _PrimedColor { get; set; }

        [JsonIgnore]
        public Color PrimedColor => Logic._PrimedColor ?? _PrimedColor ?? Color.Empty;

        public bool? _DisplayWinningTeamColor { get; set; }

        public bool? _GradualEffect { get; set; }

        [JsonIgnore]
        public bool GradualEffect { get { return Logic._GradualEffect ?? _GradualEffect ?? false; } }

        public bool? _PeripheralUse { get; set; }

        [JsonIgnore]
        public bool PeripheralUse { get { return Logic._PeripheralUse ?? _PeripheralUse ?? false; } }

        public CSGOBombLayerHandlerProperties()
        { }

        public CSGOBombLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _Sequence = new KeySequence(new[] { DeviceKeys.NUM_LOCK, DeviceKeys.NUM_SLASH, DeviceKeys.NUM_ASTERISK, DeviceKeys.NUM_MINUS, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT, DeviceKeys.NUM_NINE, DeviceKeys.NUM_PLUS, DeviceKeys.NUM_FOUR, DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX, DeviceKeys.NUM_ONE, DeviceKeys.NUM_TWO, DeviceKeys.NUM_THREE, DeviceKeys.NUM_ZERO, DeviceKeys.NUM_PERIOD, DeviceKeys.NUM_ENTER });
            _FlashColor = Color.FromArgb(255, 0, 0);
            _PrimedColor = Color.FromArgb(0, 255, 0);
            _DisplayWinningTeamColor = true;
            _GradualEffect = true;
            _PeripheralUse = true;
        }

    }

    public class CSGOBombLayerHandler : LayerHandler<CSGOBombLayerHandlerProperties>
    {
        private static readonly Stopwatch Bombtimer = new();
        private static bool _bombFlash;
        private static int _bombFlashCount;
        private static long _bombFlashTime;
        private static long _bombFlashEdat;

        public CSGOBombLayerHandler(): base("CSGO - Bomb Effect")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_CSGOBombLayer(this);
        }

        private bool _empty = true;
        public override EffectLayer Render(IGameState state)
        {
            if (state is not GameState_CSGO csgostate) return EffectLayer.EmptyLayer;

            if (csgostate.Round.Bomb != BombState.Planted)
            {
                return EffectLayer.EmptyLayer;
            }
            _empty = false;

            if (!Bombtimer.IsRunning)
            {
                Bombtimer.Restart();
                _bombFlashCount = 0;
                _bombFlashTime = 0;
                _bombFlashEdat = 0;
            }

            double bombflashamount = 1.0;
            bool isCritical = false;

            if (Bombtimer.ElapsedMilliseconds < 38000)
            {
                if (Bombtimer.ElapsedMilliseconds >= _bombFlashTime)
                {
                    _bombFlash = true;
                    _bombFlashEdat = Bombtimer.ElapsedMilliseconds;
                    _bombFlashTime = Bombtimer.ElapsedMilliseconds + (1000 - (_bombFlashCount++ * 13));
                }

                if (Bombtimer.ElapsedMilliseconds < _bombFlashEdat || Bombtimer.ElapsedMilliseconds > _bombFlashEdat + 220)
                    bombflashamount = 0.0;
                else
                    bombflashamount = Math.Pow(Math.Sin((Bombtimer.ElapsedMilliseconds - _bombFlashEdat) / 80.0 + 0.25), 2.0);
            }
            else if (Bombtimer.ElapsedMilliseconds >= 38000)
            {
                isCritical = true;
                bombflashamount = Bombtimer.ElapsedMilliseconds / 40000.0;
            }
            else if (Bombtimer.ElapsedMilliseconds >= 45000)
            {
                Bombtimer.Stop();
                csgostate.Round.Bomb = BombState.Undefined;
            }

            if (!isCritical)
            {
                if (bombflashamount <= 0.05 && _bombFlash)
                    _bombFlash = false;

                if (!_bombFlash)
                    bombflashamount = 0.0;
            }

            if (!Properties.GradualEffect)
                bombflashamount = Math.Round(bombflashamount);

            Color bombcolor;
            if (bombflashamount > 0)
            {
                if (isCritical)
                    bombcolor = ColorUtils.MultiplyColorByScalar(Properties.PrimedColor, Math.Min(bombflashamount, 1.0));
                else
                    bombcolor = ColorUtils.MultiplyColorByScalar(Properties.FlashColor, Math.Min(bombflashamount, 1.0));
            }
            else
            {
                bombcolor = Color.Empty;
            }

            EffectLayer.Set(Properties.Sequence, bombcolor);

            if (Properties.PeripheralUse)
                EffectLayer.Set(DeviceKeys.Peripheral, bombcolor);

            return EffectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOBombLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}