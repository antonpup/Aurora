using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class PercentGradientLayerHandlerProperties : PercentLayerHandlerProperties<PercentGradientLayerHandlerProperties>
    {
        public PercentGradientLayerHandlerProperties() : base() { }
        public PercentGradientLayerHandlerProperties(bool empty = false) : base(empty) { }

        [Overrides.LogicOverridable("Gradient")]
        public EffectBrush _Gradient { get; set; }
        [JsonIgnore]
        public EffectBrush Gradient { get { return Logic._Gradient ?? _Gradient ?? new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear); } }
        
        public override void Default()
        {
            base.Default();
            this._Gradient = new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear);
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    public class PercentGradientLayerHandler : PercentLayerHandler<PercentGradientLayerHandlerProperties>
    { 
        public PercentGradientLayerHandler() : base()
        {
            _ID = "PercentGradient";
        }

        protected override UserControl CreateControl()
        {
            return new Control_PercentGradientLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            double value = Properties.Logic._Value ?? Utils.GameStateUtils.TryGetDoubleFromState(state, Properties.VariablePath);
            double maxvalue = Properties.Logic._MaxValue ?? Utils.GameStateUtils.TryGetDoubleFromState(state, Properties.MaxVariablePath);

            return new EffectLayer().PercentEffect(Properties.Gradient.GetColorSpectrum(), Properties.Sequence, value, maxvalue, Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection);
        }

        public override void SetApplication(Application profile)
        {
            if (profile != null)
            {
                double value;
                if (!double.TryParse(Properties._VariablePath, out value) && !string.IsNullOrWhiteSpace(Properties._VariablePath) && !profile.ParameterLookup.ContainsKey(Properties._VariablePath))
                    Properties._VariablePath = string.Empty;

                if (!double.TryParse(Properties._MaxVariablePath, out value) && !string.IsNullOrWhiteSpace(Properties._MaxVariablePath) && !profile.ParameterLookup.ContainsKey(Properties._MaxVariablePath))
                    Properties._MaxVariablePath = string.Empty;
            }
            (Control as Control_PercentGradientLayer).SetApplication(profile);
            this.Application = profile;
        }
    }
}
