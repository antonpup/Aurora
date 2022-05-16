using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using LedCSharp;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers
{
    public class WrapperLightsLayerHandlerProperties : LayerHandlerProperties<WrapperLightsLayerHandlerProperties>
    {
        // Color Enhancing
        public bool? _ColorEnhanceEnabled { get; set; }
        [JsonIgnore]
        public bool ColorEnhanceEnabled { get { return Logic._ColorEnhanceEnabled ?? _ColorEnhanceEnabled ?? false; } }

        public int? _ColorEnhanceMode { get; set; }
        [JsonIgnore]
        public int ColorEnhanceMode { get { return Logic._ColorEnhanceMode ?? _ColorEnhanceMode ?? 0; } }

        public int? _ColorEnhanceColorFactor { get; set; }
        [JsonIgnore]
        public int ColorEnhanceColorFactor { get { return Logic._ColorEnhanceColorFactor ?? _ColorEnhanceColorFactor ?? 0; } }

        public float? _ColorEnhanceColorHSVSine { get; set; }
        [JsonIgnore]
        public float ColorEnhanceColorHSVSine { get { return Logic._ColorEnhanceColorHSVSine ?? _ColorEnhanceColorHSVSine ?? 0.0f; } }

        public float? _ColorEnhanceColorHSVGamma { get; set; }
        [JsonIgnore]
        public float ColorEnhanceColorHSVGamma { get { return Logic._ColorEnhanceColorHSVGamma ?? _ColorEnhanceColorHSVGamma ?? 0.0f; } }

        // Key cloning
        [JsonIgnore]
        public Dictionary<DeviceKey, KeySequence> CloningMap => Logic._CloningMap ?? _CloningMap ?? new Dictionary<DeviceKey, KeySequence>();
        public Dictionary<DeviceKey, KeySequence> _CloningMap { get; set; }

        public WrapperLightsLayerHandlerProperties()
        { }

        public WrapperLightsLayerHandlerProperties(bool arg = false) : base(arg) { }

        public override void Default()
        {
            base.Default();
            _PrimaryColor = ColorUtils.GenerateRandomColor();

            _ColorEnhanceEnabled = true;
            _ColorEnhanceMode = 0;
            _ColorEnhanceColorFactor = 90;
            _ColorEnhanceColorHSVSine = 0.1f;
            _ColorEnhanceColorHSVGamma = 2.5f;
            _CloningMap = new Dictionary<DeviceKey, KeySequence>();
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
        private readonly Dictionary<DeviceKeys, KeyEffect> _keyEffects = new();
        private EntireEffect _currentEffect;

        protected override UserControl CreateControl()
        {
            return new Control_WrapperLightsLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            if (gamestate is not GameState_Wrapper)
                return EffectLayer.EmptyLayer;

            var layers = new Queue<EffectLayer>();

            var colorFillLayer = new EffectLayer("Aurora Wrapper - Color Fill", GetBoostedColor(_lastFillColor));

            layers.Enqueue(colorFillLayer);

            var bitmapLayer = new EffectLayer("Aurora Wrapper - Bitmap");

            var allKeys = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>().ToArray();
            foreach (var key in allKeys)
            {

                // This checks if a key is already being cloned over and thus should be prevented from being re-set by the
                // normal wrapper. Fixes issues with some clones not working. Thanks to @Gurjot95 for finding it :)
                if (Properties.CloningMap.Values.Any(sequence => sequence.keys.Contains(key)))
                    continue;


                if (_extraKeys.ContainsKey(key))
                {
                    bitmapLayer.Set(key, GetBoostedColor(_extraKeys[key]));

                    // Do the key cloning
                    if (Properties.CloningMap.ContainsKey(key))
                        bitmapLayer.Set(Properties.CloningMap[key], GetBoostedColor(_extraKeys[key]));
                }
                else
                {
                    var logiKey = DeviceKeysUtils.ToLogitechBitmap(key);

                    if (logiKey == Logitech_keyboardBitmapKeys.UNKNOWN || _bitmap.Length <= 0) continue;
                    var color = GetBoostedColor(ColorUtils.GetColorFromInt(_bitmap[(int)logiKey / 4]));
                    bitmapLayer.Set(key, color);

                    // Key cloning
                    if (Properties.CloningMap.ContainsKey(key))
                        bitmapLayer.Set(Properties.CloningMap[key], color);
                }
            }

            layers.Enqueue(bitmapLayer);

            var effectsLayer = new EffectLayer("Aurora Wrapper - Effects");

            var effectKeys = _keyEffects.Keys.ToArray();
            var currentTime = Time.GetMillisecondsSinceEpoch();

            foreach (var key in effectKeys)
            {
                if (_keyEffects[key].duration != 0 && _keyEffects[key].timeStarted + _keyEffects[key].duration <= currentTime)
                {
                    _keyEffects.Remove(key);
                }
                else
                {
                    switch (_keyEffects[key])
                    {
                        case LogiFlashSingleKey:
                            var logiFlashSingleKey = _keyEffects[key] as LogiFlashSingleKey;
                            effectsLayer.Set(logiFlashSingleKey.key, GetBoostedColor(logiFlashSingleKey.GetColor(currentTime - logiFlashSingleKey.timeStarted)));
                            break;
                        case LogiPulseSingleKey:
                            var logiPulseSingleKey = _keyEffects[key] as LogiPulseSingleKey;
                            effectsLayer.Set(logiPulseSingleKey.key, GetBoostedColor(logiPulseSingleKey.GetColor(currentTime - logiPulseSingleKey.timeStarted)));
                            break;
                        default:
                            effectsLayer.Set(_keyEffects[key].key, GetBoostedColor(_keyEffects[key].GetColor(currentTime - _keyEffects[key].timeStarted)));
                            break;
                    }
                }
            }

            layers.Enqueue(effectsLayer);

            var entireEffectLayer = new EffectLayer("Aurora Wrapper - EntireKB effect");

            if (_currentEffect != null)
            {
                switch (_currentEffect)
                {
                    case LogiFlashLighting lighting:
                        lighting.SetEffect(entireEffectLayer, currentTime - lighting.timeStarted);
                        break;
                    case LogiPulseLighting pulseLighting:
                        pulseLighting.SetEffect(entireEffectLayer, currentTime - pulseLighting.timeStarted);
                        break;
                    default:
                        _currentEffect.SetEffect(entireEffectLayer, currentTime - _currentEffect.timeStarted);
                        break;
                }
            }

            layers.Enqueue(entireEffectLayer);

            var layersArray = layers.ToArray();
            var finalLayer = layersArray[0];
            for (var i = 1; i < layersArray.Length; i++)
                finalLayer += layersArray[i];

            return finalLayer;
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
                case "SetLighting":
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

                    break;
                }
                case "SetLightingForKeyWithKeyName":
                case "SetLightingForKeyWithScanCode":
                {
                    var bitmapKey = DeviceKeysUtils.ToLogitechBitmap((keyboardNames)ngwState.Command_Data.key);

                    if (bitmapKey != Logitech_keyboardBitmapKeys.UNKNOWN)
                    {
                        _bitmap[(int)bitmapKey / 4] = (ngwState.Command_Data.red_start << 16) | (ngwState.Command_Data.green_start << 8) | ngwState.Command_Data.blue_start;
                    }

                    break;
                }
                case "FlashSingleKey":
                {
                    var devKey = DeviceKeysUtils.ToDeviceKey((keyboardNames)ngwState.Command_Data.key);
                    var newEffect = new LogiFlashSingleKey(devKey, Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start),
                        ngwState.Command_Data.duration,
                        ngwState.Command_Data.interval
                    );

                    if (_keyEffects.ContainsKey(devKey))
                        _keyEffects[devKey] = newEffect;
                    else
                        _keyEffects.Add(devKey, newEffect);
                    break;
                }
                case "PulseSingleKey":
                {
                    var devKey = DeviceKeysUtils.ToDeviceKey((keyboardNames)ngwState.Command_Data.key);
                    long duration = ngwState.Command_Data.interval == 0 ? 0 : ngwState.Command_Data.duration;

                    var newEffect = new LogiPulseSingleKey(devKey, Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start),
                        Color.FromArgb(ngwState.Command_Data.red_end, ngwState.Command_Data.green_end, ngwState.Command_Data.blue_end),
                        duration
                    );

                    if (_keyEffects.ContainsKey(devKey))
                        _keyEffects[devKey] = newEffect;
                    else
                        _keyEffects.Add(devKey, newEffect);
                    break;
                }
                case "PulseLighting":
                    _currentEffect = new LogiPulseLighting(
                        Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start),
                        ngwState.Command_Data.duration,
                        ngwState.Command_Data.interval
                    );
                    break;
                case "FlashLighting":
                    _currentEffect = new LogiFlashLighting(
                        Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start),
                        ngwState.Command_Data.duration,
                        ngwState.Command_Data.interval
                    );
                    break;
                case "StopEffects":
                    _keyEffects.Clear();
                    _currentEffect = null;
                    break;
                case "SetLightingFromBitmap":
                    break;
                //LightFX
                case "LFX_GetNumDevices":
                {
                    //Retain previous lighting
                    var fillColorInt = ColorUtils.GetIntFromColor(_lastFillColor);

                    for (var i = 0; i < _bitmap.Length; i++)
                        _bitmap[i] = fillColorInt;

                    foreach (var extraKey in _extraKeys.Keys.ToArray())
                        _extraKeys[extraKey] = _lastFillColor;
                    break;
                }
                case "LFX_GetNumLights":
                {
                    //Retain previous lighting
                    var fillColorInt = ColorUtils.GetIntFromColor(_lastFillColor);

                    for (var i = 0; i < _bitmap.Length; i++)
                        _bitmap[i] = fillColorInt;

                    foreach (var extraKey in _extraKeys.Keys.ToArray())
                        _extraKeys[extraKey] = _lastFillColor;
                    break;
                }
                case "LFX_Light":
                {
                    //Retain previous lighting
                    var fillColorInt = ColorUtils.GetIntFromColor(_lastFillColor);

                    for (var i = 0; i < _bitmap.Length; i++)
                        _bitmap[i] = fillColorInt;

                    foreach (var extraKey in _extraKeys.Keys.ToArray())
                        _extraKeys[extraKey] = _lastFillColor;
                    break;
                }
                case "LFX_SetLightColor":
                {
                    //Retain previous lighting
                    var fillColorInt = ColorUtils.GetIntFromColor(_lastFillColor);

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
                        primary = _currentEffect.GetCurrentColor(Time.GetMillisecondsSinceEpoch() - _currentEffect.timeStarted);

                    switch (ngwState.Command_Data.effect_type)
                    {
                        case "LFX_ACTION_COLOR":
                            _currentEffect = new LFX_Color(primary);
                            break;
                        case "LFX_ACTION_PULSE":
                            _currentEffect = new LFX_Pulse(primary, secondary, ngwState.Command_Data.duration);
                            break;
                        case "LFX_ACTION_MORPH":
                            _currentEffect = new LFX_Morph(primary, secondary, ngwState.Command_Data.duration);
                            break;
                        default:
                            _currentEffect = null;
                            break;
                    }

                    break;
                }
                case "LFX_SetLightActionColorEx":
                case "LFX_ActionColorEx":
                {
                    var primary = Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start);
                    var secondary = Color.FromArgb(ngwState.Command_Data.red_end, ngwState.Command_Data.green_end, ngwState.Command_Data.blue_end);

                    switch (ngwState.Command_Data.effect_type)
                    {
                        case "LFX_ACTION_COLOR":
                            _currentEffect = new LFX_Color(primary);
                            break;
                        case "LFX_ACTION_PULSE":
                            _currentEffect = new LFX_Pulse(primary, secondary, ngwState.Command_Data.duration);
                            break;
                        case "LFX_ACTION_MORPH":
                            _currentEffect = new LFX_Morph(primary, secondary, ngwState.Command_Data.duration);
                            break;
                        default:
                            _currentEffect = null;
                            break;
                    }

                    break;
                }
                case "LFX_Reset":
                    _currentEffect = null;
                    break;
                //Razer
                case "CreateMouseEffect":
                    break;
                case "CreateMousepadEffect":
                    break;
                case "CreateKeyboardEffect":
                {
                    var primary = Color.Red;
                    var secondary = Color.Blue;

                    if (ngwState.Command_Data.red_start >= 0 &&
                        ngwState.Command_Data.green_start >= 0 &&
                        ngwState.Command_Data.blue_start >= 0
                       )
                        primary = Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start);

                    if (ngwState.Command_Data.red_end >= 0 &&
                        ngwState.Command_Data.green_end >= 0 &&
                        ngwState.Command_Data.blue_end >= 0
                       )
                        secondary = Color.FromArgb(ngwState.Command_Data.red_end, ngwState.Command_Data.green_end, ngwState.Command_Data.blue_end);

                    _currentEffect = ngwState.Command_Data.effect_type switch
                    {
                        "CHROMA_BREATHING" => new CHROMA_BREATHING(primary, secondary, ngwState.Command_Data.effect_config),
                        _ => null
                    };

                    break;
                }
                default:
                    Global.logger.Info("Unknown Wrapper Command: " + ngwState.Command);
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
                    ColorUtils.ToHsv(color, out var hue, out var saturation, out var value);
                    var x = Properties.ColorEnhanceColorHSVSine;
                    var y = 1.0f / Properties.ColorEnhanceColorHSVGamma;
                    value = (float)Math.Min(1, Math.Pow(x * Math.Sin(2 * Math.PI * value) + value, y));
                    return ColorUtils.FromHsv(hue, saturation, value);

                default:
                    return color;
            }
        }

        private void SetExtraKey(DeviceKeys key, Color color)
        {
            if (!_extraKeys.ContainsKey(key))
                _extraKeys.Add(key, color);
            else
                _extraKeys[key] = color;
        }
    }

    class KeyEffect
    {
        public DeviceKeys key;
        public Color color;
        public long duration;
        public long interval;
        public long timeStarted;

        public KeyEffect(DeviceKeys key, Color color, long duration, long interval)
        {
            this.key = key;
            this.color = color;
            this.duration = duration;
            this.interval = interval;
            timeStarted = Time.GetMillisecondsSinceEpoch();
        }

        public virtual Color GetColor(long time)
        {
            return Color.Red; //Red for debug
        }
    }

    class LogiFlashSingleKey : KeyEffect
    {
        public LogiFlashSingleKey(DeviceKeys key, Color color, long duration, long interval) : base(key, color, duration, interval)
        {
        }

        public override Color GetColor(long time)
        {
            return ColorUtils.MultiplyColorByScalar(color, Math.Round(Math.Pow(Math.Sin(time / (double)interval * Math.PI), 2.0)));
        }
    }

    class LogiPulseSingleKey : KeyEffect
    {
        private readonly Color _colorEnd;

        public LogiPulseSingleKey(DeviceKeys key, Color color, Color colorEnd, long duration) : base(key, color, duration, 1)
        {
            _colorEnd = colorEnd;
        }

        public override Color GetColor(long time)
        {
            return ColorUtils.BlendColors(color, _colorEnd, Math.Pow(Math.Sin(time / (duration == 0 ? 1000.0D : duration) * Math.PI), 2.0));
        }
    }

    class EntireEffect
    {
        public Color color;
        public long duration;
        public long interval;
        public long timeStarted;

        public EntireEffect(Color color, long duration, long interval)
        {
            this.color = color;
            this.duration = duration;
            this.interval = interval;
            timeStarted = Time.GetMillisecondsSinceEpoch();
        }

        public virtual Color GetCurrentColor(long time)
        {
            return color;
        }

        public void SetEffect(EffectLayer layer, long time)
        {
            layer.FillOver(GetCurrentColor(time));
        }
    }

    class LogiFlashLighting : EntireEffect
    {
        public LogiFlashLighting(Color color, long duration, long interval) : base(color, duration, interval)
        {
        }

        public override Color GetCurrentColor(long time)
        {
            return ColorUtils.MultiplyColorByScalar(color, Math.Round(Math.Pow(Math.Sin(time / (double)interval * Math.PI), 2.0)));
        }
    }

    class LogiPulseLighting : EntireEffect
    {
        public LogiPulseLighting(Color color, long duration, long interval) : base(color, duration, interval)
        {
        }

        public override Color GetCurrentColor(long time)
        {
            return ColorUtils.MultiplyColorByScalar(color, Math.Pow(Math.Sin(time / 1000.0D * Math.PI), 2.0));
        }
    }

    class LFX_Color : EntireEffect
    {
        public LFX_Color(Color color) : base(color, 0, 0)
        {
        }
    }

    class LFX_Pulse : EntireEffect
    {
        private Color secondary;

        public LFX_Pulse(Color primary, Color secondary, int duration) : base(primary, duration, 0)
        {
            this.secondary = secondary;
        }

        public override Color GetCurrentColor(long time)
        {
            return ColorUtils.MultiplyColorByScalar(color, Math.Pow(Math.Sin(time / 1000.0D * Math.PI), 2.0));
        }
    }

    class LFX_Morph : EntireEffect
    {
        private Color secondary;

        public LFX_Morph(Color primary, Color secondary, int duration) : base(primary, duration, 0)
        {
            this.secondary = secondary;
        }

        public override Color GetCurrentColor(long time)
        {
            if (time - timeStarted >= duration)
                return secondary;
            return ColorUtils.BlendColors(color, secondary, (time - timeStarted) % duration);
        }
    }

    class CHROMA_BREATHING : EntireEffect
    {
        private Color secondary;
        private enum BreathingType
        {
            TWO_COLORS,
            RANDOM_COLORS,
            INVALID
        }
        private BreathingType type;

        public CHROMA_BREATHING(Color primary, Color secondary, string config) : base(primary, 0, 0)
        {
            this.secondary = secondary;

            switch (config)
            {
                case "TWO_COLORS":
                    type = BreathingType.TWO_COLORS;
                    break;
                case "RANDOM_COLORS":
                    type = BreathingType.RANDOM_COLORS;
                    color = ColorUtils.GenerateRandomColor();
                    secondary = ColorUtils.GenerateRandomColor();
                    break;
                default:
                    type = BreathingType.INVALID;
                    break;
            }

        }

        public override Color GetCurrentColor(long time)
        {
            double blend_val = Math.Pow(Math.Sin(time / 1000.0D * Math.PI), 2.0);

            if (type == BreathingType.RANDOM_COLORS)
            {
                if (blend_val >= 0.95 && blend_val >= 1.0)
                    color = ColorUtils.GenerateRandomColor();
                else if (blend_val >= 0.5 && blend_val >= 0.0)
                    secondary = ColorUtils.GenerateRandomColor();
            }

            return ColorUtils.BlendColors(color, secondary, blend_val);
        }
    }
}
