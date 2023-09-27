using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Common.Devices;
using Newtonsoft.Json;
using RGB.NET.Devices.CorsairLegacy;
using Application = Aurora.Profiles.Application;

namespace Aurora.Settings;

public static class ProfileImporter
{

    /// <summary>
    /// Imports a file from disk as a profile into the given application.
    /// </summary>
    /// <param name="filepath">The full filepath of the file to import.</param>
    public static void ImportProfile(this Application app, string filepath)
    {
        var fn = filepath.ToLower();

        if (fn.EndsWith(".cueprofile") || fn.EndsWith(".cuefolder"))
            app.ImportCueprofile(filepath);
        else if (fn.EndsWith(".json"))
            app.ImportJson(filepath);
    }

    /// <summary>
    /// Imports a cue profile file to the given application.
    /// </summary>
    private static void ImportCueprofile(this Application app, string filepath)
    {
        var rootElement = XElement.Load(filepath);
        var valueElement = filepath.EndsWith(".cueprofile") ? rootElement : rootElement.Element("profile_folder").Element("profiles");

        foreach (var value in valueElement.Elements())
        {
            var profileElement = value.Element("profile");
            if (profileElement == null) continue;
            var profileName = profileElement.Element("name").Value;
            app?.AddNewProfile(profileName);

            foreach (var property in profileElement.Element("properties").Elements())
            {
                if (!"Keyboard".Equals(property.Element("key").Value)) continue;
                foreach (var profProperty in property.Element("value").Element("properties").Descendants())
                {
                    if (!profProperty.Name.ToString().Equals("keys")) continue;
                    var hasValue = profProperty.Element("value0");
                    if (hasValue == null) continue;
                    var layers = profProperty.Parent.Parent.Parent.Parent.Parent;
                    app.Profile.Layers.Clear();

                    var _basePolyID = 2147483648;
                    var _definedPolyIDS = new Dictionary<uint, string>();

                    foreach (var layer in layers.Elements())
                    {
                        var keysAndStuff = layer.Element("ptr_wrapper").Element("data").Element("base");

                        var layerName = keysAndStuff.Element("name").Value;
                        var layerEnabled = bool.Parse(keysAndStuff.Element("enabled").Value);
                        var repeatTimes = Math.Max(int.Parse(keysAndStuff.Element("executionHints").Element("stopAfterTimes").Value), 0);
                        var stopOnRelease = bool.Parse(keysAndStuff.Element("executionHints").Element("stopOnKeyRelease").Value);
                        var stopOnPress = bool.Parse(keysAndStuff.Element("executionHints").Element("stopOnKeyPress").Value);
                        var startOnKeyPress = keysAndStuff.Element("executionHints").Element("startOnKeyPress").Value;
                        var playOption = keysAndStuff.Element("executionHints").Element("playOption").Value;
                        var RippleEffectCheck = false;
                        var playFromKey = false;
                        var stopImmediately = false;
                        var triggerMode = AnimationTriggerMode.OnKeyPress;
                        if (playOption.Equals("PlayFromKeyCenter"))
                        {
                            playFromKey = true;
                        }
                        if (startOnKeyPress.Equals("true"))
                        {
                            RippleEffectCheck = true;
                            if (stopOnRelease || stopOnPress)
                            {
                                stopImmediately = true;
                                repeatTimes = 1;
                                triggerMode = AnimationTriggerMode.WhileKeyHeld;
                            }
                        }
                        var affected_keys = new KeySequence();
                        var bindings = new List<Keybind>();
                        foreach (var key in keysAndStuff.Element("keys").Elements())
                        {
                            try
                            {
                                CorsairLedId? keyValue = null;

                                switch (key.Value)
                                {
                                    case "0":
                                        keyValue = CorsairLedId.D0;
                                        break;
                                    case "1":
                                        keyValue = CorsairLedId.D1;
                                        break;
                                    case "2":
                                        keyValue = CorsairLedId.D2;
                                        break;
                                    case "3":
                                        keyValue = CorsairLedId.D3;
                                        break;
                                    case "4":
                                        keyValue = CorsairLedId.D4;
                                        break;
                                    case "5":
                                        keyValue = CorsairLedId.D5;
                                        break;
                                    case "6":
                                        keyValue = CorsairLedId.D6;
                                        break;
                                    case "7":
                                        keyValue = CorsairLedId.D7;
                                        break;
                                    case "8":
                                        keyValue = CorsairLedId.D8;
                                        break;
                                    case "9":
                                        keyValue = CorsairLedId.D9;
                                        break;
                                    case "Led_KeyboardLogo":
                                        keyValue = CorsairLedId.Logo;
                                        break;
                                    case "Led_KeyboardTopLogo2":
                                        keyValue = CorsairLedId.Logo;
                                        break;
                                    default:
                                        if (key.Value.StartsWith("Led_Top"))
                                            key.Value = "G18";
                                        if(Enum.TryParse<CorsairLedId>(key.Value, out var corsairLedId)){
                                            keyValue = corsairLedId;
                                        }
                                        else
                                        {
                                            Global.logger.Warning("CorsairLedId not mapped, skipping {Value}", key.Value);
                                        }
                                        break;
                                }

                                if (!keyValue.HasValue || !(Enum.IsDefined(typeof(CorsairLedId), keyValue) |
                                                            keyValue.ToString().Contains(","))) continue;
                                var deviceKey = ToDeviceKeys(keyValue.Value);

                                if (deviceKey == DeviceKeys.NONE) continue;
                                affected_keys.Keys.Add(deviceKey);
                                bindings.Add(new Keybind(new[] { KeyUtils.GetFormsKey(deviceKey) }));
                            }
                            catch (Exception exception)
                            {
                                Global.logger.Error(exception, "Exception in profile");
                                //break;
                            }
                        }

                        var lightingInfo = layer.Element("ptr_wrapper").Element("data").Element("lighting");
                        if (lightingInfo == null)
                        {
                            break;
                        }

                        var transitionInfo = lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions") ??
                                             lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("transitions");
                        var layerPolyId = lightingInfo.Element("polymorphic_id");
                        var layerPolyName = lightingInfo.Element("polymorphic_name")?.Value;

                        if (string.IsNullOrWhiteSpace(layerPolyName))
                        {
                            if (_definedPolyIDS.ContainsKey(uint.Parse(layerPolyId.Value)))
                                layerPolyName = _definedPolyIDS[uint.Parse(layerPolyId.Value)];
                            var waveCheck = lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity");
                            var rippleCheck = lightingInfo.Element("ptr_wrapper").Element("data").Element("waveSpread");

                            if (rippleCheck != null)
                            {
                                layerPolyName = "RippleLighting";
                            }
                            else if (waveCheck != null)
                            {
                                layerPolyName = "WaveLighting";

                            }
                            else
                            {
                                var gradientCheck = transitionInfo.Element("value1");
                                layerPolyName = gradientCheck != null ? "GradientLighting" : "StaticLighting";
                            }
                        }
                        else
                            _definedPolyIDS.Add(uint.Parse(layerPolyId.Value) - _basePolyID, layerPolyName);

                        switch (layerPolyName)
                        {
                            case "StaticLighting":
                                app.Profile.Layers.Add(new Layer
                                {
                                    Name = layerName,
                                    Enabled = layerEnabled,
                                    Handler = new SolidColorLayerHandler
                                    {
                                        Properties = new LayerHandlerProperties
                                        {
                                            _Sequence = affected_keys,
                                            _PrimaryColor = ColorTranslator.FromHtml(transitionInfo.Element("value0").Element("color").Value),
                                            _LayerOpacity = int.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("opacity").Value) / 255.0f
                                        },
                                    }
                                });
                                break;
                            case "GradientLighting":
                            {
                                var duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);
                                var animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                var transitions = new Dictionary<float, Color>();

                                foreach (var transition in transitionInfo.Elements())
                                {
                                    try
                                    {
                                        var time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                        var color = ColorTranslator.FromHtml(transition.Element("color").Value);

                                        if (transitions.ContainsKey(time * (duration / 1000.0f)))
                                            transitions.Add(time * (duration / 1000.0f) + 0.000001f, color);
                                        else
                                            transitions.Add(time * (duration / 1000.0f), color);
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                for (var x = 0; x < transitions.Count; x += 2)
                                {
                                    var transitionDuration = 0.0f;

                                    if (x + 1 != transitions.Count)
                                        transitionDuration = transitions.Keys.ElementAt(x + 1) - transitions.Keys.ElementAt(x);

                                    animTrack.SetFrame(transitions.Keys.ElementAt(x), new AnimationFill(transitions[transitions.Keys.ElementAt(x)], transitionDuration));
                                }

                                app.Profile.Layers.Add(new Layer
                                {
                                    Name = layerName,
                                    Enabled = layerEnabled,
                                    Handler = new AnimationLayerHandler
                                    {
                                        Properties = new AnimationLayerHandlerProperties
                                        {
                                            _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                            _Sequence = affected_keys,
                                            _forceKeySequence = true,
                                            _scaleToKeySequenceBounds = true,
                                            _AnimationDuration = duration / 1000.0f,
                                            _AnimationRepeat = repeatTimes,
                                            _TriggerMode = RippleEffectCheck ? triggerMode : AnimationTriggerMode.AlwaysOn,
                                            //  _TriggerAnyKey = RippleEffectCheck,
                                            _TriggerKeySequence = affected_keys,
                                            _StackMode = RippleEffectCheck ? AnimationStackMode.Reset : AnimationStackMode.Ignore,
                                            _WhileKeyHeldTerminateRunning = stopImmediately,
                                            _KeyTriggerTranslate = playFromKey
                                        }
                                    }
                                });
                                break;
                            }
                            case "SolidLighting":
                            {
                                var duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);
                                var animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                var transitions = new Dictionary<float, Color>();

                                foreach (var transition in transitionInfo.Elements())
                                {
                                    try
                                    {
                                        var time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                        var color = ColorTranslator.FromHtml(transition.Element("color").Value);

                                        if (transitions.ContainsKey(time * (duration / 1000.0f)))
                                            transitions.Add(time * (duration / 1000.0f) + 0.000001f, color);
                                        else
                                            transitions.Add(time * (duration / 1000.0f), color);
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                for (var x = 0; x < transitions.Count; x += 2)
                                {
                                    var transitionDuration = transitions.Keys.ElementAt(x + 1) - transitions.Keys.ElementAt(x);

                                    animTrack.SetFrame(transitions.Keys.ElementAt(x), new AnimationFill(transitions[transitions.Keys.ElementAt(x)], transitionDuration).SetTransitionType(AnimationFrameTransitionType.None));
                                }

                                app.Profile.Layers.Add(new Layer
                                {
                                    Name = layerName,
                                    Enabled = layerEnabled,
                                    Handler = new AnimationLayerHandler
                                    {
                                        Properties = new AnimationLayerHandlerProperties
                                        {
                                            _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                            _Sequence = affected_keys,
                                            _forceKeySequence = true,
                                            _AnimationDuration = duration / 1000.0f,
                                            _AnimationRepeat = repeatTimes
                                        }
                                    }
                                });
                                break;
                            }
                            case "WaveLighting":
                            {
                                var duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);

                                List<AnimationTrack> animTracks = new List<AnimationTrack>();

                                var transitions = new ColorSpectrum();

                                var smallest = 0.5f;
                                var largest = 0.5f;

                                foreach (var transition in transitionInfo.Elements())
                                {
                                    try
                                    {
                                        var time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                        var color = ColorTranslator.FromHtml(transition.Element("color").Value);

                                        transitions.SetColorAt(time, color);

                                        if (time < smallest)
                                            smallest = time;
                                        else if (time > largest)
                                            largest = time;
                                    }
                                    catch (Exception exception)
                                    {
                                        Global.logger.Error(exception, "Wave Exception");
                                    }
                                }

                                if (smallest > 0.0f)
                                {
                                    transitions.SetColorAt(0.0f, Color.Transparent);
                                    transitions.SetColorAt(smallest - 0.001f, Color.Transparent);
                                }

                                if (largest < 1.0f)
                                {
                                    transitions.SetColorAt(1.0f, Color.Transparent);
                                    transitions.SetColorAt(largest + 0.001f, Color.Transparent);
                                }

                                //transitions.Flip();

                                var velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value, CultureInfo.InvariantCulture) / 10.0f;
                                var width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value, CultureInfo.InvariantCulture) / 10.0f;
                                var isDoubleSided = bool.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("isDoublesided").Value);
                                var angle = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("angle").Value, CultureInfo.InvariantCulture);

                                width *= 2.1f;

                                angle %= 360; //Get angle within our range
                                if (angle < 0) angle += 360;

                                var _widthFillTime = width / (velocity * 2.1f);
                                var _terminalTime = duration / 1000.0f;


                                var _terminalOffset = velocity * _terminalTime * 2.1f;
                                var centerX = Effects.CanvasWidthCenter;
                                var centerY = Effects.CanvasHeightCenter;
                                var widthX = width;
                                if (playOption.Equals("PlayFromKeyCenter"))
                                {
                                    //terminalTime = duration / 1000.0f;
                                    centerX = 0;
                                    centerY = 0;
                                    widthX = 0;
                                }
                                if (!isDoubleSided)
                                {
                                    var animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                    var terminalTime = _terminalTime;

                                    switch (angle)
                                    {
                                        case >= 315:
                                        case <= 45:
                                        {
                                            var _angleOffset = width / 2.0f * (float)Math.Cos(angle * (Math.PI / 180.0));
                                            _angleOffset = width / 2.0f - _angleOffset;


                                            animTrack.SetFrame(terminalTime*smallest, new AnimationFilledGradientRectangle(-width - _angleOffset, 0, width, Effects.CanvasHeight * 10, new EffectBrush(transitions)).SetAngle(angle));

                                            animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle( _angleOffset, 0, width, Effects.CanvasHeight * 10, new EffectBrush(transitions)).SetAngle(angle));
                                            break;
                                        }
                                        case > 45 and < 135:
                                            animTrack.SetFrame(terminalTime * smallest, new AnimationFilledGradientRectangle(0,  width / 2, width, Effects.CanvasWidth * 10, new EffectBrush(transitions)).SetAngle(angle));

                                            animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle(0, width / 2 - (Effects.CanvasWidth + width), width, Effects.CanvasWidth * 10, new EffectBrush(transitions)).SetAngle(angle));
                                            break;
                                        case >= 135 and <= 225:
                                            animTrack.SetFrame(terminalTime * smallest, new AnimationFilledGradientRectangle( width, 0, width, Effects.CanvasHeight * 10, new EffectBrush(transitions)).SetAngle(angle));

                                            animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle(-width, 0, width, Effects.CanvasHeight * 10, new EffectBrush(transitions)).SetAngle(angle));
                                            break;
                                        case > 225 and < 315:
                                            animTrack.SetFrame(terminalTime * smallest, new AnimationFilledGradientRectangle(0, -width / 2, width, Effects.CanvasWidth * 10, new EffectBrush(transitions)).SetAngle(angle));

                                            animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle(0, -width / 2 + (Effects.CanvasWidth + width), width, Effects.CanvasWidth * 10, new EffectBrush(transitions)).SetAngle(angle));
                                            break;
                                    }

                                    animTracks.Add(animTrack);
                                }
                                else
                                {
                                    var animTrack = new AnimationTrack(layerName + " - Side 1", duration / 1000.0f);
                                    var animTrack2 = new AnimationTrack(layerName + " - Side 2", duration / 1000.0f);

                                    var widthTime = width / (velocity * (3.0f * 0.7f)) / 2;
                                    _terminalTime = (Effects.CanvasWidth + width) / (velocity * 2.1f);

                                    switch (angle)
                                    {
                                        case >= 315:
                                        case <= 45:
                                        case >= 135 and <= 225:
                                        {
                                            //Right Side
                                            var _initialBrushRight = new EffectBrush(transitions);
                                            _initialBrushRight.start = new PointF(Effects.CanvasWidthCenter, 0);
                                            _initialBrushRight.end = new PointF(Effects.CanvasWidthCenter - width, 0);

                                            animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, 0, Effects.CanvasHeight * 3, _initialBrushRight).SetAngle(angle));

                                            if (_widthFillTime < _terminalTime)
                                            {
                                                var _fillBrushRight = new EffectBrush(_initialBrushRight);
                                                _fillBrushRight.start = new PointF(Effects.CanvasWidthCenter + width, 0);
                                                _fillBrushRight.end = new PointF(Effects.CanvasWidthCenter, 0);

                                                animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, width, Effects.CanvasHeight * 3, _fillBrushRight).SetAngle(angle));

                                                var _terminalBrushRight = new EffectBrush(_fillBrushRight);
                                                _terminalBrushRight.start = new PointF(Effects.CanvasWidthCenter + _terminalOffset, 0);
                                                _terminalBrushRight.end = new PointF(Effects.CanvasWidthCenter + _terminalOffset - width, 0);

                                                animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter + _terminalOffset - width, 0, width, Effects.CanvasHeight * 3, _terminalBrushRight).SetAngle(angle));

                                            }
                                            else
                                            {
                                                var _terminalBrushRight = new EffectBrush(_initialBrushRight);
                                                _terminalBrushRight.start = new PointF(Effects.CanvasWidthCenter + _terminalOffset, 0);
                                                _terminalBrushRight.end = new PointF(Effects.CanvasWidthCenter + _terminalOffset - width, 0);

                                                animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, _terminalOffset, Effects.CanvasHeight * 3, _terminalBrushRight).SetAngle(angle));
                                            }

                                            //Left Side
                                            var _initialBrushLeft = new EffectBrush(transitions);
                                            _initialBrushLeft.start = new PointF(Effects.CanvasWidthCenter, 0);
                                            _initialBrushLeft.end = new PointF(Effects.CanvasWidthCenter + width, 0);

                                            animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, 0, Effects.CanvasHeight * 3, _initialBrushLeft).SetAngle(angle));

                                            if (_widthFillTime < _terminalTime)
                                            {
                                                var _fillBrushLeft = new EffectBrush(_initialBrushLeft);
                                                _fillBrushLeft.start = new PointF(Effects.CanvasWidthCenter - width, 0);
                                                _fillBrushLeft.end = new PointF(Effects.CanvasWidthCenter, 0);

                                                animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter - width, 0, width, Effects.CanvasHeight * 3, _fillBrushLeft).SetAngle(angle));

                                                var _terminalBrushLeft = new EffectBrush(_initialBrushLeft);
                                                _terminalBrushLeft.start = new PointF(Effects.CanvasWidthCenter - _terminalOffset, 0);
                                                _terminalBrushLeft.end = new PointF(Effects.CanvasWidthCenter - _terminalOffset + width, 0);

                                                animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter - _terminalOffset, 0, width, Effects.CanvasHeight * 3, _terminalBrushLeft).SetAngle(angle));
                                            }
                                            else
                                            {
                                                var _terminalBrushLeft = new EffectBrush(_initialBrushLeft);
                                                _terminalBrushLeft.start = new PointF(Effects.CanvasWidthCenter - _terminalOffset, 0);
                                                _terminalBrushLeft.end = new PointF(Effects.CanvasWidthCenter - _terminalOffset + width, 0);

                                                animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter - _terminalOffset, 0, _terminalOffset, Effects.CanvasHeight * 3, _terminalBrushLeft).SetAngle(angle));
                                            }

                                            break;
                                        }
                                        case > 45 and < 135:
                                        case > 225 and < 315:
                                        {
                                            angle -= 90;

                                            //Bottom Side
                                            var _initialBrushBottom = new EffectBrush(transitions);
                                            _initialBrushBottom.start = new PointF(0, Effects.CanvasHeightCenter);
                                            _initialBrushBottom.end = new PointF(0, Effects.CanvasHeightCenter - width);

                                            animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.CanvasWidth * 3, 0, _initialBrushBottom).SetAngle(angle));

                                            if (_widthFillTime < _terminalTime)
                                            {
                                                var _fillBrushBottom = new EffectBrush(_initialBrushBottom);
                                                _fillBrushBottom.start = new PointF(0, Effects.CanvasHeightCenter + width);
                                                _fillBrushBottom.end = new PointF(0, Effects.CanvasHeightCenter);


                                                animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.CanvasWidth * 3, width, _fillBrushBottom).SetAngle(angle));

                                                var _terminalBrushBottom = new EffectBrush(_fillBrushBottom);
                                                _terminalBrushBottom.start = new PointF(0, Effects.CanvasHeightCenter + _terminalOffset);
                                                _terminalBrushBottom.end = new PointF(0, Effects.CanvasHeightCenter + _terminalOffset - width);

                                                animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter + _terminalOffset - width, Effects.CanvasWidth * 3, width, _terminalBrushBottom).SetAngle(angle));
                                            }
                                            else
                                            {
                                                var _terminalBrushBottom = new EffectBrush(_initialBrushBottom);
                                                _terminalBrushBottom.start = new PointF(0, Effects.CanvasHeightCenter + _terminalOffset);
                                                _terminalBrushBottom.end = new PointF(0, Effects.CanvasHeightCenter + _terminalOffset - width);

                                                animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.CanvasWidth * 3, _terminalOffset, _terminalBrushBottom).SetAngle(angle));
                                            }

                                            //Top Side
                                            var _initialBrushtTop = new EffectBrush(transitions);
                                            _initialBrushtTop.start = new PointF(0, Effects.CanvasHeightCenter);
                                            _initialBrushtTop.end = new PointF(0, Effects.CanvasHeightCenter + width);

                                            animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.CanvasWidth * 3, 0, _initialBrushtTop).SetAngle(angle));

                                            if (_widthFillTime < _terminalTime)
                                            {
                                                var _fillBrushTop = new EffectBrush(_initialBrushtTop);
                                                _fillBrushTop.start = new PointF(0, Effects.CanvasHeightCenter - width);
                                                _fillBrushTop.end = new PointF(0, Effects.CanvasHeightCenter);

                                                animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter - width, Effects.CanvasWidth * 3, width, _fillBrushTop).SetAngle(angle));

                                                var _terminalBrushTop = new EffectBrush(_initialBrushtTop);
                                                _terminalBrushTop.start = new PointF(0, Effects.CanvasHeightCenter - _terminalOffset);
                                                _terminalBrushTop.end = new PointF(0, Effects.CanvasHeightCenter - _terminalOffset + width);
                                                animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter - _terminalOffset, Effects.CanvasWidth * 3, width, _terminalBrushTop).SetAngle(angle));
                                            }
                                            else
                                            {
                                                var _terminalBrushTop = new EffectBrush(_initialBrushtTop);
                                                _terminalBrushTop.start = new PointF(0, Effects.CanvasHeightCenter - _terminalOffset);
                                                _terminalBrushTop.end = new PointF(0, Effects.CanvasHeightCenter - _terminalOffset + width);

                                                animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter - _terminalOffset, Effects.CanvasWidth * 3, _terminalOffset, _terminalBrushTop).SetAngle(angle));
                                            }

                                            break;
                                        }
                                    }

                                    animTracks.Add(animTrack);
                                    animTracks.Add(animTrack2);
                                }

                                app.Profile.Layers.Add(new Layer
                                {
                                    Name = layerName,
                                    Enabled = layerEnabled,
                                    Handler = new AnimationLayerHandler
                                    {
                                        Properties = new AnimationLayerHandlerProperties
                                        {
                                            _AnimationMix = new AnimationMix(animTracks.ToArray()),
                                            _Sequence = affected_keys,
                                            _forceKeySequence = true,
                                            _AnimationDuration = duration / 1000.0f,
                                            _AnimationRepeat = repeatTimes,
                                            _scaleToKeySequenceBounds = true,
                                            _TriggerMode = RippleEffectCheck ? triggerMode : AnimationTriggerMode.AlwaysOn,
                                            //  _TriggerAnyKey = RippleEffectCheck,
                                            _TriggerKeySequence = affected_keys,
                                            _StackMode = RippleEffectCheck ? AnimationStackMode.Reset : AnimationStackMode.Ignore,
                                            _WhileKeyHeldTerminateRunning = stopImmediately,
                                            _KeyTriggerTranslate = playFromKey
                                        }
                                    }
                                });
                                break;
                            }
                            case "RippleLighting":
                            {
                                var duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);

                                var transitions = new ColorSpectrum();

                                var smallest = 0.5f;
                                var largest = 0.5f;

                                foreach (var transition in transitionInfo.Elements())
                                {
                                    try
                                    {
                                        var time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                        var color = ColorTranslator.FromHtml(transition.Element("color").Value);

                                        transitions.SetColorAt(time, color);

                                        if (time < smallest)
                                            smallest = time;
                                        else if (time > largest)
                                            largest = time;
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                if (smallest > 0.0f)
                                {
                                    transitions.SetColorAt(0.0f, Color.Transparent);
                                    transitions.SetColorAt(smallest - 0.001f, Color.Transparent);
                                }

                                if (largest < 1.0f)
                                {
                                    transitions.SetColorAt(1.0f, Color.Transparent);
                                    transitions.SetColorAt(largest + 0.001f, Color.Transparent);
                                }

                                transitions.Flip();

                                var velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value, CultureInfo.InvariantCulture) / 10.0f;
                                var width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value, CultureInfo.InvariantCulture) / 10.0f;

                                width *= 3.0f;

                                var animTrack = new AnimationTrack(layerName, duration / 1000.0f);
                                var terminalTime = Effects.CanvasWidth / (velocity * (3.0f * 0.7f)) / 2;
                                var centerX = Effects.CanvasWidthCenter;
                                var centerY = Effects.CanvasHeightCenter;
                                if (playOption.Equals("PlayFromKeyCenter"))
                                {
                                    //terminalTime = duration / 1000.0f;
                                    centerX = 0;
                                    centerY = 0;
                                }
                                animTrack.SetFrame(0.0f, new AnimationGradientCircle(centerX, centerY, 0, new EffectBrush(transitions).SetBrushType(EffectBrush.BrushType.Radial), (int)width));

                                animTrack.SetFrame(terminalTime, new AnimationGradientCircle(centerX, centerY, Effects.CanvasBiggest, new EffectBrush(transitions).SetBrushType(EffectBrush.BrushType.Radial), (int)width));
                                app.Profile.Layers.Add(new Layer
                                {
                                    Name = layerName,
                                    Enabled = layerEnabled,
                                    Handler = new AnimationLayerHandler
                                    {
                                        Properties = new AnimationLayerHandlerProperties
                                        {
                                            _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                            _Sequence = affected_keys,
                                            _forceKeySequence = true,
                                            _AnimationDuration = duration / 1000.0f,
                                            _AnimationRepeat = repeatTimes,
                                            _scaleToKeySequenceBounds = true,
                                            _TriggerMode = RippleEffectCheck ? triggerMode : AnimationTriggerMode.AlwaysOn,
                                            //  _TriggerAnyKey = RippleEffectCheck,
                                            _TriggerKeySequence = affected_keys,
                                            _StackMode = RippleEffectCheck ? AnimationStackMode.Reset : AnimationStackMode.Ignore,
                                            _WhileKeyHeldTerminateRunning = stopImmediately,
                                            _KeyTriggerTranslate = playFromKey
                                        }
                                    }
                                });
                                break;
                            }
                            default:
                                //Null, it's unknown.
                                Global.logger.Warning("Unknown CUE Layer Type");
                                break;
                        }
                    }

                    break;
                }
                break;
            }
        }
    }

    /// <summary>
    /// Imports a json profile into the given application.
    /// </summary>
    private static void ImportJson(this Application app, string filepath)
    {
        try
        {
            // Attempt to read and deserialise the profile
            var json = File.ReadAllText(filepath, Encoding.UTF8);
            var inProf = (ApplicationProfile)JsonConvert.DeserializeObject(json, typeof(ApplicationProfile), new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = JSONUtils.SerializationBinder,
            });

            // Create a new profile on the current application (so that profiles can be imported from different applications)
            var newProf = app.AddNewProfile(inProf.ProfileName);
            newProf.TriggerKeybind = inProf.TriggerKeybind.Clone();

            // Copy any valid layers from the read profile to the new one
            void ImportLayers(ObservableCollection<Layer> source, ObservableCollection<Layer> target) {
                target.Clear();
                for (var i = 0; i < source.Count; i++)
                    if (app.IsAllowedLayer(source[i].Handler.GetType()))
                        target.Add((Layer)source[i].Clone());
            }
            ImportLayers(inProf.Layers, newProf.Layers);
            ImportLayers(inProf.OverlayLayers, newProf.OverlayLayers);

            // Force a save to write the new profile to disk in the appdata dir
            app.SaveProfiles();

        }
        catch (Exception ex)
        {
            Global.logger.Error(ex, "Profile import error:");
            MessageBox.Show("Error importing the profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Converts CorsairLedId to DeviceKeys
    /// </summary>
    /// <param name="corsairKey">The CorsairLedId to be converted</param>
    /// <returns>The resulting DeviceKeys</returns>
    private static DeviceKeys ToDeviceKeys(CorsairLedId corsairKey)
    {
        switch (corsairKey)
        {
            case CorsairLedId.Logo:
                return DeviceKeys.LOGO;
            case CorsairLedId.Brightness:
                return DeviceKeys.BRIGHTNESS_SWITCH;
            case CorsairLedId.WinLock:
                return DeviceKeys.LOCK_SWITCH;

            case CorsairLedId.Mute:
                return DeviceKeys.VOLUME_MUTE;
            case CorsairLedId.VolumeUp:
                return DeviceKeys.VOLUME_UP;
            case CorsairLedId.VolumeDown:
                return DeviceKeys.VOLUME_DOWN;
            case CorsairLedId.Stop:
                return DeviceKeys.MEDIA_STOP;
            case CorsairLedId.PlayPause:
                return DeviceKeys.MEDIA_PLAY_PAUSE;
            case CorsairLedId.ScanPreviousTrack:
                return DeviceKeys.MEDIA_PREVIOUS;
            case CorsairLedId.ScanNextTrack:
                return DeviceKeys.MEDIA_NEXT;

            case CorsairLedId.Escape:
                return DeviceKeys.ESC;
            case CorsairLedId.F1:
                return DeviceKeys.F1;
            case CorsairLedId.F2:
                return DeviceKeys.F2;
            case CorsairLedId.F3:
                return DeviceKeys.F3;
            case CorsairLedId.F4:
                return DeviceKeys.F4;
            case CorsairLedId.F5:
                return DeviceKeys.F5;
            case CorsairLedId.F6:
                return DeviceKeys.F6;
            case CorsairLedId.F7:
                return DeviceKeys.F7;
            case CorsairLedId.F8:
                return DeviceKeys.F8;
            case CorsairLedId.F9:
                return DeviceKeys.F9;
            case CorsairLedId.F10:
                return DeviceKeys.F10;
            case CorsairLedId.F11:
                return DeviceKeys.F11;
            case CorsairLedId.F12:
                return DeviceKeys.F12;
            case CorsairLedId.PrintScreen:
                return DeviceKeys.PRINT_SCREEN;
            case CorsairLedId.ScrollLock:
                return DeviceKeys.SCROLL_LOCK;
            case CorsairLedId.PauseBreak:
                return DeviceKeys.PAUSE_BREAK;
            case CorsairLedId.GraveAccentAndTilde:
                return DeviceKeys.TILDE;
            case CorsairLedId.D1:
                return DeviceKeys.ONE;
            case CorsairLedId.D2:
                return DeviceKeys.TWO;
            case CorsairLedId.D3:
                return DeviceKeys.THREE;
            case CorsairLedId.D4:
                return DeviceKeys.FOUR;
            case CorsairLedId.D5:
                return DeviceKeys.FIVE;
            case CorsairLedId.D6:
                return DeviceKeys.SIX;
            case CorsairLedId.D7:
                return DeviceKeys.SEVEN;
            case CorsairLedId.D8:
                return DeviceKeys.EIGHT;
            case CorsairLedId.D9:
                return DeviceKeys.NINE;
            case CorsairLedId.D0:
                return DeviceKeys.ZERO;
            case CorsairLedId.MinusAndUnderscore:
                return DeviceKeys.MINUS;
            case CorsairLedId.EqualsAndPlus:
                return DeviceKeys.EQUALS;
            case CorsairLedId.Backspace:
                return DeviceKeys.BACKSPACE;
            case CorsairLedId.Insert:
                return DeviceKeys.INSERT;
            case CorsairLedId.Home:
                return DeviceKeys.HOME;
            case CorsairLedId.PageUp:
                return DeviceKeys.PAGE_UP;
            case CorsairLedId.NumLock:
                return DeviceKeys.NUM_LOCK;
            case CorsairLedId.KeypadSlash:
                return DeviceKeys.NUM_SLASH;
            case CorsairLedId.KeypadAsterisk:
                return DeviceKeys.NUM_ASTERISK;
            case CorsairLedId.KeypadMinus:
                return DeviceKeys.NUM_MINUS;
            case CorsairLedId.Tab:
                return DeviceKeys.TAB;
            case CorsairLedId.Q:
                return DeviceKeys.Q;
            case CorsairLedId.W:
                return DeviceKeys.W;
            case CorsairLedId.E:
                return DeviceKeys.E;
            case CorsairLedId.R:
                return DeviceKeys.R;
            case CorsairLedId.T:
                return DeviceKeys.T;
            case CorsairLedId.Y:
                return DeviceKeys.Y;
            case CorsairLedId.U:
                return DeviceKeys.U;
            case CorsairLedId.I:
                return DeviceKeys.I;
            case CorsairLedId.O:
                return DeviceKeys.O;
            case CorsairLedId.P:
                return DeviceKeys.P;
            case CorsairLedId.BracketLeft:
                return DeviceKeys.OPEN_BRACKET;
            case CorsairLedId.BracketRight:
                return DeviceKeys.CLOSE_BRACKET;
            case CorsairLedId.Backslash:
                return DeviceKeys.BACKSLASH;
            case CorsairLedId.Delete:
                return DeviceKeys.DELETE;
            case CorsairLedId.End:
                return DeviceKeys.END;
            case CorsairLedId.PageDown:
                return DeviceKeys.PAGE_DOWN;
            case CorsairLedId.Keypad7:
                return DeviceKeys.NUM_SEVEN;
            case CorsairLedId.Keypad8:
                return DeviceKeys.NUM_EIGHT;
            case CorsairLedId.Keypad9:
                return DeviceKeys.NUM_NINE;
            case CorsairLedId.KeypadPlus:
                return DeviceKeys.NUM_PLUS;
            case CorsairLedId.CapsLock:
                return DeviceKeys.CAPS_LOCK;
            case CorsairLedId.A:
                return DeviceKeys.A;
            case CorsairLedId.S:
                return DeviceKeys.S;
            case CorsairLedId.D:
                return DeviceKeys.D;
            case CorsairLedId.F:
                return DeviceKeys.F;
            case CorsairLedId.G:
                return DeviceKeys.G;
            case CorsairLedId.H:
                return DeviceKeys.H;
            case CorsairLedId.J:
                return DeviceKeys.J;
            case CorsairLedId.K:
                return DeviceKeys.K;
            case CorsairLedId.L:
                return DeviceKeys.L;
            case CorsairLedId.SemicolonAndColon:
                return DeviceKeys.SEMICOLON;
            case CorsairLedId.ApostropheAndDoubleQuote:
                return DeviceKeys.APOSTROPHE;
            case CorsairLedId.NonUsTilde:
                return DeviceKeys.HASHTAG;
            case CorsairLedId.Enter:
                return DeviceKeys.ENTER;
            case CorsairLedId.Keypad4:
                return DeviceKeys.NUM_FOUR;
            case CorsairLedId.Keypad5:
                return DeviceKeys.NUM_FIVE;
            case CorsairLedId.Keypad6:
                return DeviceKeys.NUM_SIX;
            case CorsairLedId.LeftShift:
                return DeviceKeys.LEFT_SHIFT;
            case CorsairLedId.NonUsBackslash:
                return DeviceKeys.BACKSLASH_UK;
            case CorsairLedId.Z:
                return DeviceKeys.Z;
            case CorsairLedId.X:
                return DeviceKeys.X;
            case CorsairLedId.C:
                return DeviceKeys.C;
            case CorsairLedId.V:
                return DeviceKeys.V;
            case CorsairLedId.B:
                return DeviceKeys.B;
            case CorsairLedId.N:
                return DeviceKeys.N;
            case CorsairLedId.M:
                return DeviceKeys.M;
            case CorsairLedId.CommaAndLessThan:
                return DeviceKeys.COMMA;
            case CorsairLedId.PeriodAndBiggerThan:
                return DeviceKeys.PERIOD;
            case CorsairLedId.SlashAndQuestionMark:
                return DeviceKeys.FORWARD_SLASH;
            case CorsairLedId.RightShift:
                return DeviceKeys.RIGHT_SHIFT;
            case CorsairLedId.UpArrow:
                return DeviceKeys.ARROW_UP;
            case CorsairLedId.Keypad1:
                return DeviceKeys.NUM_ONE;
            case CorsairLedId.Keypad2:
                return DeviceKeys.NUM_TWO;
            case CorsairLedId.Keypad3:
                return DeviceKeys.NUM_THREE;
            case CorsairLedId.KeypadEnter:
                return DeviceKeys.NUM_ENTER;
            case CorsairLedId.LeftCtrl:
                return DeviceKeys.LEFT_CONTROL;
            case CorsairLedId.LeftGui:
                return DeviceKeys.LEFT_WINDOWS;
            case CorsairLedId.LeftAlt:
                return DeviceKeys.LEFT_ALT;
            case CorsairLedId.Space:
                return DeviceKeys.SPACE;
            case CorsairLedId.RightAlt:
                return DeviceKeys.RIGHT_ALT;
            case CorsairLedId.RightGui:
                return DeviceKeys.RIGHT_WINDOWS;
            case CorsairLedId.Application:
                return DeviceKeys.APPLICATION_SELECT;
            case CorsairLedId.RightCtrl:
                return DeviceKeys.RIGHT_CONTROL;
            case CorsairLedId.LeftArrow:
                return DeviceKeys.ARROW_LEFT;
            case CorsairLedId.DownArrow:
                return DeviceKeys.ARROW_DOWN;
            case CorsairLedId.RightArrow:
                return DeviceKeys.ARROW_RIGHT;
            case CorsairLedId.Keypad0:
                return DeviceKeys.NUM_ZERO;
            case CorsairLedId.KeypadPeriodAndDelete:
                return DeviceKeys.NUM_PERIOD;

            case CorsairLedId.Fn:
                return DeviceKeys.FN_Key;

            case CorsairLedId.G1:
                return DeviceKeys.G1;
            case CorsairLedId.G2:
                return DeviceKeys.G2;
            case CorsairLedId.G3:
                return DeviceKeys.G3;
            case CorsairLedId.G4:
                return DeviceKeys.G4;
            case CorsairLedId.G5:
                return DeviceKeys.G5;
            case CorsairLedId.G6:
                return DeviceKeys.G6;
            case CorsairLedId.G7:
                return DeviceKeys.G7;
            case CorsairLedId.G8:
                return DeviceKeys.G8;
            case CorsairLedId.G9:
                return DeviceKeys.G9;
            case CorsairLedId.G10:
                return DeviceKeys.G10;
            case CorsairLedId.G11:
                return DeviceKeys.G11;
            case CorsairLedId.G12:
                return DeviceKeys.G12;
            case CorsairLedId.G13:
                return DeviceKeys.G13;
            case CorsairLedId.G14:
                return DeviceKeys.G14;
            case CorsairLedId.G15:
                return DeviceKeys.G15;
            case CorsairLedId.G16:
                return DeviceKeys.G16;
            case CorsairLedId.G17:
                return DeviceKeys.G17;
            case CorsairLedId.G18:
                return DeviceKeys.G18;

            default:
                return DeviceKeys.NONE;
        }
    }
}