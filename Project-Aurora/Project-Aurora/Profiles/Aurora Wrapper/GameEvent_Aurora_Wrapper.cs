using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Utils;

namespace Aurora.Profiles.Aurora_Wrapper;

public class GameEvent_Aurora_Wrapper : LightEvent
{
    private int[] _bitmap = new int[126];
    private readonly Dictionary<DeviceKeys, Color> _extraKeys = new();
    private Color _lastFillColor = Color.Black;
    private EntireEffect? _currentEffect;

    private readonly bool _colorEnhanceEnabled = false;
    private readonly int _colorEnhanceMode = 0;
    private readonly int _colorEnhanceColorFactor = 90;
    private readonly float _colorEnhanceColorHsvSine = 0.1f;
    private readonly float _colorEnhanceColorHsvGamma = 2.5f;

    public sealed override void UpdateLights(EffectFrame frame)
    {
    }

    public override void SetGameState(IGameState newGameState)
    {
        UpdateWrapperLights(newGameState);
    }

    protected virtual void UpdateWrapperLights(IGameState newGameState)
    {
        if (newGameState is not GameState_Wrapper ngwState) return;
        _game_state = ngwState;

        if(ngwState.Sent_Bitmap.Length != 0)
            _bitmap = ngwState.Sent_Bitmap;

        SetExtraKey(DeviceKeys.LOGO, ngwState.Extra_Keys.logo);
        SetExtraKey(DeviceKeys.LOGO2, ngwState.Extra_Keys.badge);
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
                var newfill = Color.FromArgb(ngwState.Command_Data.red_start, ngwState.Command_Data.green_start, ngwState.Command_Data.blue_start);

                if (!_lastFillColor.Equals(newfill))
                {
                    _lastFillColor = newfill;

                    for (var i = 0; i < _bitmap.Length; i++)
                    {
                        _bitmap[i] = (ngwState.Command_Data.red_start << 16) | (ngwState.Command_Data.green_start << 8) | ngwState.Command_Data.blue_start;
                    }
                }

                foreach (var extraKey in _extraKeys.Keys.ToArray())
                    _extraKeys[extraKey] = newfill;
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
                Global.logger.Information("Unknown Wrapper Command: {Command}", ngwState.Command);
                break;
        }
    }

    private float[] RgbToHsv(Color colorRgb)
    {
        var r = colorRgb.R / 255.0f;
        var g = colorRgb.G / 255.0f;
        var b = colorRgb.B / 255.0f;

        var M = Math.Max(Math.Max(r, g), b);
        var m = Math.Min(Math.Min(r, g), b);
        var c = M - m;

        var h = 0.0f;
        if (M == r)
            h = (g - b) / c % 6;
        else if (M == g)
            h = (b - r) / c + 2;
        else if (M == b)
            h = (r - g) / c + 4;
        h *= 60.0f;
        if (h < 0.0f)
            h += 360.0f;

        var v = M;
        float s = 0;
        if (v != 0)
            s = c / v;

        return new[] { h, s, v };
    }

    private Color HsvToRgb(IReadOnlyList<float> colorHsv)
    {
        var h = colorHsv[0] / 60.0;
        var s = colorHsv[1];
        var v = colorHsv[2];

        var c = v * s;

        float[] rgb = { 0, 0, 0 };

        var x = (float)(c * (1 - Math.Abs(h % 2 - 1)));

        var i = (int)Math.Floor(h);
        switch (i)
        {
            case 0:
            case 6:
                rgb = new[] { c, x, 0 };
                break;
            case 1:
                rgb = new[] { x, c, 0 };
                break;
            case 2:
                rgb = new[] { 0, c, x };
                break;
            case 3:
                rgb = new[] { 0, x, c };
                break;
            case 4:
                rgb = new[] { x, 0, c };
                break;
            case 5:
            case -1:
                rgb = new[] { c, 0, x };
                break;
        }
        var m = v - c;

        return Color.FromArgb(Clamp((int)((rgb[0] + m) * 255 + 0.5f)), Clamp((int)((rgb[1] + m) * 255 + 0.5f)), Clamp((int)((rgb[2] + m) * 255 + 0.5f)));
    }

    private int Clamp(int n)
    {
        return n switch
        {
            <= 0 => 0,
            >= 255 => 255,
            _ => n
        };
    }

    private Color GetBoostedColor(Color color)
    {
        if (!_colorEnhanceEnabled)
            return color;

        switch (_colorEnhanceMode)
        {
            case 0:
                var boostAmount = 0.0f;
                boostAmount += 1.0f - color.R / _colorEnhanceColorFactor;
                boostAmount += 1.0f - color.G / _colorEnhanceColorFactor;
                boostAmount += 1.0f - color.B / _colorEnhanceColorFactor;

                boostAmount = boostAmount <= 1.0f ? 1.0f : boostAmount;

                return ColorUtils.MultiplyColorByScalar(color, boostAmount);

            case 1:
                var colorHsv = RgbToHsv(color);
                var x = _colorEnhanceColorHsvSine;
                var y = 1.0f / _colorEnhanceColorHsvGamma;
                colorHsv[2] = (float)Math.Min(1, Math.Pow(x * Math.Sin(2 * Math.PI * colorHsv[2]) + colorHsv[2], y));
                return HsvToRgb(colorHsv);

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
    public readonly long TimeStarted;

    protected Color Color;
    protected readonly long Duration;
    protected readonly long Interval;

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

class LFX_Color : EntireEffect
{
    public LFX_Color(Color color) : base(color, 0, 0)
    {
    }
}

class LFX_Pulse : EntireEffect
{
    private Color _secondary;

    public LFX_Pulse(Color primary, Color secondary, int duration) : base(primary, duration, 0)
    {
        _secondary = secondary;
    }

    public override Color GetCurrentColor(long time)
    {
        return ColorUtils.MultiplyColorByScalar(Color, Math.Pow(Math.Sin((time / 1000.0D) * Math.PI), 2.0));
    }
}

class LFX_Morph : EntireEffect
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