using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Modules.Razer;
using Aurora.Settings.Layers.Controls;
using RazerSdkWrapper.Data;

namespace Aurora.Settings.Layers;

public class RazerLayerHandlerProperties : LayerHandlerProperties<RazerLayerHandlerProperties>
{

    [JsonIgnore]
    private bool? _transparencyEnabled;
    [JsonProperty("_TransparencyEnabled")]
    [LogicOverridable("Enable Transparency")]
    public bool TransparencyEnabled
    {
        get => Logic._transparencyEnabled ?? false;
        set => _transparencyEnabled = value;
    }

    [JsonIgnore]
    private bool? _colorPostProcessEnabled;
    [JsonProperty("_ColorPostProcessEnabled")]
    public bool ColorPostProcessEnabled
    {
        get => Logic._colorPostProcessEnabled ?? _colorPostProcessEnabled ?? false;
        set => _colorPostProcessEnabled = value;
    }

    [JsonIgnore]
    private double? _brightnessChange;
    [JsonProperty("_BrightnessChange")]
    public double BrightnessChange
    {
        get => Logic._brightnessChange ?? _brightnessChange ?? 0;
        set => _brightnessChange = value;
    }

    [JsonIgnore]
    private double? _saturationChange;
    [JsonProperty("_SaturationChange")]
    public double SaturationChange
    {
        get => Logic._saturationChange ?? _saturationChange ?? 0;
        set => _saturationChange = value;
    }

    [JsonIgnore]
    private double? _hueShift;
    [JsonProperty("_HueShift")]
    public double HueShift
    {
        get => Logic._hueShift ?? _hueShift ?? 0;
        set => _hueShift = value;
    }

    [JsonIgnore]
    private Dictionary<DeviceKeys, DeviceKeys>? _keyCloneMap;
    [JsonProperty("_KeyCloneMap")]
    public Dictionary<DeviceKeys, DeviceKeys> KeyCloneMap
    {
        get => Logic._keyCloneMap ?? (_keyCloneMap ??= new Dictionary<DeviceKeys, DeviceKeys>());
        set => _keyCloneMap = value;
    }

    public RazerLayerHandlerProperties() { }

    public RazerLayerHandlerProperties(bool arg = false) : base(arg) { }

    public override void Default()
    {
        base.Default();

        _colorPostProcessEnabled = false;
        _brightnessChange = 0;
        _saturationChange = 0;
        _hueShift = 0;
        _keyCloneMap = new Dictionary<DeviceKeys, DeviceKeys>();
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[LayerHandlerMeta(Name = "Razer Chroma", IsDefault = true)]
public class RazerLayerHandler : LayerHandler<RazerLayerHandlerProperties>
{
    public RazerLayerHandler() : base("Chroma Layer")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_RazerLayer(this);
    }

    private bool _empty = true;
    public override EffectLayer Render(IGameState gamestate)
    {
        if (!RzHelper.IsCurrentAppValid())
        {
            return EffectLayer.EmptyLayer;
        }
        _empty = false;
        if (!RzHelper.IsStale())
            return EffectLayer;

        foreach (var key in (DeviceKeys[])Enum.GetValues(typeof(DeviceKeys)))
        {
            if (!TryGetColor(key, out var color))
                continue;
                
            EffectLayer.Set(key, color);
        }

        foreach (var target in Properties.KeyCloneMap)
            if(TryGetColor(target.Value, out var clr))
                EffectLayer.Set(target.Key, clr);

        return EffectLayer;
    }

    private bool TryGetColor(DeviceKeys key, out Color color)
    {
        RzColor rColor;
        if (RazerLayoutMap.GenericKeyboard.TryGetValue(key, out var position))
            rColor = RzHelper.KeyboardColors[position[1] + position[0] * 22];
        else if (RazerLayoutMap.Mousepad.TryGetValue(key, out position))
            rColor = RzHelper.MousepadColors[1, position[0]];
        else if (RazerLayoutMap.Mouse.TryGetValue(key, out position))
            rColor = RzHelper.MouseColors[position[1] + position[0] * 7];
        else if (RazerLayoutMap.Headset.TryGetValue(key, out position))
            rColor = RzHelper.HeadsetColors[position[1]];
        else if (RazerLayoutMap.ChromaLink.TryGetValue(key, out position))
            rColor = RzHelper.ChromaLinkColors[position[0]];
        else
        {
            color = Color.Transparent;
            return false;
        }

        if (Properties.ColorPostProcessEnabled)
            rColor = PostProcessColor(rColor);
        color = FastTransform(rColor);

        return true;
    }

    private RzColor PostProcessColor(RzColor color)
    {
        if (color is { R: 0, G: 0, B: 0 })
            return color;

        if (Properties.BrightnessChange != 0)
            color = ColorUtils.ChangeBrightness(color, Properties.BrightnessChange);
        if (Properties.SaturationChange != 0)
            color = ColorUtils.ChangeSaturation(color, Properties.SaturationChange);
        if (Properties.HueShift != 0)
            color = ColorUtils.ChangeHue(color, Properties.HueShift);

        return color;
    }

    private Color FastTransform(RzColor color)
    {
        return Properties.TransparencyEnabled ?
            ColorUtils.FastColorTransparent(color.R, color.G, color.B) :
            ColorUtils.FastColor(color.R, color.G, color.B);
    }
}