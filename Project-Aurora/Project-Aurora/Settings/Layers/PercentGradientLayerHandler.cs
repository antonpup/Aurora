using Aurora.EffectsEngine;
using Aurora.Profiles;
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

        public EffectBrush _Gradient { get; set; }

        [JsonIgnore]
        public EffectBrush Gradient { get { return Logic._Gradient ?? _Gradient ?? new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear); } }
        
        public override void Default()
        {
            base.Default();
            this._Gradient = new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear);
        }
    }

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
            EffectLayer percent_layer = new EffectLayer();

            double value = 0;
            if (!double.TryParse(Properties._VariablePath, out value) && !string.IsNullOrWhiteSpace(Properties._VariablePath))
            {
                try
                {
                    value = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, Properties._VariablePath));
                }
                catch (Exception exc)
                {
                    value = 0;
                }
            }


            double maxvalue = 0;
            if (!double.TryParse(Properties._MaxVariablePath, out maxvalue) && !string.IsNullOrWhiteSpace(Properties._MaxVariablePath))
            {
                try
                {
                    maxvalue = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, Properties._MaxVariablePath));
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
            if (profile != null)
            {
                double value;
                if (!double.TryParse(Properties._VariablePath, out value) && !string.IsNullOrWhiteSpace(Properties._VariablePath) && !profile.ParameterLookup.ContainsKey(Properties._VariablePath))
                    Properties._VariablePath = string.Empty;

                if (!double.TryParse(Properties._MaxVariablePath, out value) && !string.IsNullOrWhiteSpace(Properties._MaxVariablePath) && !profile.ParameterLookup.ContainsKey(Properties._MaxVariablePath))
                    Properties._MaxVariablePath = string.Empty;
            }
            (Control as Control_PercentGradientLayer).SetProfile(profile);
        }
    }
}
