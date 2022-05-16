using Aurora.EffectsEngine;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Dota_2.Layers
{
    public class Dota2RespawnLayerHandlerProperties : LayerHandlerProperties2Color<Dota2RespawnLayerHandlerProperties>
    {
        public Color? _RespawnColor { get; set; }

        [JsonIgnore]
        public Color RespawnColor => Logic._RespawnColor ?? _RespawnColor ?? Color.Empty;

        public Color? _RespawningColor { get; set; }

        [JsonIgnore]
        public Color RespawningColor => Logic._RespawningColor ?? _RespawningColor ?? Color.Empty;

        public Color? _BackgroundColor { get; set; }

        [JsonIgnore]
        public Color BackgroundColor => Logic._BackgroundColor ?? _BackgroundColor ?? Color.Empty;

        public Dota2RespawnLayerHandlerProperties() : base() { }

        public Dota2RespawnLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _RespawnColor = Color.FromArgb(255, 0, 0);
            _RespawningColor = Color.FromArgb(255, 170, 0);
            _BackgroundColor = Color.FromArgb(255, 255, 255);
            _Sequence = new KeySequence(
                new [] {
                    Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12,
                    Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                }
                );
        }

    }

    public class Dota2RespawnLayerHandler : LayerHandler<Dota2RespawnLayerHandlerProperties>
    {
        private readonly EffectLayer _respawnLayer = new("Dota 2 - Respawn");
        private readonly SolidBrush _solidBrush = new(Color.Empty);

        protected override UserControl CreateControl()
        {
            return new Control_Dota2RespawnLayer(this);
        }

        private bool _empty;
        public override EffectLayer Render(IGameState state)
        {
            if (state is not GameState_Dota2 dota2State) return _respawnLayer;

            if (dota2State.Player.Team != PlayerTeam.Undefined && dota2State.Player.Team != PlayerTeam.None && !dota2State.Hero.IsAlive)
            {
                var percent = dota2State.Hero.SecondsToRespawn > 5 ? 0.0 : 1.0 - dota2State.Hero.SecondsToRespawn / 5.0;
                if (percent > 0)
                {
                    _empty = false;
                    _solidBrush.Color = Utils.ColorUtils.BlendColors(Color.Transparent, Properties.BackgroundColor, percent);
                    _respawnLayer.Fill(_solidBrush);

                    _respawnLayer.PercentEffect(
                        Properties.RespawningColor,
                        Properties.RespawnColor,
                        Properties.Sequence,
                        percent,
                        1.0,
                        PercentEffectType.AllAtOnce);
                }
                else
                {
                    _respawnLayer.Clear();
                    _empty = true;
                }
            }
            else
            {
                if (_empty) return _respawnLayer;
                _respawnLayer.Clear();
                _empty = true;
            }

            return _respawnLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2RespawnLayer)?.SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
