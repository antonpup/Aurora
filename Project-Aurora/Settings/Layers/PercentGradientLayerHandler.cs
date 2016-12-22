using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Layers
{
    public class PercentGradientLayerHandlerProperties : LayerHandlerProperties2Color<PercentGradientLayerHandlerProperties>
    {
        public EffectBrush _Gradient { get; set; }

        [JsonIgnore]
        public EffectBrush Gradient { get { return Logic._Gradient ?? _Gradient ?? new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear); } }

        public PercentEffectType? _PercentType { get; set; }

        [JsonIgnore]
        public PercentEffectType PercentType { get { return Logic._PercentType ?? _PercentType ?? PercentEffectType.Progressive_Gradual; } }

        public double? _BlinkThreshold { get; set; }

        [JsonIgnore]
        public double BlinkThreshold { get { return Logic._BlinkThreshold ?? _BlinkThreshold ?? 0.0; } }

        public bool? _BlinkDirection { get; set; }

        [JsonIgnore]
        public bool BlinkDirection { get { return Logic._BlinkDirection ?? _BlinkDirection ?? false; } }

        public PercentGradientLayerHandlerProperties() : base() { }

        public PercentGradientLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
            this._SecondaryColor = Utils.ColorUtils.GenerateRandomColor();
            this._Gradient = new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear);
            this._PercentType = PercentEffectType.Progressive_Gradual;
            this._BlinkThreshold = 0.0;
            this._BlinkDirection = false;
        }
    }

    public class PercentGradientLayerHandler : LayerHandler<PercentGradientLayerHandlerProperties>
    {
        public string VariablePath = "";
        public string MaxVariablePath = "";

        public PercentGradientLayerHandler() : base()
        {
            _Control = new Control_PercentGradientLayer(this);

            _Type = LayerType.PercentGradient;
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer percent_layer = new EffectLayer();

            double value = 0;
            if (!double.TryParse(VariablePath, out value) && !string.IsNullOrWhiteSpace(VariablePath))
            {
                try
                {
                    value = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, VariablePath));
                }
                catch (Exception exc)
                {
                    value = 0;
                }
            }


            double maxvalue = 0;
            if (!double.TryParse(MaxVariablePath, out maxvalue) && !string.IsNullOrWhiteSpace(MaxVariablePath))
            {
                try
                {
                    maxvalue = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, MaxVariablePath));
                }
                catch (Exception exc)
                {
                    maxvalue = 0;
                }
            }

            percent_layer.PercentEffect(Properties.Gradient.GetColorSpectrum(), Properties.Sequence, value, maxvalue, Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection);

            return percent_layer;
        }

        public override void SetProfile(ProfileManager profile)
        {
            (_Control as Control_PercentGradientLayer).SetProfile(profile);
        }
    }
}
