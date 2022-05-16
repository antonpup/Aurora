using Aurora.Devices;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Settings.Layers;
using Corsair.CUE.SDK;
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
using Aurora.Utils;

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
                                                    CorsairLedId? keyValue = null;

                                                    switch (key.Value)
                                                    {
                                                        case "0":
                                                            keyValue = CorsairLedId.CLK_0;
                                                            break;
                                                        case "1":
                                                            keyValue = CorsairLedId.CLK_1;
                                                            break;
                                                        case "2":
                                                            keyValue = CorsairLedId.CLK_2;
                                                            break;
                                                        case "3":
                                                            keyValue = CorsairLedId.CLK_3;
                                                            break;
                                                        case "4":
                                                            keyValue = CorsairLedId.CLK_4;
                                                            break;
                                                        case "5":
                                                            keyValue = CorsairLedId.CLK_5;
                                                            break;
                                                        case "6":
                                                            keyValue = CorsairLedId.CLK_6;
                                                            break;
                                                        case "7":
                                                            keyValue = CorsairLedId.CLK_7;
                                                            break;
                                                        case "8":
                                                            keyValue = CorsairLedId.CLK_8;
                                                            break;
                                                        case "9":
                                                            keyValue = CorsairLedId.CLK_9;
                                                            break;
                                                        case "Led_KeyboardLogo":
                                                            keyValue = CorsairLedId.CLK_Logo;
                                                            break;
                                                        case "Led_KeyboardTopLogo2":
                                                            keyValue = CorsairLedId.CLK_Logo;
                                                            break;
                                                        default:
                                                            if (key.Value.StartsWith("Led_Top"))
                                                                key.Value = "G18";
                                                            if(Enum.TryParse<CorsairLedId>("CLK_" + key.Value, out var corsairLedId)){
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
                                                float centerX = Effects.CanvasWidthCenter;
                                                float centerY = Effects.CanvasHeightCenter;
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
                                                        _initialBrushRight.start = new System.Drawing.PointF(Effects.CanvasWidthCenter, 0);
                                                        _initialBrushRight.end = new System.Drawing.PointF(Effects.CanvasWidthCenter - width, 0);

                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, 0, Effects.canvas_height * 3, _initialBrushRight).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                            _fillBrushRight.start = new System.Drawing.PointF(Effects.CanvasWidthCenter + width, 0);
                                                            _fillBrushRight.end = new System.Drawing.PointF(Effects.CanvasWidthCenter, 0);

                                                            animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, width, Effects.canvas_height * 3, _fillBrushRight).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_fillBrushRight);
                                                            _terminalBrushRight.start = new System.Drawing.PointF(Effects.CanvasWidthCenter + _terminalOffset, 0);
                                                            _terminalBrushRight.end = new System.Drawing.PointF(Effects.CanvasWidthCenter + _terminalOffset - width, 0);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter + _terminalOffset - width, 0, width, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));

                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                            _terminalBrushRight.start = new System.Drawing.PointF(Effects.CanvasWidthCenter + _terminalOffset, 0);
                                                            _terminalBrushRight.end = new System.Drawing.PointF(Effects.CanvasWidthCenter + _terminalOffset - width, 0);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, _terminalOffset, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));
                                                        }

                                                        //Left Side
                                                        EffectsEngine.EffectBrush _initialBrushLeft = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushLeft.start = new System.Drawing.PointF(Effects.CanvasWidthCenter, 0);
                                                        _initialBrushLeft.end = new System.Drawing.PointF(Effects.CanvasWidthCenter + width, 0);

                                                        animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter, 0, 0, Effects.canvas_height * 3, _initialBrushLeft).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _fillBrushLeft.start = new System.Drawing.PointF(Effects.CanvasWidthCenter - width, 0);
                                                            _fillBrushLeft.end = new System.Drawing.PointF(Effects.CanvasWidthCenter, 0);

                                                            animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter - width, 0, width, Effects.canvas_height * 3, _fillBrushLeft).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _terminalBrushLeft.start = new System.Drawing.PointF(Effects.CanvasWidthCenter - _terminalOffset, 0);
                                                            _terminalBrushLeft.end = new System.Drawing.PointF(Effects.CanvasWidthCenter - _terminalOffset + width, 0);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter - _terminalOffset, 0, width, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _terminalBrushLeft.start = new System.Drawing.PointF(Effects.CanvasWidthCenter - _terminalOffset, 0);
                                                            _terminalBrushLeft.end = new System.Drawing.PointF(Effects.CanvasWidthCenter - _terminalOffset + width, 0);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.CanvasWidthCenter - _terminalOffset, 0, _terminalOffset, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                        }
                                                    }
                                                    else if ((angle > 45 && angle < 135) || (angle > 225 && angle < 315))
                                                    {
                                                        angle -= 90;

                                                        //Bottom Side
                                                        EffectsEngine.EffectBrush _initialBrushBottom = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushBottom.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter);
                                                        _initialBrushBottom.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter - width);

                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.canvas_width * 3, 0, _initialBrushBottom).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                            _fillBrushBottom.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter + width);
                                                            _fillBrushBottom.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter);


                                                            animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.canvas_width * 3, width, _fillBrushBottom).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_fillBrushBottom);
                                                            _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter + _terminalOffset);
                                                            _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter + _terminalOffset - width);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter + _terminalOffset - width, Effects.canvas_width * 3, width, _terminalBrushBottom).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                            _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter + _terminalOffset);
                                                            _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter + _terminalOffset - width);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.canvas_width * 3, _terminalOffset, _terminalBrushBottom).SetAngle(angle));
                                                        }

                                                        //Top Side
                                                        EffectsEngine.EffectBrush _initialBrushtTop = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushtTop.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter);
                                                        _initialBrushtTop.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter + width);

                                                        animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter, Effects.canvas_width * 3, 0, _initialBrushtTop).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _fillBrushTop.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter - width);
                                                            _fillBrushTop.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter);

                                                            animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter - width, Effects.canvas_width * 3, width, _fillBrushTop).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter - _terminalOffset);
                                                            _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter - _terminalOffset + width);
                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter - _terminalOffset, Effects.canvas_width * 3, width, _terminalBrushTop).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.CanvasHeightCenter - _terminalOffset);
                                                            _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.CanvasHeightCenter - _terminalOffset + width);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(0, Effects.CanvasHeightCenter - _terminalOffset, Effects.canvas_width * 3, _terminalOffset, _terminalBrushTop).SetAngle(angle));
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
                                                float centerX = Effects.CanvasWidthCenter;
                                                float centerY = Effects.CanvasHeightCenter;
                                                if (playOption.Equals("PlayFromKeyCenter"))
                                                {
                                                    //terminalTime = duration / 1000.0f;
                                                    centerX = 0;
                                                    centerY = 0;
                                                }
                                                animTrack.SetFrame(0.0f, new AnimationGradientCircle(centerX, centerY, 0, new EffectsEngine.EffectBrush(transitions).SetBrushType(EffectsEngine.EffectBrush.BrushType.Radial), (int)width));

                                                animTrack.SetFrame(terminalTime, new AnimationGradientCircle(centerX, centerY, Effects.CanvasBiggest, new EffectsEngine.EffectBrush(transitions).SetBrushType(EffectsEngine.EffectBrush.BrushType.Radial), (int)width));
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
                    Binder = JSONUtils.SerializationBinder,
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
        public static DeviceKeys ToDeviceKeys(CorsairLedId CorsairKey)
        {
            switch (CorsairKey)
            {
                case (CorsairLedId.CLK_Logo):
                    return DeviceKeys.LOGO;
                case (CorsairLedId.CLK_Brightness):
                    return DeviceKeys.BRIGHTNESS_SWITCH;
                case (CorsairLedId.CLK_WinLock):
                    return DeviceKeys.LOCK_SWITCH;

                case (CorsairLedId.CLK_Mute):
                    return DeviceKeys.VOLUME_MUTE;
                case (CorsairLedId.CLK_VolumeUp):
                    return DeviceKeys.VOLUME_UP;
                case (CorsairLedId.CLK_VolumeDown):
                    return DeviceKeys.VOLUME_DOWN;
                case (CorsairLedId.CLK_Stop):
                    return DeviceKeys.MEDIA_STOP;
                case (CorsairLedId.CLK_PlayPause):
                    return DeviceKeys.MEDIA_PLAY_PAUSE;
                case (CorsairLedId.CLK_ScanPreviousTrack):
                    return DeviceKeys.MEDIA_PREVIOUS;
                case (CorsairLedId.CLK_ScanNextTrack):
                    return DeviceKeys.MEDIA_NEXT;

                case (CorsairLedId.CLK_Escape):
                    return DeviceKeys.ESC;
                case (CorsairLedId.CLK_F1):
                    return DeviceKeys.F1;
                case (CorsairLedId.CLK_F2):
                    return DeviceKeys.F2;
                case (CorsairLedId.CLK_F3):
                    return DeviceKeys.F3;
                case (CorsairLedId.CLK_F4):
                    return DeviceKeys.F4;
                case (CorsairLedId.CLK_F5):
                    return DeviceKeys.F5;
                case (CorsairLedId.CLK_F6):
                    return DeviceKeys.F6;
                case (CorsairLedId.CLK_F7):
                    return DeviceKeys.F7;
                case (CorsairLedId.CLK_F8):
                    return DeviceKeys.F8;
                case (CorsairLedId.CLK_F9):
                    return DeviceKeys.F9;
                case (CorsairLedId.CLK_F10):
                    return DeviceKeys.F10;
                case (CorsairLedId.CLK_F11):
                    return DeviceKeys.F11;
                case (CorsairLedId.CLK_F12):
                    return DeviceKeys.F12;
                case (CorsairLedId.CLK_PrintScreen):
                    return DeviceKeys.PRINT_SCREEN;
                case (CorsairLedId.CLK_ScrollLock):
                    return DeviceKeys.SCROLL_LOCK;
                case (CorsairLedId.CLK_PauseBreak):
                    return DeviceKeys.PAUSE_BREAK;
                case (CorsairLedId.CLK_GraveAccentAndTilde):
                    return DeviceKeys.TILDE;
                case (CorsairLedId.CLK_1):
                    return DeviceKeys.ONE;
                case (CorsairLedId.CLK_2):
                    return DeviceKeys.TWO;
                case (CorsairLedId.CLK_3):
                    return DeviceKeys.THREE;
                case (CorsairLedId.CLK_4):
                    return DeviceKeys.FOUR;
                case (CorsairLedId.CLK_5):
                    return DeviceKeys.FIVE;
                case (CorsairLedId.CLK_6):
                    return DeviceKeys.SIX;
                case (CorsairLedId.CLK_7):
                    return DeviceKeys.SEVEN;
                case (CorsairLedId.CLK_8):
                    return DeviceKeys.EIGHT;
                case (CorsairLedId.CLK_9):
                    return DeviceKeys.NINE;
                case (CorsairLedId.CLK_0):
                    return DeviceKeys.ZERO;
                case (CorsairLedId.CLK_MinusAndUnderscore):
                    return DeviceKeys.MINUS;
                case (CorsairLedId.CLK_EqualsAndPlus):
                    return DeviceKeys.EQUALS;
                case (CorsairLedId.CLK_Backspace):
                    return DeviceKeys.BACKSPACE;
                case (CorsairLedId.CLK_Insert):
                    return DeviceKeys.INSERT;
                case (CorsairLedId.CLK_Home):
                    return DeviceKeys.HOME;
                case (CorsairLedId.CLK_PageUp):
                    return DeviceKeys.PAGE_UP;
                case (CorsairLedId.CLK_NumLock):
                    return DeviceKeys.NUM_LOCK;
                case (CorsairLedId.CLK_KeypadSlash):
                    return DeviceKeys.NUM_SLASH;
                case (CorsairLedId.CLK_KeypadAsterisk):
                    return DeviceKeys.NUM_ASTERISK;
                case (CorsairLedId.CLK_KeypadMinus):
                    return DeviceKeys.NUM_MINUS;
                case (CorsairLedId.CLK_Tab):
                    return DeviceKeys.TAB;
                case (CorsairLedId.CLK_Q):
                    return DeviceKeys.Q;
                case (CorsairLedId.CLK_W):
                    return DeviceKeys.W;
                case (CorsairLedId.CLK_E):
                    return DeviceKeys.E;
                case (CorsairLedId.CLK_R):
                    return DeviceKeys.R;
                case (CorsairLedId.CLK_T):
                    return DeviceKeys.T;
                case (CorsairLedId.CLK_Y):
                    return DeviceKeys.Y;
                case (CorsairLedId.CLK_U):
                    return DeviceKeys.U;
                case (CorsairLedId.CLK_I):
                    return DeviceKeys.I;
                case (CorsairLedId.CLK_O):
                    return DeviceKeys.O;
                case (CorsairLedId.CLK_P):
                    return DeviceKeys.P;
                case (CorsairLedId.CLK_BracketLeft):
                    return DeviceKeys.OPEN_BRACKET;
                case (CorsairLedId.CLK_BracketRight):
                    return DeviceKeys.CLOSE_BRACKET;
                case (CorsairLedId.CLK_Backslash):
                    return DeviceKeys.BACKSLASH;
                case (CorsairLedId.CLK_Delete):
                    return DeviceKeys.DELETE;
                case (CorsairLedId.CLK_End):
                    return DeviceKeys.END;
                case (CorsairLedId.CLK_PageDown):
                    return DeviceKeys.PAGE_DOWN;
                case (CorsairLedId.CLK_Keypad7):
                    return DeviceKeys.NUM_SEVEN;
                case (CorsairLedId.CLK_Keypad8):
                    return DeviceKeys.NUM_EIGHT;
                case (CorsairLedId.CLK_Keypad9):
                    return DeviceKeys.NUM_NINE;
                case (CorsairLedId.CLK_KeypadPlus):
                    return DeviceKeys.NUM_PLUS;
                case (CorsairLedId.CLK_CapsLock):
                    return DeviceKeys.CAPS_LOCK;
                case (CorsairLedId.CLK_A):
                    return DeviceKeys.A;
                case (CorsairLedId.CLK_S):
                    return DeviceKeys.S;
                case (CorsairLedId.CLK_D):
                    return DeviceKeys.D;
                case (CorsairLedId.CLK_F):
                    return DeviceKeys.F;
                case (CorsairLedId.CLK_G):
                    return DeviceKeys.G;
                case (CorsairLedId.CLK_H):
                    return DeviceKeys.H;
                case (CorsairLedId.CLK_J):
                    return DeviceKeys.J;
                case (CorsairLedId.CLK_K):
                    return DeviceKeys.K;
                case (CorsairLedId.CLK_L):
                    return DeviceKeys.L;
                case (CorsairLedId.CLK_SemicolonAndColon):
                    return DeviceKeys.SEMICOLON;
                case (CorsairLedId.CLK_ApostropheAndDoubleQuote):
                    return DeviceKeys.APOSTROPHE;
                case (CorsairLedId.CLK_NonUsTilde):
                    return DeviceKeys.HASHTAG;
                case (CorsairLedId.CLK_Enter):
                    return DeviceKeys.ENTER;
                case (CorsairLedId.CLK_Keypad4):
                    return DeviceKeys.NUM_FOUR;
                case (CorsairLedId.CLK_Keypad5):
                    return DeviceKeys.NUM_FIVE;
                case (CorsairLedId.CLK_Keypad6):
                    return DeviceKeys.NUM_SIX;
                case (CorsairLedId.CLK_LeftShift):
                    return DeviceKeys.LEFT_SHIFT;
                case (CorsairLedId.CLK_NonUsBackslash):
                    return DeviceKeys.BACKSLASH_UK;
                case (CorsairLedId.CLK_Z):
                    return DeviceKeys.Z;
                case (CorsairLedId.CLK_X):
                    return DeviceKeys.X;
                case (CorsairLedId.CLK_C):
                    return DeviceKeys.C;
                case (CorsairLedId.CLK_V):
                    return DeviceKeys.V;
                case (CorsairLedId.CLK_B):
                    return DeviceKeys.B;
                case (CorsairLedId.CLK_N):
                    return DeviceKeys.N;
                case (CorsairLedId.CLK_M):
                    return DeviceKeys.M;
                case (CorsairLedId.CLK_CommaAndLessThan):
                    return DeviceKeys.COMMA;
                case (CorsairLedId.CLK_PeriodAndBiggerThan):
                    return DeviceKeys.PERIOD;
                case (CorsairLedId.CLK_SlashAndQuestionMark):
                    return DeviceKeys.FORWARD_SLASH;
                case (CorsairLedId.CLK_RightShift):
                    return DeviceKeys.RIGHT_SHIFT;
                case (CorsairLedId.CLK_UpArrow):
                    return DeviceKeys.ARROW_UP;
                case (CorsairLedId.CLK_Keypad1):
                    return DeviceKeys.NUM_ONE;
                case (CorsairLedId.CLK_Keypad2):
                    return DeviceKeys.NUM_TWO;
                case (CorsairLedId.CLK_Keypad3):
                    return DeviceKeys.NUM_THREE;
                case (CorsairLedId.CLK_KeypadEnter):
                    return DeviceKeys.NUM_ENTER;
                case (CorsairLedId.CLK_LeftCtrl):
                    return DeviceKeys.LEFT_CONTROL;
                case (CorsairLedId.CLK_LeftGui):
                    return DeviceKeys.LEFT_WINDOWS;
                case (CorsairLedId.CLK_LeftAlt):
                    return DeviceKeys.LEFT_ALT;
                case (CorsairLedId.CLK_Space):
                    return DeviceKeys.SPACE;
                case (CorsairLedId.CLK_RightAlt):
                    return DeviceKeys.RIGHT_ALT;
                case (CorsairLedId.CLK_RightGui):
                    return DeviceKeys.RIGHT_WINDOWS;
                case (CorsairLedId.CLK_Application):
                    return DeviceKeys.APPLICATION_SELECT;
                case (CorsairLedId.CLK_RightCtrl):
                    return DeviceKeys.RIGHT_CONTROL;
                case (CorsairLedId.CLK_LeftArrow):
                    return DeviceKeys.ARROW_LEFT;
                case (CorsairLedId.CLK_DownArrow):
                    return DeviceKeys.ARROW_DOWN;
                case (CorsairLedId.CLK_RightArrow):
                    return DeviceKeys.ARROW_RIGHT;
                case (CorsairLedId.CLK_Keypad0):
                    return DeviceKeys.NUM_ZERO;
                case (CorsairLedId.CLK_KeypadPeriodAndDelete):
                    return DeviceKeys.NUM_PERIOD;

                case (CorsairLedId.CLK_Fn):
                    return DeviceKeys.FN_Key;

                case (CorsairLedId.CLK_G1):
                    return DeviceKeys.G1;
                case (CorsairLedId.CLK_G2):
                    return DeviceKeys.G2;
                case (CorsairLedId.CLK_G3):
                    return DeviceKeys.G3;
                case (CorsairLedId.CLK_G4):
                    return DeviceKeys.G4;
                case (CorsairLedId.CLK_G5):
                    return DeviceKeys.G5;
                case (CorsairLedId.CLK_G6):
                    return DeviceKeys.G6;
                case (CorsairLedId.CLK_G7):
                    return DeviceKeys.G7;
                case (CorsairLedId.CLK_G8):
                    return DeviceKeys.G8;
                case (CorsairLedId.CLK_G9):
                    return DeviceKeys.G9;
                case (CorsairLedId.CLK_G10):
                    return DeviceKeys.G10;
                case (CorsairLedId.CLK_G11):
                    return DeviceKeys.G11;
                case (CorsairLedId.CLK_G12):
                    return DeviceKeys.G12;
                case (CorsairLedId.CLK_G13):
                    return DeviceKeys.G13;
                case (CorsairLedId.CLK_G14):
                    return DeviceKeys.G14;
                case (CorsairLedId.CLK_G15):
                    return DeviceKeys.G15;
                case (CorsairLedId.CLK_G16):
                    return DeviceKeys.G16;
                case (CorsairLedId.CLK_G17):
                    return DeviceKeys.G17;
                case (CorsairLedId.CLK_G18):
                    return DeviceKeys.G18;

                default:
                    return DeviceKeys.NONE;
            }
        }
    }
}
