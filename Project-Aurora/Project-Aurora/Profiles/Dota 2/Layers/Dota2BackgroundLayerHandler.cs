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

        public Dota2BackgroundLayerHandler(): base("Dota 2 - Background")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_Dota2BackgroundLayer(this);
        }

        private SolidBrush _currentColor = new(Color.Empty);
        public override EffectLayer Render(IGameState state)
        {
            if (state is not GameState_Dota2 dota2State) return EffectLayer.EmptyLayer;

            if (dota2State.Previously.Hero.HealthPercent == 0 && dota2State.Hero.HealthPercent == 100 && !dota2State.Previously.Hero.IsAlive && dota2State.Hero.IsAlive)
            {
                isDimming = false;
                dim_bg_at = dota2State.Map.GameTime + (int)Properties.DimDelay;
                dim_value = 1.0;
            }

            Color bgColor = dota2State.Player.Team switch
            {
                PlayerTeam.Dire => Properties.DireColor,
                PlayerTeam.Radiant => Properties.RadiantColor,
                _ => Properties.DefaultColor
            };

            if (dota2State.Player.Team == PlayerTeam.Dire || dota2State.Player.Team == PlayerTeam.Radiant)
            {
                if (dim_bg_at <= dota2State.Map.GameTime || !dota2State.Hero.IsAlive)
                {
                    isDimming = true;
                    bgColor = Utils.ColorUtils.MultiplyColorByScalar(bgColor, GetDimmingValue());
                }
                else
                {
                    isDimming = false;
                    dim_value = 1.0;
                }
            }

            if (_currentColor.Color != bgColor)
            {
                _currentColor = new SolidBrush(bgColor);
                EffectLayer.Clear();
                EffectLayer.Fill(_currentColor);
            }

            return EffectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2BackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        private double GetDimmingValue()
        {
            if (isDimming && Properties.DimEnabled)
            {
                dim_value -= 0.02;
                return dim_value = (dim_value < 0.0 ? 0.0 : dim_value);
            }

            return dim_value = 1.0;
        }
    }
}