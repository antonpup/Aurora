using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aurora.Settings {
    public static class ProfileImporter {

        /// <summary>
        /// Imports a file from disk as a profile into the given application.
        /// </summary>
        /// <param name="filepath">The full filepath of the file to import.</param>
        public static void ImportProfile(this Application app, string filepath) {
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
                                                if (stopOnRelease||stopOnPress)
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
                                                    CUE.NET.Devices.Generic.Enums.CorsairLedId keyValue;

                                                    switch (key.Value)
                                                    {
                                                        case "0":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D0;
                                                            break;
                                                        case "1":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D1;
                                                            break;
                                                        case "2":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D2;
                                                            break;
                                                        case "3":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D3;
                                                            break;
                                                        case "4":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D4;
                                                            break;
                                                        case "5":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D5;
                                                            break;
                                                        case "6":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D6;
                                                            break;
                                                        case "7":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D7;
                                                            break;
                                                        case "8":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D8;
                                                            break;
                                                        case "9":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D9;
                                                            break;
                                                        case "Led_KeyboardLogo":
                                                            keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.Logo;
                                                            break;
                                                        default:
                                                            if (key.Value.StartsWith("Led_Top"))
                                                                key.Value = "G18";
                                                            keyValue = (CUE.NET.Devices.Generic.Enums.CorsairLedId)Enum.Parse(typeof(CUE.NET.Devices.Generic.Enums.CorsairLedId), key.Value);
                                                            break;
                                                    }

                                                    if (Enum.IsDefined(typeof(CUE.NET.Devices.Generic.Enums.CorsairLedId), keyValue) | keyValue.ToString().Contains(","))
                                                    {
                                                        Devices.DeviceKeys deviceKey = Utils.KeyUtils.ToDeviceKeys(keyValue);

                                                        if (deviceKey != Devices.DeviceKeys.NONE)
                                                        {
                                                            affected_keys.keys.Add(deviceKey);
                                                            bindings.Add(new Keybind(new System.Windows.Forms.Keys[] { Utils.KeyUtils.GetFormsKey(deviceKey) }));
                                                        }
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    Global.logger.Debug("Exception in profile");
                                                    //break;
                                                }
                                            }

                                            var lightingInfo = layer.Element("ptr_wrapper").Element("data").Element("lighting");
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
                                                            _PrimaryColor = System.Drawing.ColorTranslator.FromHtml(transitionInfo.Element("value0").Element("color").Value)
                                                        },
                                                        Opacity = int.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("opacity").Value) / 255.0f
                                                    }
                                                });
                                            }
                                            else if ("GradientLighting".Equals(layerPolyName))
                                            {
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);
                                                AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                                Dictionary<float, System.Drawing.Color> transitions = new Dictionary<float, System.Drawing.Color>();

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value);
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
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);
                                                AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                                Dictionary<float, System.Drawing.Color> transitions = new Dictionary<float, System.Drawing.Color>();

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value);
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
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);

                                                List<AnimationTrack> animTracks = new List<AnimationTrack>();

                                                EffectsEngine.ColorSpectrum transitions = new EffectsEngine.ColorSpectrum();

                                                float smallest = 0.5f;
                                                float largest = 0.5f;

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value);
                                                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                        transitions.SetColorAt(time, color);

                                                        if (time < smallest)
                                                            smallest = time;
                                                        else if (time > largest)
                                                            largest = time;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Global.logger.Debug("Wave Ex");
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

                                                float velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value) / 10.0f;
                                                float width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value) / 10.0f;
                                                bool isDoubleSided = bool.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("isDoublesided").Value);
                                                float angle = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("angle").Value);

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

                                                    float terminalTime = (Effects.canvas_width + width) / (velocity * (3.0f * 0.7f));

                                                    if (angle >= 315 || angle <= 45)
                                                    {
                                                        float _angleOffset = (width / 2.0f) * (float)Math.Cos((double)angle * (Math.PI / 180.0));
                                                        _angleOffset = (width / 2.0f) - _angleOffset;

                                                        terminalTime = (Effects.canvas_width + width + 2.0f * _angleOffset) / (velocity * (3.0f * 0.7f));

                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-width - _angleOffset, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width + _angleOffset, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                    }
                                                    else if (angle > 45 && angle < 135)
                                                    {
                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, Effects.canvas_height + width / 2, width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, (Effects.canvas_height + width / 2) - (Effects.canvas_width + width), width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                    }
                                                    else if (angle >= 135 && angle <= 225)
                                                    {
                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width + width, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-width, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                    }
                                                    else if (angle > 225 && angle < 315)
                                                    {
                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, -width / 2, width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                        animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, (-width / 2) + (Effects.canvas_width + width), width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
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

                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, 0, Effects.canvas_height * 3, _initialBrushRight).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                            _fillBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + width, 0);
                                                            _fillBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center, 0);

                                                            animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, width, Effects.canvas_height * 3, _fillBrushRight).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_fillBrushRight);
                                                            _terminalBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset, 0);
                                                            _terminalBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset - width, 0);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center + _terminalOffset - width, -Effects.canvas_height, width, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));

                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                            _terminalBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset, 0);
                                                            _terminalBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset - width, 0);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, _terminalOffset, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));
                                                        }

                                                        //Left Side
                                                        EffectsEngine.EffectBrush _initialBrushLeft = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center, 0);
                                                        _initialBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center + width, 0);

                                                        animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, 0, Effects.canvas_height * 3, _initialBrushLeft).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _fillBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - width, 0);
                                                            _fillBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center, 0);

                                                            animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - width, -Effects.canvas_height, width, Effects.canvas_height * 3, _fillBrushLeft).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _terminalBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset, 0);
                                                            _terminalBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset + width, 0);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - _terminalOffset, -Effects.canvas_height, width, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                            _terminalBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset, 0);
                                                            _terminalBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset + width, 0);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - _terminalOffset, -Effects.canvas_height, _terminalOffset, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                        }
                                                    }
                                                    else if ((angle > 45 && angle < 135) || (angle > 225 && angle < 315))
                                                    {
                                                        angle -= 90;

                                                        //Bottom Side
                                                        EffectsEngine.EffectBrush _initialBrushBottom = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center);
                                                        _initialBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center - width);

                                                        animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, 0, _initialBrushBottom).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                            _fillBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + width);
                                                            _fillBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center);


                                                            animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, width, _fillBrushBottom).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_fillBrushBottom);
                                                            _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset);
                                                            _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset - width);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center + _terminalOffset - width, Effects.canvas_width * 3, width, _terminalBrushBottom).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                            _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset);
                                                            _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset - width);

                                                            animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, _terminalOffset, _terminalBrushBottom).SetAngle(angle));
                                                        }

                                                        //Top Side
                                                        EffectsEngine.EffectBrush _initialBrushtTop = new EffectsEngine.EffectBrush(transitions);
                                                        _initialBrushtTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center);
                                                        _initialBrushtTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center + width);

                                                        animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, 0, _initialBrushtTop).SetAngle(angle));

                                                        if (_widthFillTime < _terminalTime)
                                                        {
                                                            EffectsEngine.EffectBrush _fillBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _fillBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - width);
                                                            _fillBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center);

                                                            animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center - width, Effects.canvas_width * 3, width, _fillBrushTop).SetAngle(angle));

                                                            EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset);
                                                            _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset + width);
                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center - _terminalOffset, Effects.canvas_width * 3, width, _terminalBrushTop).SetAngle(angle));
                                                        }
                                                        else
                                                        {
                                                            EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                            _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset);
                                                            _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset + width);

                                                            animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center - _terminalOffset, Effects.canvas_width * 3, _terminalOffset, _terminalBrushTop).SetAngle(angle));
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
                                                float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);

                                                EffectsEngine.ColorSpectrum transitions = new EffectsEngine.ColorSpectrum();

                                                float smallest = 0.5f;
                                                float largest = 0.5f;

                                                foreach (XElement transition in transitionInfo.Elements())
                                                {
                                                    try
                                                    {
                                                        float time = float.Parse(transition.Element("time").Value);
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

                                                float velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value) / 10.0f;
                                                float width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value) / 10.0f;

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
        private static void ImportJson(this Application app, string filepath) {
            try {
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
                newProf.Layers.Clear();

                // Copy any valid layers from the read profile to the new one
                for (int i = 0; i < inProf.Layers.Count; i++)
                    if (Global.LightingStateManager.DefaultLayerHandlers.Contains(inProf.Layers[i].Handler.ID) || app.Config.ExtraAvailableLayers.Contains(inProf.Layers[i].Handler.ID))
                        newProf.Layers.Add((Layer)inProf.Layers[i].Clone());

                // Force a save to write the new profile to disk in the appdata dir
                app.SaveProfiles();

            } catch (Exception ex) {
                Global.logger.Error(ex);
                System.Windows.Forms.MessageBox.Show("Error importing the profile: " + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
