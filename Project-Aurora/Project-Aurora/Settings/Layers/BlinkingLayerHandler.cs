using System;
using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Layers.Controls;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers;

public class BlinkingLayerHandlerProperties : LayerHandlerProperties2Color<BlinkingLayerHandlerProperties>
{
    [JsonIgnore]
    private bool? _randomPrimaryColor;

    [JsonProperty("_RandomPrimaryColor")]
    public bool RandomPrimaryColor
    {
        get => Logic._randomPrimaryColor ?? _randomPrimaryColor ?? false;
        set => _randomPrimaryColor = value;
    }

    [JsonIgnore]
    private bool? _randomSecondaryColor;

    [JsonProperty("_RandomSecondaryColor")]
    public bool RandomSecondaryColor
    {
        get => Logic._randomSecondaryColor ?? _randomSecondaryColor ?? false;
        set => _randomSecondaryColor = value;
    }

    [JsonIgnore]
    private float? _effectSpeed;

    [JsonProperty("_EffectSpeed")]
    [LogicOverridable("Effect Speed")] 
    public float EffectSpeed
    {
        get => Logic._effectSpeed ?? _effectSpeed ?? 0.0f;
        set => _effectSpeed = value;
    }

    public BlinkingLayerHandlerProperties()
    { }

    public BlinkingLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();
        _randomPrimaryColor = false;
        _randomSecondaryColor = false;
        _effectSpeed = 1.0f;
    }
}

public class BlinkingLayerHandler : LayerHandler<BlinkingLayerHandlerProperties>
{
    private double _currentSine;

    private Color _currentPrimaryColor = Color.Transparent;
    private Color _currentSecondaryColor = Color.Transparent;

    public BlinkingLayerHandler() : base("Blinking Layer")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_BlinkingLayer(this);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        _currentSine = Math.Round(Math.Pow(Math.Sin(Time.GetMillisecondsSinceEpoch() % 10000L / 10000.0d * 2 * Math.PI * Properties.EffectSpeed), 2));

        if (_currentSine == 0.0f && Properties.RandomSecondaryColor)
            _currentSecondaryColor = ColorUtils.GenerateRandomColor();
        else if(!Properties.RandomSecondaryColor)
            _currentSecondaryColor = Properties.SecondaryColor;

        if (_currentSine >= 0.99f && Properties.RandomPrimaryColor)
            _currentPrimaryColor = ColorUtils.GenerateRandomColor();
        else if (!Properties.RandomPrimaryColor)
            _currentPrimaryColor = Properties.PrimaryColor;

        EffectLayer.Clear();
        EffectLayer.Set(Properties.Sequence, ColorUtils.BlendColors(_currentPrimaryColor, _currentSecondaryColor, _currentSine));

        return EffectLayer;
    }
}