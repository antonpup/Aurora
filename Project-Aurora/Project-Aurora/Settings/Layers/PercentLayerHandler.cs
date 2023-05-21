using System.ComponentModel;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Layers.Controls;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers;

public class PercentLayerHandlerProperties<TProperty> : LayerHandlerProperties2Color<TProperty> where TProperty : PercentLayerHandlerProperties<TProperty>
{
    [LogicOverridable("Percent Type")]
    public PercentEffectType? _PercentType { get; set; }
    [JsonIgnore]
    public PercentEffectType PercentType => Logic._PercentType ?? _PercentType ?? PercentEffectType.Progressive_Gradual;

    [LogicOverridable("Blink Threshold")]
    public double? _BlinkThreshold { get; set; }
    [JsonIgnore]
    public double BlinkThreshold => Logic._BlinkThreshold ?? _BlinkThreshold ?? 0.0;

    public bool? _BlinkDirection { get; set; }
    [JsonIgnore]
    public bool BlinkDirection => Logic._BlinkDirection ?? _BlinkDirection ?? false;

    [LogicOverridable("Blink Background")]
    public bool? _BlinkBackground { get; set; }
    [JsonIgnore]
    public bool BlinkBackground => Logic._BlinkBackground ?? _BlinkBackground ?? false;

    public string _VariablePath { get; set; }
    [JsonIgnore]
    public string VariablePath => Logic._VariablePath ?? _VariablePath ?? string.Empty;

    public string _MaxVariablePath { get; set; }
    [JsonIgnore]
    public string MaxVariablePath => Logic._MaxVariablePath ?? _MaxVariablePath ?? string.Empty;


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


    public PercentLayerHandlerProperties()
    { }
    public PercentLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();
        _PrimaryColor = ColorUtils.GenerateRandomColor();
        _SecondaryColor = ColorUtils.GenerateRandomColor();
        _PercentType = PercentEffectType.Progressive_Gradual;
        _BlinkThreshold = 0.0;
        _BlinkDirection = false;
        _BlinkBackground = false;
    }
}

public class PercentLayerHandlerProperties : PercentLayerHandlerProperties<PercentLayerHandlerProperties>
{
    public PercentLayerHandlerProperties()
    { }

    public PercentLayerHandlerProperties(bool empty = false) : base(empty) { }
}

public class PercentLayerHandler<TProperty> : LayerHandler<TProperty> where TProperty : PercentLayerHandlerProperties<TProperty>
{
    private double _value;
    protected bool Invalidated;

    public PercentLayerHandler() : base("PercentLayer")
    {
    }

    public override EffectLayer Render(IGameState state)
    {
        if (Invalidated)
        {
            EffectLayer.Clear();
            Invalidated = false;
            _value = -1;
        }
        var value = Properties.Logic._Value ?? state.GetNumber(Properties.VariablePath);
        if (ColorUtils.NearlyEqual(_value, value, 0.000001))
        {
            return EffectLayer;
        }
        _value = value;
            
        var maxvalue = Properties.Logic._MaxValue ?? state.GetNumber(Properties.MaxVariablePath);

        EffectLayer.PercentEffect(Properties.PrimaryColor, Properties.SecondaryColor, Properties.Sequence, value, maxvalue, Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection, Properties.BlinkBackground);
        return EffectLayer;
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

    protected override void PropertiesChanged(object sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        Invalidated = true;
    }
}

public class PercentLayerHandler : PercentLayerHandler<PercentLayerHandlerProperties>
{
    protected override UserControl CreateControl()
    {
        return new Control_PercentLayer(this);
    }
}