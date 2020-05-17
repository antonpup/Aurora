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
        public Color RespawnColor { get { return Logic._RespawnColor ?? _RespawnColor ?? Color.Empty; } }

        public Color? _RespawningColor { get; set; }

        [JsonIgnore]
        public Color RespawningColor { get { return Logic._RespawningColor ?? _RespawningColor ?? Color.Empty; } }

        public Color? _BackgroundColor { get; set; }

        [JsonIgnore]
        public Color BackgroundColor { get { return Logic._BackgroundColor ?? _BackgroundColor ?? Color.Empty; } }

        public Dota2RespawnLayerHandlerProperties() : base() { }

        public Dota2RespawnLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._RespawnColor = Color.FromArgb(255, 0, 0);
            this._RespawningColor = Color.FromArgb(255, 170, 0);
            this._BackgroundColor = Color.FromArgb(255, 255, 255);
            this._Sequence = new KeySequence(
                new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12,
                    Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                }
                );
        }

    }

    public class Dota2RespawnLayerHandler : LayerHandler<Dota2RespawnLayerHandlerProperties>
    {

        protected override UserControl CreateControl()
        {
            return new Control_Dota2RespawnLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer respawn_layer = new EffectLayer("Dota 2 - Respawn");

            if (state is GameState_Dota2)
            {
                GameState_Dota2 dota2state = state as GameState_Dota2;

                if (dota2state.Player.Team != PlayerTeam.Undefined && dota2state.Player.Team != PlayerTeam.None && !dota2state.Hero.IsAlive)
                {
                    double percent = (dota2state.Hero.SecondsToRespawn > 5 ? 0.0 : 1.0 - (dota2state.Hero.SecondsToRespawn / 5.0));

                    respawn_layer.Fill(Utils.ColorUtils.BlendColors(Color.Transparent, Properties.BackgroundColor, percent));

                    respawn_layer.PercentEffect(Properties.RespawningColor,
                            Properties.RespawnColor,
                            Properties.Sequence,
                            percent,
                            1.0,
                            PercentEffectType.AllAtOnce);
                }
            }

            return respawn_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2RespawnLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
