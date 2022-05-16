using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Profiles.Desktop;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
using SharpDX.RawInput;
using UserControl = System.Windows.Controls.UserControl;

namespace Aurora.Settings.Layers
{
    public class input_item
    {
        public enum input_type
        {
            AnimationMix,
            Spectrum
        }

        public DeviceKeys key;
        public float progress;
        public bool waitOnKeyUp;
        public AnimationMix animation;
        public ColorSpectrum spectrum;
        public readonly input_type type;

        public input_item(DeviceKeys key, float progress, bool waitOnKeyUp, AnimationMix animation)
        {
            this.key = key;
            this.progress = progress;
            this.waitOnKeyUp = waitOnKeyUp;
            this.animation = animation;

            type = input_type.AnimationMix;
        }

        public input_item(DeviceKeys key, float progress, bool waitOnKeyUp, ColorSpectrum spectrum)
        {
            this.key = key;
            this.progress = progress;
            this.waitOnKeyUp = waitOnKeyUp;
            this.spectrum = spectrum;

            type = input_type.Spectrum;
        }
    }

    public class InteractiveLayerHandlerProperties : LayerHandlerProperties2Color<InteractiveLayerHandlerProperties>
    {
        public bool? _RandomPrimaryColor { get; set; }

        [JsonIgnore]
        public bool RandomPrimaryColor { get { return Logic._RandomPrimaryColor ?? _RandomPrimaryColor ?? false; } }

        public bool? _RandomSecondaryColor { get; set; }

        [JsonIgnore]
        public bool RandomSecondaryColor { get { return Logic._RandomSecondaryColor ?? _RandomSecondaryColor ?? false; } }

        [LogicOverridable("Effect Speed")]
        public float? _EffectSpeed { get; set; }

        public bool? _WaitOnKeyUp { get; set; }

        [JsonIgnore]
        public bool WaitOnKeyUp { get { return Logic._WaitOnKeyUp ?? _WaitOnKeyUp ?? false; } }

        [JsonIgnore]
        public float EffectSpeed { get { return Logic._EffectSpeed ?? _EffectSpeed ?? 0.0f; } }

        [LogicOverridable("Interactive Effect")]
        public InteractiveEffects? _InteractiveEffect { get; set; }

        [JsonIgnore]
        public InteractiveEffects InteractiveEffect { get { return Logic._InteractiveEffect ?? _InteractiveEffect ?? InteractiveEffects.None; } }

        [LogicOverridable("Effect Width")]
        public int? _EffectWidth { get; set; }

        [JsonIgnore]
        public int EffectWidth { get { return Logic._EffectWidth ?? _EffectWidth ?? 0; } }
        
        public bool? _UsePressBuffer { get; set; }
        [JsonIgnore]
        public bool UsePressBuffer => Logic._UsePressBuffer ?? _UsePressBuffer ?? true;

        public InteractiveLayerHandlerProperties()
        { }

        public InteractiveLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _RandomPrimaryColor = false;
            _RandomSecondaryColor = false;
            _WaitOnKeyUp = false;
            _EffectSpeed = 1.0f;
            _InteractiveEffect = InteractiveEffects.None;
            _EffectWidth = 2;
            _UsePressBuffer = true;
        }
    }

    public class InteractiveLayerHandler : LayerHandler<InteractiveLayerHandlerProperties>
    {

        private readonly EffectLayer _interactiveLayer = new("Interactive Effects");
        private readonly Func<KeyValuePair<DeviceKeys, long>, bool> _keysToRemove;
        private readonly List<input_item> _inputList = new();
        
        private Keys _previousKey = Keys.None;
        private long _previousTime;
        private long _currentTime;

        private input_item _holdKeyInputItem;

        public InteractiveLayerHandler()
        {
            Global.InputEvents.KeyDown += InputEventsKeyDown;
            Global.InputEvents.KeyUp += InputEventsKeyUp;
            _keysToRemove = lengthPresses => !Properties.UsePressBuffer || _currentTime - lengthPresses.Value > PressBuffer;
        }

        private float GetDeltaTime()
        {
            return (_currentTime - _previousTime) / 1000.0f;
        }

        protected override UserControl CreateControl()
        {
            return new Control_InteractiveLayer(this);
        }

        private void InputEventsKeyUp(object sender, KeyboardInputEventArgs e)
        {
            if (Time.GetMillisecondsSinceEpoch() - _previousTime > 1000L)
                return; //This event wasn't used for at least 1 second

            var deviceKey = e.GetDeviceKey();
            if (deviceKey != DeviceKeys.NONE)
            {
                foreach (var input in _inputList.ToArray())
                {
                    if (input.waitOnKeyUp && input.key == deviceKey)
                        input.waitOnKeyUp = false;
                }
            }

            if (_previousKey == e.Key)
                _previousKey = Keys.None;
        }

        private readonly ConcurrentDictionary<DeviceKeys, long> _timeOfLastPress = new();
        private const long PressBuffer = 300L;

        private void InputEventsKeyDown(object sender, KeyboardInputEventArgs e)
        {
            if (Time.GetMillisecondsSinceEpoch() - _previousTime > 1000L)
                return; //This event wasn't used for at least 1 second


            if (_previousKey == e.Key)
                return;

            long? currentTime = null;
            var deviceKey = e.GetDeviceKey();

            if (_timeOfLastPress.ContainsKey(deviceKey))
            {
                if (Properties.UsePressBuffer && (currentTime = Time.GetMillisecondsSinceEpoch()) - _timeOfLastPress[deviceKey] < PressBuffer)
                    return;
                _timeOfLastPress.TryRemove(deviceKey, out _);
            }

            if (deviceKey == DeviceKeys.NONE || Properties.Sequence.keys.Contains(deviceKey)) return;
            var pt = Effects.GetBitmappingFromDeviceKey(deviceKey).Center;
            if (pt.IsEmpty) return;
            
            _timeOfLastPress.TryAdd(deviceKey, currentTime ?? Time.GetMillisecondsSinceEpoch());

            _inputList.Add(CreateInputItem(deviceKey, pt));
            _previousKey = e.Key;
        }

        private input_item CreateInputItem(DeviceKeys key, PointF origin)
        {
            var primaryC = Properties.RandomPrimaryColor ? ColorUtils.GenerateRandomColor() : Properties.PrimaryColor;
            var secondaryC = Properties.RandomSecondaryColor ? ColorUtils.GenerateRandomColor() : Properties.SecondaryColor;

            var animMix = new AnimationMix();

            switch (Properties.InteractiveEffect)
            {
                case InteractiveEffects.Wave:
                {
                    AnimationTrack wave = new AnimationTrack("Wave effect", 1.0f);
                    wave.SetFrame(0.0f,
                        new AnimationCircle(origin, 0, primaryC, Properties.EffectWidth)
                    );
                    wave.SetFrame(0.80f,
                        new AnimationCircle(origin, Effects.canvas_width * 0.80f, secondaryC, Properties.EffectWidth)
                    );
                    wave.SetFrame(1.00f,
                        new AnimationCircle(origin, Effects.canvas_width + (Properties.EffectWidth / 2), Color.FromArgb(0, secondaryC), Properties.EffectWidth)
                    );
                    animMix.AddTrack(wave);
                    break;
                }
                case InteractiveEffects.Wave_Rainbow:
                {
                    var rainbowWave = new AnimationTrack("Rainbow Wave", 1.0f);

                    rainbowWave.SetFrame(0.0f, new AnimationGradientCircle(origin, 0, new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip()).SetBrushType(EffectBrush.BrushType.Radial), Properties.EffectWidth));
                    rainbowWave.SetFrame(1.0f, new AnimationGradientCircle(origin, Effects.canvas_width + (Properties.EffectWidth / 2), new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip()).SetBrushType(EffectBrush.BrushType.Radial), Properties.EffectWidth));

                    animMix.AddTrack(rainbowWave);
                    break;
                }
                case InteractiveEffects.Wave_Filled:
                {
                    var wave = new AnimationTrack("Filled Wave effect", 1.0f);
                    wave.SetFrame(0.0f,
                        new AnimationFilledCircle(origin, 0, primaryC, Properties.EffectWidth)
                    );
                    wave.SetFrame(0.80f,
                        new AnimationFilledCircle(origin, Effects.canvas_width * 0.80f, secondaryC, Properties.EffectWidth)
                    );
                    wave.SetFrame(1.00f,
                        new AnimationFilledCircle(origin, Effects.canvas_width + (Properties.EffectWidth / 2), Color.FromArgb(0, secondaryC), Properties.EffectWidth)
                    );
                    animMix.AddTrack(wave);
                    break;
                }
                case InteractiveEffects.KeyPress:
                {
                    ColorSpectrum spec;
                    spec = new ColorSpectrum(primaryC, Color.FromArgb(0, secondaryC));
                    spec.SetColorAt(0.80f, secondaryC);

                    return new input_item(key, 0.0f, Properties.WaitOnKeyUp, spec);
                }
                case InteractiveEffects.ArrowFlow:
                {
                    var arrow = new AnimationTrack("Arrow Flow effect", 1.0f);
                    arrow.SetFrame(0.0f,
                        new AnimationLines(
                            new[] {
                                new AnimationLine(origin, origin, primaryC, Properties.EffectWidth),
                                new AnimationLine(origin, origin, primaryC, Properties.EffectWidth)
                            }
                        )
                    );
                    arrow.SetFrame(0.33f,
                        new AnimationLines(
                            new[] {
                                new AnimationLine(origin, new PointF(origin.X + Effects.canvas_width * 0.33f, origin.Y), ColorUtils.BlendColors(primaryC, secondaryC, 0.33D), Properties.EffectWidth),
                                new AnimationLine(origin, new PointF(origin.X - Effects.canvas_width * 0.33f, origin.Y), ColorUtils.BlendColors(primaryC, secondaryC, 0.33D), Properties.EffectWidth)
                            }
                        )
                    );
                    arrow.SetFrame(0.66f,
                        new AnimationLines(
                            new[] {
                                new AnimationLine(new PointF(origin.X + Effects.canvas_width * 0.33f, origin.Y), new PointF(origin.X + Effects.canvas_width * 0.66f, origin.Y), secondaryC, Properties.EffectWidth),
                                new AnimationLine(new PointF(origin.X - Effects.canvas_width * 0.33f, origin.Y), new PointF(origin.X - Effects.canvas_width * 0.66f, origin.Y), secondaryC, Properties.EffectWidth)
                            }
                        )
                    );
                    arrow.SetFrame(1.0f,
                        new AnimationLines(
                            new[] {
                                new AnimationLine(new PointF(origin.X + Effects.canvas_width * 0.66f, origin.Y), new PointF(origin.X + Effects.canvas_width, origin.Y), Color.FromArgb(0, secondaryC), Properties.EffectWidth),
                                new AnimationLine(new PointF(origin.X - Effects.canvas_width * 0.66f, origin.Y), new PointF(origin.X - Effects.canvas_width, origin.Y), Color.FromArgb(0, secondaryC), Properties.EffectWidth)
                            }
                        )
                    );
                    animMix.AddTrack(arrow);
                    break;
                }
            }

            return new input_item(key, 0.0f, Properties.WaitOnKeyUp, animMix);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            _previousTime = _currentTime;
            _currentTime = Time.GetMillisecondsSinceEpoch();
            foreach (var lengthPresses in _timeOfLastPress.ToList().Where(_keysToRemove))
            {
                _timeOfLastPress.TryRemove(lengthPresses.Key, out _);
            }

            if (_inputList.Count > 0)
            {
                _interactiveLayer.Clear();
            }
            foreach (var input in _inputList.ToArray())
            {
                if (input == null)
                    continue;

                try
                {
                    switch (input.type)
                    {
                        case input_item.input_type.Spectrum:
                        {
                            var transitionValue = input.progress / Effects.canvas_width;

                            if (transitionValue > 1.0f)
                                continue;

                            var color = input.spectrum.GetColorAt(transitionValue);

                            _interactiveLayer.Set(input.key, color);
                            break;
                        }
                        case input_item.input_type.AnimationMix:
                        {
                            var timeValue = input.progress / Effects.canvas_width;

                            if (timeValue > 1.0f)
                                continue;

                            input.animation.Draw(_interactiveLayer.GetGraphics(), timeValue);
                            break;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Interative layer exception, " + exc);
                }
            }

            for (var x = _inputList.Count - 1; x >= 0; x--)
            {
                try
                {
                    if (_inputList[x].progress > Effects.canvas_width)
                        _inputList.RemoveAt(x);
                    else
                    {
                        if (_inputList[x].waitOnKeyUp) continue;
                        var transAdded = (Properties.EffectSpeed * (GetDeltaTime() * 5.0f));
                        _inputList[x].progress += transAdded;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Interative layer exception, " + exc);
                }
            }

            return _interactiveLayer;
        }

        public override void Dispose()
        {
            _interactiveLayer.Dispose();
            base.Dispose();
        }
    }
}
