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
    public class Dota2BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<Dota2BackgroundLayerHandlerProperties>
    {
        public Color? _DefaultColor { get; set; }

        [JsonIgnore]
        public Color DefaultColor => Logic._DefaultColor ?? _DefaultColor ?? Color.Empty;

        public Color? _RadiantColor { get; set; }

        [JsonIgnore]
        public Color RadiantColor => Logic._RadiantColor ?? _RadiantColor ?? Color.Empty;

        public Color? _DireColor { get; set; }

        [JsonIgnore]
        public Color DireColor => Logic._DireColor ?? _DireColor ?? Color.Empty;

        public bool? _DimEnabled { get; set; }

        [JsonIgnore]
        public bool DimEnabled => Logic._DimEnabled ?? _DimEnabled ?? false;

        public double? _DimDelay { get; set; }

        [JsonIgnore]
        public double DimDelay => Logic._DimDelay ?? _DimDelay ?? 0.0;

        public Dota2BackgroundLayerHandlerProperties() : base() { }

        public Dota2BackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _DefaultColor = Color.FromArgb(140, 190, 230);
            _RadiantColor = Color.FromArgb(0, 140, 30);
            _DireColor = Color.FromArgb(200, 0, 0);
            _DimEnabled = true;
            _DimDelay = 15;
        }

    }

    public class Dota2BackgroundLayerHandler : LayerHandler<Dota2BackgroundLayerHandlerProperties>
    {
        private bool isDimming;
        private double dim_value = 1.0;
        private int dim_bg_at = 15;
        private readonly EffectLayer _bgLayer = new("Dota 2 - Background");

        protected override UserControl CreateControl()
        {
            return new Control_Dota2BackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            if (state is GameState_Dota2)
            {
                GameState_Dota2 dota2state = state as GameState_Dota2;

                if (dota2state.Previously.Hero.HealthPercent == 0 && dota2state.Hero.HealthPercent == 100 && !dota2state.Previously.Hero.IsAlive && dota2state.Hero.IsAlive)
                {
                    isDimming = false;
                    dim_bg_at = dota2state.Map.GameTime + (int)Properties.DimDelay;
                    dim_value = 1.0;
                }


                Color bg_color = Properties.DefaultColor;

                switch (dota2state.Player.Team)
                {
                    case PlayerTeam.Dire:
                        bg_color = Properties.DireColor;
                        break;
                    case PlayerTeam.Radiant:
                        bg_color = Properties.RadiantColor;
                        break;
                    default:
                        break;
                }

                if (dota2state.Player.Team == PlayerTeam.Dire || dota2state.Player.Team == PlayerTeam.Radiant)
                {
                    if (dim_bg_at <= dota2state.Map.GameTime || !dota2state.Hero.IsAlive)
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

                _bgLayer.Fill(bg_color);
            }

            return _bgLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2BackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        private double getDimmingValue()
        {
            if (isDimming && Properties.DimEnabled)
            {
                dim_value -= 0.02;
                return dim_value = (dim_value < 0.0 ? 0.0 : dim_value);
            }
            else
                return dim_value = 1.0;
        }
    }
}