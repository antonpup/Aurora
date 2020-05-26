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
    public class PercentLayerHandlerProperties<TProperty> : LayerHandlerProperties2Color<TProperty> where TProperty : PercentLayerHandlerProperties<TProperty>
    {
        [LogicOverridable("Percent Type")]
        public PercentEffectType? _PercentType { get; set; }
        [JsonIgnore]
        public PercentEffectType PercentType { get { return Logic._PercentType ?? _PercentType ?? PercentEffectType.Progressive_Gradual; } }

        [LogicOverridable("Blink Threshold")]
        public double? _BlinkThreshold { get; set; }
        [JsonIgnore]
        public double BlinkThreshold { get { return Logic._BlinkThreshold ?? _BlinkThreshold ?? 0.0; } }

        public bool? _BlinkDirection { get; set; }
        [JsonIgnore]
        public bool BlinkDirection { get { return Logic._BlinkDirection ?? _BlinkDirection ?? false; } }

        [LogicOverridable("Blink Background")]
        public bool? _BlinkBackground { get; set; }
        [JsonIgnore]
        public bool BlinkBackground { get { return Logic._BlinkBackground ?? _BlinkBackground ?? false; } }

        public string _VariablePath { get; set; }
        [JsonIgnore]
        public string VariablePath { get { return Logic._VariablePath ?? _VariablePath ?? string.Empty; } }

        public string _MaxVariablePath { get; set; }
        [JsonIgnore]
        public string MaxVariablePath { get { return Logic._MaxVariablePath ?? _MaxVariablePath ?? string.Empty; } }


        // These two properties work slightly differently to the others. These are special properties that allow for
        // override the value using the overrides system. These are not displayed/directly editable by the user and 
        // will not actually store a value (so should be ignored by the JSON serializers). If these have a value (non
        // null), then they will be used as the value/max value for the percent effect, else the _VariablePath and
        // _MaxVariablePaths will be resolved.
        [JsonIgnore]
        [LogicOverridable("Value")]
        public double? _Value { get; set; }

        [JsonIgnore]
        [LogicOverridable("Max Value")]
        public double? _MaxValue { get; set; }


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
            double value = Properties.Logic._Value ?? state.GetNumber(Properties.VariablePath);
            double maxvalue = Properties.Logic._MaxValue ?? state.GetNumber(Properties.MaxVariablePath);

            return new EffectLayer().PercentEffect(Properties.PrimaryColor, Properties.SecondaryColor, Properties.Sequence, value, maxvalue, Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection, Properties.BlinkBackground);
        }

        public override void SetApplication(Application profile)
        {
            if (profile != null)
            {
                if (!double.TryParse(Properties._VariablePath, out _) && !string.IsNullOrWhiteSpace(Properties._VariablePath) && !profile.ParameterLookup.IsValidParameter(Properties._VariablePath))
                    Properties._VariablePath = string.Empty;

                if (!double.TryParse(Properties._MaxVariablePath, out _) && !string.IsNullOrWhiteSpace(Properties._MaxVariablePath) && !profile.ParameterLookup.IsValidParameter(Properties._MaxVariablePath))
                    Properties._MaxVariablePath = string.Empty;
            }
            (Control as Control_PercentLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }

    public class PercentLayerHandler : PercentLayerHandler<PercentLayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_PercentLayer(this);
        }
    }
}
