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
    public class PercentLayerHandlerProperties<TProperty> : LayerHandlerProperties2Color<TProperty> where TProperty : PercentLayerHandlerProperties<TProperty>
    {
        public PercentEffectType? _PercentType { get; set; }

        [JsonIgnore]
        public PercentEffectType PercentType { get { return Logic._PercentType ?? _PercentType ?? PercentEffectType.Progressive_Gradual; } }

        public double? _BlinkThreshold { get; set; }

        [JsonIgnore]
        public double BlinkThreshold { get { return Logic._BlinkThreshold ?? _BlinkThreshold ?? 0.0; } }

        public bool? _BlinkDirection { get; set; }

        public bool? _BlinkBackground { get; set; }

        [JsonIgnore]
        public bool BlinkDirection { get { return Logic._BlinkDirection ?? _BlinkDirection ?? false; } }

        [JsonIgnore]
        public bool BlinkBackground { get { return Logic._BlinkBackground ?? _BlinkBackground ?? false; } }

        public string _VariablePath { get; set; }

        [JsonIgnore]
        public string VariablePath { get { return Logic._VariablePath ?? _VariablePath ?? string.Empty; } }

        public string _MaxVariablePath { get; set; }

        [JsonIgnore]
        public string MaxVariablePath { get { return Logic._MaxVariablePath ?? _MaxVariablePath ?? string.Empty; } }

        public PercentLayerHandlerProperties() : base() { }

        public PercentLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
            this._SecondaryColor = Utils.ColorUtils.GenerateRandomColor();
            this._PercentType = PercentEffectType.Progressive_Gradual;
            this._BlinkThreshold = 0.0;
            this._BlinkDirection = false;
            this._BlinkBackground = false;
        }
    }

    public class PercentLayerHandlerProperties : PercentLayerHandlerProperties<PercentLayerHandlerProperties>
    {
        public PercentLayerHandlerProperties() : base() { }

        public PercentLayerHandlerProperties(bool empty = false) : base(empty) { }
    }

    public class PercentLayerHandler<TProperty> : LayerHandler<TProperty> where TProperty : PercentLayerHandlerProperties<TProperty>
    {
        public override EffectLayer Render(IGameState state)
        {
            double value = 0;
            if (!double.TryParse(Properties.VariablePath, out value) && !string.IsNullOrWhiteSpace(Properties.VariablePath))
            {
                try
                {
                    value = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, Properties.VariablePath));
                }
                catch (Exception exc)
                {
                    value = 0;
                    if (Global.isDebug)
                        throw exc;
                }
            }


            double maxvalue = 0;
            if (!double.TryParse(Properties.MaxVariablePath, out maxvalue) && !string.IsNullOrWhiteSpace(Properties.MaxVariablePath))
            {
                try
                {
                    maxvalue = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, Properties.MaxVariablePath));
                }
                catch (Exception exc)
                {
                    maxvalue = 0;
                    if (Global.isDebug)
                        throw exc;
                }
            }
            return new EffectLayer().PercentEffect(Properties.PrimaryColor, Properties.SecondaryColor, Properties.Sequence, value, maxvalue, Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection, Properties.BlinkBackground);
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
            (Control as Control_PercentLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }

    public class PercentLayerHandler : PercentLayerHandler<PercentLayerHandlerProperties>
    {
        public PercentLayerHandler() : base()
        {
            _ID = "Percent";
        }

        protected override UserControl CreateControl()
        {
            return new Control_PercentLayer(this);
        }
    }
}
