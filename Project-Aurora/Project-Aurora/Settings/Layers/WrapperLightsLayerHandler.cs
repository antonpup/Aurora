using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Layers.Controls;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Common.Devices;
using Common.Devices.Logitech;
using Common.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers;

public class WrapperLightsLayerHandlerProperties : LayerHandlerProperties<WrapperLightsLayerHandlerProperties>
{
    // Color Enhancing
    [JsonIgnore]
    private bool? _colorEnhanceEnabled;
    [JsonProperty("_ColorEnhanceEnabled")]
    public bool ColorEnhanceEnabled
    {
        get => Logic._colorEnhanceEnabled ?? _colorEnhanceEnabled ?? false;
        set => _colorEnhanceEnabled = value;
    }

    [JsonIgnore]
    private int? _colorEnhanceMode;
    [JsonProperty("_ColorEnhanceMode")]
    public int ColorEnhanceMode
    {
        get => Logic._colorEnhanceMode ?? _colorEnhanceMode ?? 0;
        set => _colorEnhanceMode = value;
    }

    [JsonIgnore]
    private int? _colorEnhanceColorFactor;
    [JsonProperty("_ColorEnhanceColorFactor")]
    public int ColorEnhanceColorFactor
    {
        get => Logic._colorEnhanceColorFactor ?? _colorEnhanceColorFactor ?? 0;
        set => _colorEnhanceColorFactor = value;
    }

    [JsonIgnore]
    private float? _colorEnhanceColorHsvSine;
    [JsonProperty("_ColorEnhanceColorHSVSine")]
    public float ColorEnhanceColorHsvSine
    {
        get => Logic._colorEnhanceColorHsvSine ?? _colorEnhanceColorHsvSine ?? 0.0f;
        set => _colorEnhanceColorHsvSine = value;
    }

    [JsonIgnore]
    private float? _colorEnhanceColorHsvGamma;
    [JsonProperty("_ColorEnhanceColorHSVGamma")]
    public float ColorEnhanceColorHsvGamma
    {
        get => Logic._colorEnhanceColorHsvGamma ?? _colorEnhanceColorHsvGamma ?? 0.0f;
        set => _colorEnhanceColorHsvGamma = value;
    }

    // Key cloning
    [JsonIgnore]
    private Dictionary<DeviceKeys, KeySequence>? _cloningMap;
    [JsonProperty("_CloningMap")]
    public Dictionary<DeviceKeys, KeySequence> CloningMap => Logic._cloningMap ?? _cloningMap ?? new Dictionary<DeviceKeys, KeySequence>();

    public WrapperLightsLayerHandlerProperties()
    { }

    public WrapperLightsLayerHandlerProperties(bool arg = false) : base(arg) { }

    public override void Default()
    {
        base.Default();
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();

        _colorEnhanceEnabled = true;
        _colorEnhanceMode = 0;
        _colorEnhanceColorFactor = 90;
        _colorEnhanceColorHsvSine = 0.1f;
        _colorEnhanceColorHsvGamma = 2.5f;
        _cloningMap = new Dictionary<DeviceKeys, KeySequence>();
    }
}
    
[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[LayerHandlerMeta(IsDefault = false)]
public class WrapperLightsLayerHandler : LayerHandler<WrapperLightsLayerHandlerProperties>
{
    private int[] _bitmap = new int[126];
    private readonly Dictionary<DeviceKeys, Color> _extraKeys = new();
    private Color _lastFillColor = Color.Black;
    private EntireEffect? _currentEffect;
    private readonly SolidBrush _fillBrush = new(Color.Transparent);

    public WrapperLightsLayerHandler() : base("Aurora Wrapper")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_WrapperLightsLayer(this);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        if (gamestate is not GameState_Wrapper)
            return EffectLayer.EmptyLayer;

        _fillBrush.Color = GetBoostedColor(_lastFillColor);
        EffectLayer.Fill(_fillBrush);

        var allKeys = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>().ToArray();
        foreach (var key in allKeys)
        {
            // This checks if a key is already being cloned over and thus should be prevented from being re-set by the
            // normal wrapper. Fixes issues with some clones not working. Thanks to @Gurjot95 for finding it :)
            if (Properties.CloningMap.Values.Any(sequence => sequence.Keys.Contains(key)))
                continue;

            if (!_extraKeys.ContainsKey(key)) continue;
            EffectLayer.Set(key, GetBoostedColor(_extraKeys[key]));

            // Do the key cloning
            if (Properties.CloningMap.TryGetValue(key, out var targetKey))
                EffectLayer.Set(targetKey, GetBoostedColor(_extraKeys[key]));
        }

        var currentTime = Time.GetMillisecondsSinceEpoch();
        _currentEffect?.SetEffect(EffectLayer, currentTime - _currentEffect.TimeStarted);

        return EffectLayer;
    }

    public override void SetGameState(IGameState gamestate)
    {
        if (gamestate is not GameState_Wrapper ngwState)
            return;

        if (ngwState.Sent_Bitmap.Length != 0)
            _bitmap = ngwState.Sent_Bitmap;

        SetExtraKey(DeviceKeys.LOGO, ngwState.Extra_Keys.logo);
        SetExtraKey(DeviceKeys.LOGO2, ngwState.Extra_Keys.badge);
        SetExtraKey(DeviceKeys.Peripheral, ngwState.Extra_Keys.peripheral);
        //Reversing the mousepad lights from left to right, razer takes it from right to left
        SetExtraKey(DeviceKeys.Peripheral, ngwState.Extra_Keys.peripheral);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT15, ngwState.Extra_Keys.mousepad1);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT14, ngwState.Extra_Keys.mousepad2);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT13, ngwState.Extra_Keys.mousepad3);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT12, ngwState.Extra_Keys.mousepad4);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT11, ngwState.Extra_Keys.mousepad5);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT10, ngwState.Extra_Keys.mousepad6);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT9, ngwState.Extra_Keys.mousepad7);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT8, ngwState.Extra_Keys.mousepad8);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT7, ngwState.Extra_Keys.mousepad9);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT6, ngwState.Extra_Keys.mousepad10);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT5, ngwState.Extra_Keys.mousepad11);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT4, ngwState.Extra_Keys.mousepad12);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT3, ngwState.Extra_Keys.mousepad13);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT2, ngwState.Extra_Keys.mousepad14);
        SetExtraKey(DeviceKeys.MOUSEPADLIGHT1, ngwState.Extra_Keys.mousepad15);
        SetExtraKey(DeviceKeys.G1, ngwState.Extra_Keys.G1);
        SetExtraKey(DeviceKeys.G2, ngwState.Extra_Keys.G2);
        SetExtraKey(DeviceKeys.G3, ngwState.Extra_Keys.G3);
        SetExtraKey(DeviceKeys.G4, ngwState.Extra_Keys.G4);
        SetExtraKey(DeviceKeys.G5, ngwState.Extra_Keys.G5);
        SetExtraKey(DeviceKeys.G6, ngwState.Extra_Keys.G6);
        SetExtraKey(DeviceKeys.G7, ngwState.Extra_Keys.G7);
        SetExtraKey(DeviceKeys.G8, ngwState.Extra_Keys.G8);
        SetExtraKey(DeviceKeys.G9, ngwState.Extra_Keys.G9);
        SetExtraKey(DeviceKeys.G10, ngwState.Extra_Keys.G10);
        SetExtraKey(DeviceKeys.G11, ngwState.Extra_Keys.G11);
        SetExtraKey(DeviceKeys.G12, ngwState.Extra_Keys.G12);
        SetExtraKey(DeviceKeys.G13, ngwState.Extra_Keys.G13);
        SetExtraKey(DeviceKeys.G14, ngwState.Extra_Keys.G14);
        SetExtraKey(DeviceKeys.G15, ngwState.Extra_Keys.G15);
        SetExtraKey(DeviceKeys.G16, ngwState.Extra_Keys.G16);
        SetExtraKey(DeviceKeys.G17, ngwState.Extra_Keys.G17);
        SetExtraKey(DeviceKeys.G18, ngwState.Extra_Keys.G18);
        SetExtraKey(DeviceKeys.G19, ngwState.Extra_Keys.G19);
        SetExtraKey(DeviceKeys.G20, ngwState.Extra_Keys.G20);

        switch (ngwState.Command)
        {
            //LightFX
            case "LFX_GetNumDevices":
            {
                //Retain previous lighting
                var fillColorInt = CommonColorUtils.GetIntFromColor(_lastFillColor);

                for (var i = 0; i < _bitmap.Length; i++)
                    _bitmap[i] = fillColorInt;

                foreach (var extraKey in _extraKeys.Keys.ToArray())
                    _extraKeys[extraKey] = _lastFillColor;
                break;
            }
            case "LFX_GetNumLights":
            {
                //Retain previous lighting
                var fillColorInt = CommonColorUtils.GetIntFromColor(_lastFillColor);

                for (var i = 0; i < _bitmap.Length; i++)
                    _bitmap[i] = fillColorInt;

                foreach (var extraKey in _extraKeys.Keys.ToArray())
                    _extraKeys[extraKey] = _lastFillColor;
                break;
            }
            case "LFX_Light":
            {
                //Retain previous lighting
                var fillColorInt = CommonColorUtils.GetIntFromColor(_lastFillColor);

                for (var i = 0; i < _bitmap.Length; i++)
                    _bitmap[i] = fillColorInt;

                foreach (var extraKey in _extraKeys.Keys.ToArray())
                    _extraKeys[extraKey] = _lastFillColor;
                break;
            }
            case "LFX_SetLightColor":
            {
                //Retain previous lighting
                var fillColorInt = CommonColorUtils.GetIntFromColor(_lastFillColor);

                for (var i = 0; i < _bitmap.Length; i++)
                    _bitmap[i] = fillColorInt;

                foreach (var extraKey in _extraKeys.Keys.ToArray())
                    _extraKeys[extraKey] = _lastFillColor;
                break;
            }
            case "LFX_Update":
            {
                var newFill = Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start);

                if (_lastFillColor != newFill)
                {
                    _lastFillColor = newFill;

                    for (var i = 0; i < _bitmap.Length; i++)
                    {
                        _bitmap[i] = (ngwState.Command_Data.red_start << 16) | (ngwState.Command_Data.green_start << 8) | ngwState.Command_Data.blue_start;
                    }
                }

                foreach (var extraKey in _extraKeys.Keys.ToArray())
                    _extraKeys[extraKey] = newFill;
                break;
            }
            case "LFX_SetLightActionColor":
            case "LFX_ActionColor":
            {
                var primary = Color.Transparent;
                var secondary = Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start);

                if (_currentEffect != null)
                    primary = _currentEffect.GetCurrentColor(Time.GetMillisecondsSinceEpoch() - _currentEffect.TimeStarted);

                _currentEffect = ngwState.Command_Data.effect_type switch
                {
                    "LFX_ACTION_COLOR" => new LFX_Color(primary),
                    "LFX_ACTION_PULSE" => new LFX_Pulse(primary, secondary, ngwState.Command_Data.duration),
                    "LFX_ACTION_MORPH" => new LFX_Morph(primary, secondary, ngwState.Command_Data.duration),
                    _ => null
                };

                break;
            }
            case "LFX_SetLightActionColorEx":
            case "LFX_ActionColorEx":
            {
                var primary = Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start);
                var secondary = Color.FromArgb(ngwState.Command_Data.red_end, ngwState.Command_Data.green_end, ngwState.Command_Data.blue_end);

                _currentEffect = ngwState.Command_Data.effect_type switch
                {
                    "LFX_ACTION_COLOR" => new LFX_Color(primary),
                    "LFX_ACTION_PULSE" => new LFX_Pulse(primary, secondary, ngwState.Command_Data.duration),
                    "LFX_ACTION_MORPH" => new LFX_Morph(primary, secondary, ngwState.Command_Data.duration),
                    _ => null
                };

                break;
            }
            case "LFX_Reset":
                _currentEffect = null;
                break;
            default:
                Global.logger.Information("Unknown Wrapper Command: {Command} Data: {Data}", ngwState.Command, ngwState.Command_Data);
                break;
        }
    }

    private Color GetBoostedColor(Color color)
    {
        if (!Properties.ColorEnhanceEnabled)
            return color;

        switch (Properties.ColorEnhanceMode)
        {
            case 0:
                var boostAmount = 0.0f;
                boostAmount += 1.0f - color.R / Properties.ColorEnhanceColorFactor;
                boostAmount += 1.0f - color.G / Properties.ColorEnhanceColorFactor;
                boostAmount += 1.0f - color.B / Properties.ColorEnhanceColorFactor;

                boostAmount = boostAmount <= 1.0f ? 1.0f : boostAmount;

                return ColorUtils.MultiplyColorByScalar(color, boostAmount);

            case 1:
                CommonColorUtils.ToHsv(color, out var hue, out var saturation, out var value);
                var x = Properties.ColorEnhanceColorHsvSine;
                var y = 1.0f / Properties.ColorEnhanceColorHsvGamma;
                value = (float)Math.Min(1, Math.Pow(x * Math.Sin(2 * Math.PI * value) + value, y));
                return CommonColorUtils.FromHsv(hue, saturation, value);

            default:
                return color;
        }
    }

    private void SetExtraKey(DeviceKeys key, Color color)
    {
        _extraKeys[key] = color;
    }
}

internal class EntireEffect
{
    protected Color Color;
    protected readonly long Duration;
    public long Interval;
    public readonly long TimeStarted;

    protected EntireEffect(Color color, long duration, long interval)
    {
        Color = color;
        Duration = duration;
        Interval = interval;
        TimeStarted = Time.GetMillisecondsSinceEpoch();
    }

    public virtual Color GetCurrentColor(long time)
    {
        return Color;
    }

    public void SetEffect(EffectLayer layer, long time)
    {
        layer.FillOver(GetCurrentColor(time));
    }
}

internal class LFX_Color : EntireEffect
{
    public LFX_Color(Color color) : base(color, 0, 0)
    {
    }
}

internal class LFX_Pulse : EntireEffect
{
    private readonly Color _secondary;

    public LFX_Pulse(Color primary, Color secondary, int duration) : base(primary, duration, 0)
    {
        _secondary = secondary;
    }

    public override Color GetCurrentColor(long time)
    {
        return ColorUtils.MultiplyColorByScalar(Color, Math.Pow(Math.Sin(time / 1000.0D * Math.PI), 2.0));
    }
}

internal class LFX_Morph : EntireEffect
{
    private readonly Color _secondary;

    public LFX_Morph(Color primary, Color secondary, int duration) : base(primary, duration, 0)
    {
        _secondary = secondary;
    }

    public override Color GetCurrentColor(long time)
    {
        return time - TimeStarted >= Duration ? _secondary : ColorUtils.BlendColors(Color, _secondary, (time - TimeStarted) % Duration);
    }
}