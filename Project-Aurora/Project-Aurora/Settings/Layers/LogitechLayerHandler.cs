using System;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Modules;
using Aurora.Settings.Layers.Controls;

namespace Aurora.Settings.Layers;

public class LogitechLayerHandlerProperties : LayerHandlerProperties<LogitechLayerHandlerProperties>
{

    [JsonIgnore]
    private Dictionary<DeviceKeys, DeviceKeys>? _keyCloneMap;
    [JsonProperty("_KeyCloneMap")]
    public Dictionary<DeviceKeys, DeviceKeys> KeyCloneMap
    {
        get => Logic?._keyCloneMap ?? (_keyCloneMap ??= new Dictionary<DeviceKeys, DeviceKeys>());
        set => _keyCloneMap = value;
    }

    public LogitechLayerHandlerProperties() { }

    public LogitechLayerHandlerProperties(bool arg = false) : base(arg) { }

    public override void Default()
    {
        base.Default();

        _keyCloneMap = new Dictionary<DeviceKeys, DeviceKeys>();
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[LayerHandlerMeta(Name = "Logitech Lightsync", IsDefault = true)]
public sealed class LogitechLayerHandler : LayerHandler<LogitechLayerHandlerProperties>
{
    private bool _invalidated = true;
    private SolidBrush _background = new(Color.Empty);
    
    public LogitechLayerHandler() : base("Logitech Layer")
    {
        LogitechSdkModule.LogitechSdkListener.ColorsUpdated += LogitechSdkListenerOnColorsUpdated;
    }

    protected override UserControl CreateControl()
    {
        return new Control_DefaultLayer();//TODO Logitech Control
    }

    private void LogitechSdkListenerOnColorsUpdated(object? sender, EventArgs e)
    {
        _invalidated = true;
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        var logitechSdk = LogitechSdkModule.LogitechSdkListener;
        if (!logitechSdk.IsConnected)
        {
            return EffectLayer.EmptyLayer;
        }

        if (!_invalidated)
        {
            return EffectLayer;
        }

        _background.Color = logitechSdk.BackgroundColor;
        EffectLayer.Fill(_background);
        foreach (var kv in logitechSdk.Colors)
        {
            EffectLayer.Set(kv.Key, kv.Value);
        }

        foreach (var target in Properties.KeyCloneMap)
            if(TryGetColor(target.Value, out var clr))
                EffectLayer.Set(target.Key, clr);

        return EffectLayer;
    }

    private bool TryGetColor(DeviceKeys key, out Color color)
    {
        return !LogitechSdkModule.LogitechSdkListener.Colors.TryGetValue(key, out color);
    }

    public override void Dispose()
    {
        base.Dispose();

        LogitechSdkModule.LogitechSdkListener.ColorsUpdated -= LogitechSdkListenerOnColorsUpdated;
    }
}