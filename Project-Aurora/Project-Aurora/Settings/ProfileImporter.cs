using Aurora.Devices;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Settings.Layers;
using CorsairRGB.NET.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aurora.Settings
{
    public static class ProfileImporter
    {

        /// <summary>
        /// Imports a file from disk as a profile into the given application.
        /// </summary>
        /// <param name="filepath">The full filepath of the file to import.</param>
        public static void ImportProfile(this Application app, string filepath)
        {
            string fn = filepath.ToLower();

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
            XElement rootElement = XElement.Load(filepath);
            XElement valueElement = filepath.EndsWith(".cueprofile") ? rootElement : rootElement.Element("profile_folder").Element("profiles");

            foreach (XElement value in valueElement.Elements())
            {
                XElement profileElement = value.Element("profile");
                if (profileElement != null)
                {
                    string profileName = profileElement.Element("name").Value;
                    app?.AddNewProfile(profileName);

                    foreach (XElement property in profileElement.Element("properties").Elements())
                    {
                        if ("Keyboard".Equals(property.Element("key").Value))
                        {
                            foreach (XElement profProperty in property.Element("value").Element("properties").Descendants())
                            {
                                if (profProperty.Name.ToString().Equals("keys"))
                                {
                                    var hasValue = profProperty.Element("value0");
                                    if (hasValue != null)
                                    {
                                        var layers = profProperty.Parent.Parent.Parent.Parent.Parent;
                                        app.Profile.Layers.Clear();

                                        uint _basePolyID = 2147483648;
                                        Dictionary<uint, string> _definedPolyIDS = new Dictionary<uint, string>();

                                        foreach (XElement layer in layers.Elements())
                                        {
                                            var keysAndStuff = layer.Element("ptr_wrapper").Element("data").Element("base");

                                            string layerName = keysAndStuff.Element("name").Value;
                                            bool layerEnabled = bool.Parse(keysAndStuff.Element("enabled").Value);
                                            int repeatTimes = Math.Max(int.Parse(keysAndStuff.Element("executionHints").Element("stopAfterTimes").Value), 0);
                                            bool stopOnRelease = bool.Parse(keysAndStuff.Element("executionHints").Element("stopOnKeyRelease").Value);
                                            bool stopOnPress = bool.Parse(keysAndStuff.Element("executionHints").Element("stopOnKeyPress").Value);
                                            var startOnKeyPress = keysAndStuff.Element("executionHints").Element("startOnKeyPress").Value;
                                            var playOption = keysAndStuff.Element("executionHints").Element("playOption").Value;
                                            bool RippleEffectCheck = false;
                                            bool playFromKey = false;
                                            bool stopImmediately = false;
                                            AnimationTriggerMode triggerMode = AnimationTriggerMode.OnKeyPress;
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
                                            KeySequence affected_keys = new KeySequence();
                                            List<Keybind> bindings = new List<Keybind>();
                                            foreach (XElement key in keysAndStuff.Element("keys").Elements())
                                            {
                                                try
                                                {
                                                    CorsairRGB.NET.Enums.CorsairLedId? keyValue = null;

                                                    switch (key.Value)
                                                    {
                                                        case "0":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_0;
                                                            break;
                                                        case "1":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_1;
                                                            break;
                                                        case "2":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_2;
                                                            break;
                                                        case "3":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_3;
                                                            break;
                                                        case "4":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_4;
                                                            break;
                                                        case "5":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_5;
                                                            break;
                                                        case "6":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_6;
                                                            break;
                                                        case "7":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_7;
                                                            break;
                                                        case "8":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_8;
                                                            break;
                                                        case "9":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_9;
                                                            break;
                                                        case "Led_KeyboardLogo":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_Logo;
                                                            break;
                                                        case "Led_KeyboardTopLogo2":
                                                            keyValue = CorsairRGB.NET.Enums.CorsairLedId.K_Logo;
                                                            break;
                                                        default:
                                                            if (key.Value.StartsWith("Led_Top"))
                                                                key.Value = "G18";
                                                            if(Enum.TryParse<CorsairLedId>("K_" + key.Value, out var corsairLedId)){
                                                                keyValue = corsairLedId;
                                                            }
                                                            else
                                                            {
                                                                Global.logger.Warn($"CorsairLedId not mapped, skipping {key.Value}");
                                                            }
                                                            break;
                                                    }

                                                    if (keyValue.HasValue && Enum.IsDefined(typeof(CorsairLedId), keyValue) | keyValue.ToString().Contains(","))
                                                    {
                                                        Devices.DeviceKeys deviceKey = ToDeviceKeys(keyValue.Value);

                                                        if (deviceKey != Devices.DeviceKeys.NONE)
                                                        {
                                                            affected_keys.keys.Add(deviceKey);
                                                            bindings.Add(new Keybind(new System.Windows.Forms.Keys[] { Utils.KeyUtils.GetFormsKey(deviceKey) }));
                                                        }
                                                    }
                                                }
                                                catch (Exception exception)
                                                {
                                                    Global.logger.Error(exception, "Exception in profile: " + exception.StackTrace);
                                                    //break;
                                                }
                                            }

                                            var lightingInfo = layer.Element("ptr_wrapper").Element("data").Element("lighting");
                                            if (lightingInfo == null)
                                            {
                                                break;
                                            }

                                            var transitionInfo = lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions");
                                            if (transitionInfo == null)
                                            {
                                                transitionInfo = lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("transitions");
                                            }
                                            var layerPolyId = lightingInfo.Element("polymorphic_id");
                                            var layerPolyName = lightingInfo.Element("polymorphic_name")?.Value;

                                            if (String.IsNullOrWhiteSpace(layerPolyName))
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
                                                    if (gradientCheck != null)
                                                    {
                                                        layerPolyName = "GradientLighting";
                                                    }
                                                    else
                                                        layerPolyName = "StaticLighting";
                                                }
                                            }
                                            else
                                                _definedPolyIDS.Add(uint.Parse(layerPolyId.Value) - _basePolyID, layerPolyName);

                                            if ("StaticLighting".Equals(layerPolyName))
                                            {
                                                app.Profile.Layers.Add(new Layers.Layer()
                                                {
                                                    Name = layerName,
                                                    Enabled = layerEnabled,
                                                    Handler = new Layers.SolidColorLayerHandler()
                                                    {
                                                        Properties = new Layers.LayerHandlerProperties()
                                                        {
                                                            _Sequence = affected_keys,
                                                            _PrimaryColor = System.Drawing.ColorTranslator.FromHtml(transitionInfo.Element("value0").Element("color").Value),
                                                            _LayerOpacity = int.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("opacity").Value) / 255.0f
                                                        },
                                                    }
                                                });
                                            }
                                            else if ("GradientLighting".Equals(layerPolyName))
                                            {
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);
                                                AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                                Dictionary<float, System.Drawing.Color> transitions = new Dictionary<float, System.Drawing.Color>();

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                        if (transitions.ContainsKey(time * (duration / 1000.0f)))
                                                            transitions.Add(time * (duration / 1000.0f) + 0.000001f, color);
                                                        else
                                                            transitions.Add(time * (duration / 1000.0f), color);
                                                    }
                                                    catch (Exception)
                                                    {

                                                    }
                                                }

                                                for (int x = 0; x < transitions.Count; x += 2)
                                                {
                                                    float transitionDuration = 0.0f;

                                                    if (x + 1 != transitions.Count)
                                                        transitionDuration = transitions.Keys.ElementAt(x + 1) - transitions.Keys.ElementAt(x);

                                                    animTrack.SetFrame(transitions.Keys.ElementAt(x), new AnimationFill(transitions[transitions.Keys.ElementAt(x)], transitionDuration));
                                                }

                                                app.Profile.Layers.Add(new Layers.Layer()
                                                {
                                                    Name = layerName,
                                                    Enabled = layerEnabled,
                                                    Handler = new Layers.AnimationLayerHandler()
                                                    {
                                                        Properties = new Layers.AnimationLayerHandlerProperties()
                                                        {
                                                            _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                                            _Sequence = affected_keys,
                                                            _forceKeySequence = true,
                                                            _scaleToKeySequenceBounds = true,
                                                            _AnimationDuration = (duration / 1000.0f),
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
                                            }
                                            else if ("SolidLighting".Equals(layerPolyName))
                                            {
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);
                                                AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                                Dictionary<float, System.Drawing.Color> transitions = new Dictionary<float, System.Drawing.Color>();

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                        if (transitions.ContainsKey(time * (duration / 1000.0f)))
                                                            transitions.Add(time * (duration / 1000.0f) + 0.000001f, color);
                                                        else
                                                            transitions.Add(time * (duration / 1000.0f), color);
                                                    }
                                                    catch (Exception)
                                                    {

                                                    }
                                                }

                                                for (int x = 0; x < transitions.Count; x += 2)
                                                {
                                                    float transitionDuration = transitions.Keys.ElementAt(x + 1) - transitions.Keys.ElementAt(x);

                                                    animTrack.SetFrame(transitions.Keys.ElementAt(x), new AnimationFill(transitions[transitions.Keys.ElementAt(x)], transitionDuration).SetTransitionType(AnimationFrameTransitionType.None));
                                                }

                                                app.Profile.Layers.Add(new Layers.Layer()
                                                {
                                                    Name = layerName,
                                                    Enabled = layerEnabled,
                                                    Handler = new Layers.AnimationLayerHandler()
                                                    {
                                                        Properties = new Layers.AnimationLayerHandlerProperties()
                                                        {
                                                            _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                                            _Sequence = affected_keys,
                                                            _forceKeySequence = true,
                                                            _AnimationDuration = (duration / 1000.0f),
                                                            _AnimationRepeat = repeatTimes
                                                        }
                                                    }
                                                });
                                            }
                                            else if ("WaveLighting".Equals(layerPolyName))
                                            {
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);

                                                List<AnimationTrack> animTracks = new List<AnimationTrack>();

                                                EffectsEngine.ColorSpectrum transitions = new EffectsEngine.ColorSpectrum();

                                                float smallest = 0.5f;
                                                float largest = 0.5f;

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                        transitions.SetColorAt(time, color);

                                                        if (time < smallest)
                                                            smallest = time;
                                                        else if (time > largest)
                                                            largest = time;
                                                    }
                                                    catch (Exception exception)
                                                    {
                                                        Global.logger.Error(exception, "Wave Ex " + exception.StackTrace);
                                                    }
                                                }

                                                if (smallest > 0.0f)
                                                {
                                                    transitions.SetColorAt(0.0f, System.Drawing.Color.Transparent);
                                                    transitions.SetColorAt(smallest - 0.001f, System.Drawing.Color.Transparent);
                                                }

                                                if (largest < 1.0f)
                                                {
                                                    transitions.SetColorAt(1.0f, System.Drawing.Color.Transparent);
                                                    transitions.SetColorAt(largest + 0.001f, System.Drawing.Color.Transparent);
                                                }

                                                //transitions.Flip();

                                                float velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value, CultureInfo.InvariantCulture) / 10.0f;
                                                float width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value, CultureInfo.InvariantCulture) / 10.0f;
                                                bool isDoubleSided = bool.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("isDoublesided").Value);
                                                float angle = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("angle").Value, CultureInfo.InvariantCulture);

                                                width *= 2.1f;

                                                angle %= 360; //Get angle within our range
                                                if (angle < 0) angle += 360;

                                                float _widthFillTime = width / (velocity * 2.1f);
                                                float _terminalTime = duration / 1000.0f;


                                                float _terminalOffset = velocity * _terminalTime * 2.1f;
                                                float centerX = Effects.canvas_width_center;
                                                float centerY = Effects.canvas_height_center;
                                                float widthX = width;
                                                if (playOption.Equals("PlayFromKeyCenter"))
                                                {
                                                    //terminalTime = duration / 1000.0f;
                                                    centerX = 0;
                                                    centerY = 0;
                                                    widthX = 0;
                                                }
                                                if (!isDoubleSided)
                                                {
                                                    AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                                    float terminalTime = _terminalTime;

                                                    if (angle >= 315 || angle <= 45)
                                                    {
                                                        float _angleOffset = (width / 2.0f) * (float)Math.Cos((double)angle * (Math.PI / 180.0));
                                                        _angleOffset = (width / 2.0f) - _angleOffset;


                                                        animTrack.SetFrame(terminalTime*smallest, new AnimationFilledGradientRectangle(-width - _angleOffset, 0, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle( _angleOffset, 0, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                    }
                                                    else if (angle > 45 && angle < 135)
                                                    {
                                                        animTrack.SetFrame(terminalTime * smallest, new AnimationFilledGradientRectangle(0,  width / 2, width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle(0, (width / 2) - (Effects.canvas_width + width), width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                    }
                                                    else if (angle >= 135 && angle <= 225)
                                                    {
                                                        animTrack.SetFrame(terminalTime * smallest, new AnimationFilledGradientRectangle( width, 0, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle(-width, 0, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                    }
                                                    else if (angle > 225 && angle < 315)
                                                    {
                                                        animTrack.SetFrame(terminalTime * smallest, new AnimationFilledGradientRectangle(0, -width / 2, width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime*largest, new AnimationFilledGradientRectangle(0, (-width / 2) + (Effects.canvas_width + width), width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                    }

                                                    animTracks.Add(animTrack);
                                                }
                                                else
                                                {
                                                    AnimationTrack animTrack = new AnimationTrack(layerName + " - Side 1", duration / 1000.0f);
                                                    AnimationTrack animTrack2 = new AnimationTrack(layerName + " - Side 2", duration / 1000.0f);

                                                    float widthTime = width / (velocity * (3.0f * 0.7f)) / 2;
                                                    _terminalTime = (Effects.canvas_width + width) / (velocity * 2.1f);

                                                    if ((angle >= 315 || angle <= 45) || (angle >= 135 && angle <= 225))
                                                    {
                                                        //Right Side
                                                        EffectsEngine.EffectBrush _initialBrushRight = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center, 0);
                                                        _initialBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center - width, 0);

                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, 0, Effects.canvas_height * 3, _initialBrushRight).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                            _fillBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + width, 0);
                                                            _fillBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center, 0);

                                                            animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, width, Effects.canvas_height * 3, _fillBrushRight).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_fillBrushRight);
                                                            _terminalBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset, 0);
                                                            _terminalBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset - width, 0);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center + _terminalOffset - width, 0, width, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));

                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                            _terminalBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset, 0);
                                                            _terminalBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset - width, 0);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, _terminalOffset, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));
                                                        }

                                                        //Left Side
                                                        EffectsEngine.EffectBrush _initialBrushLeft = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center, 0);
                                                        _initialBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center + width, 0);

                                                        animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, 0, Effects.canvas_height * 3, _initialBrushLeft).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _fillBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - width, 0);
                                                            _fillBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center, 0);

                                                            animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - width, 0, width, Effects.canvas_height * 3, _fillBrushLeft).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _terminalBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset, 0);
                                                            _terminalBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset + width, 0);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - _terminalOffset, 0, width, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _terminalBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset, 0);
                                                            _terminalBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset + width, 0);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - _terminalOffset, 0, _terminalOffset, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                        }
                                                    }
                                                    else if ((angle > 45 && angle < 135) || (angle > 225 && angle < 315))
                                                    {
                                                        angle -= 90;

                                                        //Bottom Side
                                                        EffectsEngine.EffectBrush _initialBrushBottom = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center);
                                                        _initialBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center - width);

                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center, Effects.canvas_width * 3, 0, _initialBrushBottom).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                            _fillBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + width);
                                                            _fillBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center);


                                                            animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center, Effects.canvas_width * 3, width, _fillBrushBottom).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_fillBrushBottom);
                                                            _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset);
                                                            _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset - width);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center + _terminalOffset - width, Effects.canvas_width * 3, width, _terminalBrushBottom).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                            _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset);
                                                            _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset - width);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center, Effects.canvas_width * 3, _terminalOffset, _terminalBrushBottom).SetAngle(angle));
                                                        }

                                                        //Top Side
                                                        EffectsEngine.EffectBrush _initialBrushtTop = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushtTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center);
                                                        _initialBrushtTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center + width);

                                                        animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center, Effects.canvas_width * 3, 0, _initialBrushtTop).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _fillBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - width);
                                                            _fillBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center);

                                                            animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center - width, Effects.canvas_width * 3, width, _fillBrushTop).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset);
                                                            _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset + width);
                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center - _terminalOffset, Effects.canvas_width * 3, width, _terminalBrushTop).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset);
                                                            _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset + width);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.canvas_height_center - _terminalOffset, Effects.canvas_width * 3, _terminalOffset, _terminalBrushTop).SetAngle(angle));
                                                        }
                                                    }

                                                    animTracks.Add(animTrack);
                                                    animTracks.Add(animTrack2);
                                                }

                                                app.Profile.Layers.Add(new Layers.Layer()
                                                {
                                                    Name = layerName,
                                                    Enabled = layerEnabled,
                                                    Handler = new Layers.AnimationLayerHandler()
                                                    {
                                                        Properties = new Layers.AnimationLayerHandlerProperties()
                                                        {
                                                            _AnimationMix = new AnimationMix(animTracks.ToArray()),
                                                            _Sequence = affected_keys,
                                                            _forceKeySequence = true,
                                                            _AnimationDuration = (duration / 1000.0f),
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
                                            }
                                            else if ("RippleLighting".Equals(layerPolyName))
                                            {
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value, CultureInfo.InvariantCulture);

                                                EffectsEngine.ColorSpectrum transitions = new EffectsEngine.ColorSpectrum();

                                                float smallest = 0.5f;
                                                float largest = 0.5f;

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value, CultureInfo.InvariantCulture);
                                                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

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
                                                    transitions.SetColorAt(0.0f, System.Drawing.Color.Transparent);
                                                    transitions.SetColorAt(smallest - 0.001f, System.Drawing.Color.Transparent);
                                                }

                                                if (largest < 1.0f)
                                                {
                                                    transitions.SetColorAt(1.0f, System.Drawing.Color.Transparent);
                                                    transitions.SetColorAt(largest + 0.001f, System.Drawing.Color.Transparent);
                                                }

                                                transitions.Flip();

                                                float velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value, CultureInfo.InvariantCulture) / 10.0f;
                                                float width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value, CultureInfo.InvariantCulture) / 10.0f;

                                                width *= 3.0f;

                                                AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);
                                                float terminalTime = Effects.canvas_width / (velocity * (3.0f * 0.7f)) / 2;
                                                float centerX = Effects.canvas_width_center;
                                                float centerY = Effects.canvas_height_center;
                                                if (playOption.Equals("PlayFromKeyCenter"))
                                                {
                                                    //terminalTime = duration / 1000.0f;
                                                    centerX = 0;
                                                    centerY = 0;
                                                }
                                                animTrack.SetFrame(0.0f, new AnimationGradientCircle(centerX, centerY, 0, new EffectsEngine.EffectBrush(transitions).SetBrushType(EffectsEngine.EffectBrush.BrushType.Radial), (int)width));

                                                animTrack.SetFrame(terminalTime, new AnimationGradientCircle(centerX, centerY, Effects.canvas_biggest, new EffectsEngine.EffectBrush(transitions).SetBrushType(EffectsEngine.EffectBrush.BrushType.Radial), (int)width));
                                                app.Profile.Layers.Add(new Layers.Layer()
                                                {
                                                    Name = layerName,
                                                    Enabled = layerEnabled,
                                                    Handler = new Layers.AnimationLayerHandler()
                                                    {
                                                        Properties = new Layers.AnimationLayerHandlerProperties()
                                                        {
                                                            _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                                            _Sequence = affected_keys,
                                                            _forceKeySequence = true,
                                                            _AnimationDuration = (duration / 1000.0f),
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
                                            }

                                            else
                                            {
                                                //Null, it's unknown.
                                                Global.logger.Warn("Unknown CUE Layer Type");
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
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
                string json = File.ReadAllText(filepath, Encoding.UTF8);
                ApplicationProfile inProf = (ApplicationProfile)JsonConvert.DeserializeObject(json, typeof(ApplicationProfile), new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    TypeNameHandling = TypeNameHandling.All,
                    Binder = Utils.JSONUtils.SerializationBinder
                });

                // Create a new profile on the current application (so that profiles can be imported from different applications)
                ApplicationProfile newProf = app.AddNewProfile(inProf.ProfileName);
                newProf.TriggerKeybind = inProf.TriggerKeybind.Clone();

                // Copy any valid layers from the read profile to the new one
                void ImportLayers(ObservableCollection<Layer> source, ObservableCollection<Layer> target) {
                    target.Clear();
                    for (int i = 0; i < source.Count; i++)
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
                Global.logger.Error(ex);
                System.Windows.Forms.MessageBox.Show("Error importing the profile: " + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Converts CorsairLedId to Devices.DeviceKeys
        /// </summary>
        /// <param name="CorsairKey">The CorsairLedId to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        public static DeviceKeys ToDeviceKeys(CorsairRGB.NET.Enums.CorsairLedId CorsairKey)
        {
            switch (CorsairKey)
            {
                case (CorsairLedId.K_Logo):
                    return DeviceKeys.LOGO;
                case (CorsairLedId.K_Brightness):
                    return DeviceKeys.BRIGHTNESS_SWITCH;
                case (CorsairLedId.K_WinLock):
                    return DeviceKeys.LOCK_SWITCH;

                case (CorsairLedId.K_Mute):
                    return DeviceKeys.VOLUME_MUTE;
                case (CorsairLedId.K_VolumeUp):
                    return DeviceKeys.VOLUME_UP;
                case (CorsairLedId.K_VolumeDown):
                    return DeviceKeys.VOLUME_DOWN;
                case (CorsairLedId.K_Stop):
                    return DeviceKeys.MEDIA_STOP;
                case (CorsairLedId.K_PlayPause):
                    return DeviceKeys.MEDIA_PLAY_PAUSE;
                case (CorsairLedId.K_ScanPreviousTrack):
                    return DeviceKeys.MEDIA_PREVIOUS;
                case (CorsairLedId.K_ScanNextTrack):
                    return DeviceKeys.MEDIA_NEXT;

                case (CorsairLedId.K_Escape):
                    return DeviceKeys.ESC;
                case (CorsairLedId.K_F1):
                    return DeviceKeys.F1;
                case (CorsairLedId.K_F2):
                    return DeviceKeys.F2;
                case (CorsairLedId.K_F3):
                    return DeviceKeys.F3;
                case (CorsairLedId.K_F4):
                    return DeviceKeys.F4;
                case (CorsairLedId.K_F5):
                    return DeviceKeys.F5;
                case (CorsairLedId.K_F6):
                    return DeviceKeys.F6;
                case (CorsairLedId.K_F7):
                    return DeviceKeys.F7;
                case (CorsairLedId.K_F8):
                    return DeviceKeys.F8;
                case (CorsairLedId.K_F9):
                    return DeviceKeys.F9;
                case (CorsairLedId.K_F10):
                    return DeviceKeys.F10;
                case (CorsairLedId.K_F11):
                    return DeviceKeys.F11;
                case (CorsairLedId.K_F12):
                    return DeviceKeys.F12;
                case (CorsairLedId.K_PrintScreen):
                    return DeviceKeys.PRINT_SCREEN;
                case (CorsairLedId.K_ScrollLock):
                    return DeviceKeys.SCROLL_LOCK;
                case (CorsairLedId.K_PauseBreak):
                    return DeviceKeys.PAUSE_BREAK;
                case (CorsairLedId.K_GraveAccentAndTilde):
                    return DeviceKeys.TILDE;
                case (CorsairLedId.K_1):
                    return DeviceKeys.ONE;
                case (CorsairLedId.K_2):
                    return DeviceKeys.TWO;
                case (CorsairLedId.K_3):
                    return DeviceKeys.THREE;
                case (CorsairLedId.K_4):
                    return DeviceKeys.FOUR;
                case (CorsairLedId.K_5):
                    return DeviceKeys.FIVE;
                case (CorsairLedId.K_6):
                    return DeviceKeys.SIX;
                case (CorsairLedId.K_7):
                    return DeviceKeys.SEVEN;
                case (CorsairLedId.K_8):
                    return DeviceKeys.EIGHT;
                case (CorsairLedId.K_9):
                    return DeviceKeys.NINE;
                case (CorsairLedId.K_0):
                    return DeviceKeys.ZERO;
                case (CorsairLedId.K_MinusAndUnderscore):
                    return DeviceKeys.MINUS;
                case (CorsairLedId.K_EqualsAndPlus):
                    return DeviceKeys.EQUALS;
                case (CorsairLedId.K_Backspace):
                    return DeviceKeys.BACKSPACE;
                case (CorsairLedId.K_Insert):
                    return DeviceKeys.INSERT;
                case (CorsairLedId.K_Home):
                    return DeviceKeys.HOME;
                case (CorsairLedId.K_PageUp):
                    return DeviceKeys.PAGE_UP;
                case (CorsairLedId.K_NumLock):
                    return DeviceKeys.NUM_LOCK;
                case (CorsairLedId.K_KeypadSlash):
                    return DeviceKeys.NUM_SLASH;
                case (CorsairLedId.K_KeypadAsterisk):
                    return DeviceKeys.NUM_ASTERISK;
                case (CorsairLedId.K_KeypadMinus):
                    return DeviceKeys.NUM_MINUS;
                case (CorsairLedId.K_Tab):
                    return DeviceKeys.TAB;
                case (CorsairLedId.K_Q):
                    return DeviceKeys.Q;
                case (CorsairLedId.K_W):
                    return DeviceKeys.W;
                case (CorsairLedId.K_E):
                    return DeviceKeys.E;
                case (CorsairLedId.K_R):
                    return DeviceKeys.R;
                case (CorsairLedId.K_T):
                    return DeviceKeys.T;
                case (CorsairLedId.K_Y):
                    return DeviceKeys.Y;
                case (CorsairLedId.K_U):
                    return DeviceKeys.U;
                case (CorsairLedId.K_I):
                    return DeviceKeys.I;
                case (CorsairLedId.K_O):
                    return DeviceKeys.O;
                case (CorsairLedId.K_P):
                    return DeviceKeys.P;
                case (CorsairLedId.K_BracketLeft):
                    return DeviceKeys.OPEN_BRACKET;
                case (CorsairLedId.K_BracketRight):
                    return DeviceKeys.CLOSE_BRACKET;
                case (CorsairLedId.K_Backslash):
                    return DeviceKeys.BACKSLASH;
                case (CorsairLedId.K_Delete):
                    return DeviceKeys.DELETE;
                case (CorsairLedId.K_End):
                    return DeviceKeys.END;
                case (CorsairLedId.K_PageDown):
                    return DeviceKeys.PAGE_DOWN;
                case (CorsairLedId.K_Keypad7):
                    return DeviceKeys.NUM_SEVEN;
                case (CorsairLedId.K_Keypad8):
                    return DeviceKeys.NUM_EIGHT;
                case (CorsairLedId.K_Keypad9):
                    return DeviceKeys.NUM_NINE;
                case (CorsairLedId.K_KeypadPlus):
                    return DeviceKeys.NUM_PLUS;
                case (CorsairLedId.K_CapsLock):
                    return DeviceKeys.CAPS_LOCK;
                case (CorsairLedId.K_A):
                    return DeviceKeys.A;
                case (CorsairLedId.K_S):
                    return DeviceKeys.S;
                case (CorsairLedId.K_D):
                    return DeviceKeys.D;
                case (CorsairLedId.K_F):
                    return DeviceKeys.F;
                case (CorsairLedId.K_G):
                    return DeviceKeys.G;
                case (CorsairLedId.K_H):
                    return DeviceKeys.H;
                case (CorsairLedId.K_J):
                    return DeviceKeys.J;
                case (CorsairLedId.K_K):
                    return DeviceKeys.K;
                case (CorsairLedId.K_L):
                    return DeviceKeys.L;
                case (CorsairLedId.K_SemicolonAndColon):
                    return DeviceKeys.SEMICOLON;
                case (CorsairLedId.K_ApostropheAndDoubleQuote):
                    return DeviceKeys.APOSTROPHE;
                case (CorsairLedId.K_NonUsTilde):
                    return DeviceKeys.HASHTAG;
                case (CorsairLedId.K_Enter):
                    return DeviceKeys.ENTER;
                case (CorsairLedId.K_Keypad4):
                    return DeviceKeys.NUM_FOUR;
                case (CorsairLedId.K_Keypad5):
                    return DeviceKeys.NUM_FIVE;
                case (CorsairLedId.K_Keypad6):
                    return DeviceKeys.NUM_SIX;
                case (CorsairLedId.K_LeftShift):
                    return DeviceKeys.LEFT_SHIFT;
                case (CorsairLedId.K_NonUsBackslash):
                    return DeviceKeys.BACKSLASH_UK;
                case (CorsairLedId.K_Z):
                    return DeviceKeys.Z;
                case (CorsairLedId.K_X):
                    return DeviceKeys.X;
                case (CorsairLedId.K_C):
                    return DeviceKeys.C;
                case (CorsairLedId.K_V):
                    return DeviceKeys.V;
                case (CorsairLedId.K_B):
                    return DeviceKeys.B;
                case (CorsairLedId.K_N):
                    return DeviceKeys.N;
                case (CorsairLedId.K_M):
                    return DeviceKeys.M;
                case (CorsairLedId.K_CommaAndLessThan):
                    return DeviceKeys.COMMA;
                case (CorsairLedId.K_PeriodAndBiggerThan):
                    return DeviceKeys.PERIOD;
                case (CorsairLedId.K_SlashAndQuestionMark):
                    return DeviceKeys.FORWARD_SLASH;
                case (CorsairLedId.K_RightShift):
                    return DeviceKeys.RIGHT_SHIFT;
                case (CorsairLedId.K_UpArrow):
                    return DeviceKeys.ARROW_UP;
                case (CorsairLedId.K_Keypad1):
                    return DeviceKeys.NUM_ONE;
                case (CorsairLedId.K_Keypad2):
                    return DeviceKeys.NUM_TWO;
                case (CorsairLedId.K_Keypad3):
                    return DeviceKeys.NUM_THREE;
                case (CorsairLedId.K_KeypadEnter):
                    return DeviceKeys.NUM_ENTER;
                case (CorsairLedId.K_LeftCtrl):
                    return DeviceKeys.LEFT_CONTROL;
                case (CorsairLedId.K_LeftGui):
                    return DeviceKeys.LEFT_WINDOWS;
                case (CorsairLedId.K_LeftAlt):
                    return DeviceKeys.LEFT_ALT;
                case (CorsairLedId.K_Space):
                    return DeviceKeys.SPACE;
                case (CorsairLedId.K_RightAlt):
                    return DeviceKeys.RIGHT_ALT;
                case (CorsairLedId.K_RightGui):
                    return DeviceKeys.RIGHT_WINDOWS;
                case (CorsairLedId.K_Application):
                    return DeviceKeys.APPLICATION_SELECT;
                case (CorsairLedId.K_RightCtrl):
                    return DeviceKeys.RIGHT_CONTROL;
                case (CorsairLedId.K_LeftArrow):
                    return DeviceKeys.ARROW_LEFT;
                case (CorsairLedId.K_DownArrow):
                    return DeviceKeys.ARROW_DOWN;
                case (CorsairLedId.K_RightArrow):
                    return DeviceKeys.ARROW_RIGHT;
                case (CorsairLedId.K_Keypad0):
                    return DeviceKeys.NUM_ZERO;
                case (CorsairLedId.K_KeypadPeriodAndDelete):
                    return DeviceKeys.NUM_PERIOD;

                case (CorsairLedId.K_Fn):
                    return DeviceKeys.FN_Key;

                case (CorsairLedId.K_G1):
                    return DeviceKeys.G1;
                case (CorsairLedId.K_G2):
                    return DeviceKeys.G2;
                case (CorsairLedId.K_G3):
                    return DeviceKeys.G3;
                case (CorsairLedId.K_G4):
                    return DeviceKeys.G4;
                case (CorsairLedId.K_G5):
                    return DeviceKeys.G5;
                case (CorsairLedId.K_G6):
                    return DeviceKeys.G6;
                case (CorsairLedId.K_G7):
                    return DeviceKeys.G7;
                case (CorsairLedId.K_G8):
                    return DeviceKeys.G8;
                case (CorsairLedId.K_G9):
                    return DeviceKeys.G9;
                case (CorsairLedId.K_G10):
                    return DeviceKeys.G10;
                case (CorsairLedId.K_G11):
                    return DeviceKeys.G11;
                case (CorsairLedId.K_G12):
                    return DeviceKeys.G12;
                case (CorsairLedId.K_G13):
                    return DeviceKeys.G13;
                case (CorsairLedId.K_G14):
                    return DeviceKeys.G14;
                case (CorsairLedId.K_G15):
                    return DeviceKeys.G15;
                case (CorsairLedId.K_G16):
                    return DeviceKeys.G16;
                case (CorsairLedId.K_G17):
                    return DeviceKeys.G17;
                case (CorsairLedId.K_G18):
                    return DeviceKeys.G18;

                default:
                    return DeviceKeys.NONE;
            }
        }
    }
}
