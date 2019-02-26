using Aurora.Devices.Layout.Layouts;
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
                new KeyboardKeys[] {
                    KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4, KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8, KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12,
                    KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR, KeyboardKeys.FIVE, KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT, KeyboardKeys.NINE, KeyboardKeys.ZERO, KeyboardKeys.MINUS, KeyboardKeys.EQUALS
                }
                );
        }

    }

    public class Dota2RespawnLayerHandler : LayerHandler<Dota2RespawnLayerHandlerProperties>
    {
        public Dota2RespawnLayerHandler() : base()
        {
            _ID = "Dota2Respawn";
        }

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
