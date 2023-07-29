using System;
using System.ComponentModel;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System.Windows.Controls;
using Aurora.Settings.Layers.Controls;

namespace Aurora.Settings.Layers;

public class BinaryCounterLayerHandlerProperties : LayerHandlerProperties<BinaryCounterLayerHandlerProperties> {

    // The var path of the variable to use (set though the UI, cannot be set with overrides)
    [JsonIgnore] private string _variablePath;

    [JsonProperty("_VariablePath")]
    public string VariablePath
    {
        get => Logic._variablePath ?? _variablePath ?? "";
        set => SetFieldAndRaisePropertyChanged(out _variablePath, value);
    }

    // Allows the value to be directly set using the overrides system
    [JsonIgnore, LogicOverridable("Value")]
    public double? _Value { get; set; }

    public BinaryCounterLayerHandlerProperties() : base() { }
    public BinaryCounterLayerHandlerProperties(bool empty) : base(empty) { }

    public override void Default() {
        base.Default();
        _variablePath = "";
    }
}


public class BinaryCounterLayerHandler : LayerHandler<BinaryCounterLayerHandlerProperties> {

    private Control_BinaryCounterLayer _control;
    protected override UserControl CreateControl() => _control ??= new Control_BinaryCounterLayer(this);
    private double lastValue = -1;

    public BinaryCounterLayerHandler() : base("BinaryCounterLayer") { }

    public override void SetApplication(Application profile) {
        base.SetApplication(profile);
        _control?.SetApplication(profile);
    }

    public override EffectLayer Render(IGameState gamestate) {
        // Get the current game state value
        var value = Properties.Logic._Value ?? gamestate.GetNumber(Properties.VariablePath);
        if (Math.Abs(lastValue - value) < 0.1)
        {
            return EffectLayer;
        }

        EffectLayer.Clear();
        // Set the active key
        for (var i = 0; i < Properties.Sequence.Keys.Count; i++)
            if (((int)value & 1 << i) > 0)
                EffectLayer.Set(Properties.Sequence.Keys[i], Properties.PrimaryColor);
        lastValue = value;
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        lastValue = -1;
    }
}