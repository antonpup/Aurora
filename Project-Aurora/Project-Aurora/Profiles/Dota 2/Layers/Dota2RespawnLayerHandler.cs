using Aurora.EffectsEngine;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Utils;
using Common.Devices;

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
                new[] {
                    DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12,
                    DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                }
                );
        }

    }

    public class Dota2RespawnLayerHandler : LayerHandler<Dota2RespawnLayerHandlerProperties>
    {
        private readonly SolidBrush _solidBrush = new(Color.Empty);

        public Dota2RespawnLayerHandler(): base("Dota 2 - Respawn")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_Dota2RespawnLayer(this);
        }

        private bool _empty;
        public override EffectLayer Render(IGameState state)
        {
            if (state is not GameState_Dota2 dota2State) return EffectLayer.EmptyLayer;

            if (dota2State.Player.Team is PlayerTeam.Undefined or PlayerTeam.None ||
                dota2State.Hero.IsAlive) return EffectLayer.EmptyLayer;
            var percent = dota2State.Hero.SecondsToRespawn > 5 ? 0.0 : 1.0 - dota2State.Hero.SecondsToRespawn / 5.0;
            if (!(percent > 0)) return EffectLayer.EmptyLayer;

            _empty = false;
            _solidBrush.Color = ColorUtils.BlendColors(Color.Transparent, Properties.BackgroundColor, percent);
            EffectLayer.Fill(_solidBrush);

            EffectLayer.PercentEffect(
                Properties.RespawningColor,
                Properties.RespawnColor,
                Properties.Sequence,
                percent,
                1.0,
                PercentEffectType.AllAtOnce);
                    
            return EffectLayer;

        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2RespawnLayer)?.SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
