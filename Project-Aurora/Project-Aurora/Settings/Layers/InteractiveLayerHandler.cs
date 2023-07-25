using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Modules.Inputs;
using Aurora.Profiles;
using Aurora.Profiles.Desktop;
using Aurora.Settings.Layers.Controls;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
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

        [JsonIgnore]
        private DeviceKeys? _mouseEffectKey;

        public DeviceKeys MouseEffectKey
        {
            get => (Logic._mouseEffectKey ?? _mouseEffectKey) ?? DeviceKeys.NONE;
            set => SetFieldAndRaisePropertyChanged(out _mouseEffectKey, value);
        }

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
        private readonly Func<KeyValuePair<DeviceKeys, long>, bool> _keysToRemove;
        private readonly List<input_item> _inputList = new();
        
        private DeviceKeys _previousKey = DeviceKeys.NONE;
        private long _previousTime;
        private long _currentTime;

        public InteractiveLayerHandler(): base("Interactive Effects")
        {
            Global.InputEvents.KeyDown += InputEventsKeyDown;
            Global.InputEvents.KeyUp += InputEventsKeyUp;
            Global.InputEvents.MouseButtonDown += MouseKeyDown;
            Global.InputEvents.MouseButtonUp += MouseKeyUp;
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

        private void MouseKeyUp(object sender, EventArgs mouseInputEventArgs)
        {
            if (Properties.MouseEffectKey == DeviceKeys.NONE)
                return;

            DeviceKeyUp(Properties.MouseEffectKey);
        }

        private void InputEventsKeyUp(object sender, KeyEvent e)
        {
            var deviceKey = e.GetDeviceKey();
            DeviceKeyUp(deviceKey);
        }

        private void DeviceKeyUp(DeviceKeys deviceKey)
        {
            if (Time.GetMillisecondsSinceEpoch() - _previousTime > 1000L)
                return; //This event wasn't used for at least 1 second

            if (deviceKey != DeviceKeys.NONE)
            {
                foreach (var input in _inputList.ToArray())
                {
                    if (input.waitOnKeyUp && input.key == deviceKey)
                        input.waitOnKeyUp = false;
                }
            }

            if (_previousKey == deviceKey)
                _previousKey = DeviceKeys.NONE;
        }

        private readonly ConcurrentDictionary<DeviceKeys, long> _timeOfLastPress = new();
        private const long PressBuffer = 300L;

        private void MouseKeyDown(object sender, EventArgs mouseInputEventArgs)
        {
            if (Properties.MouseEffectKey == DeviceKeys.NONE)
                return;

            DeviceKeyDown(Properties.MouseEffectKey);
        }

        private void InputEventsKeyDown(object sender, KeyEvent e)
        {
            var deviceKey = e.GetDeviceKey();

            DeviceKeyDown(deviceKey);
        }

        private void DeviceKeyDown(DeviceKeys deviceKey)
        {
            if (Time.GetMillisecondsSinceEpoch() - _previousTime > 1000L)
                return; //This event wasn't used for at least 1 second

            if (_previousKey == deviceKey)
                return;

            long? currentTime = null;

            if (_timeOfLastPress.ContainsKey(deviceKey))
            {
                if (Properties.UsePressBuffer &&
                    (currentTime = Time.GetMillisecondsSinceEpoch()) - _timeOfLastPress[deviceKey] < PressBuffer)
                    return;
                _timeOfLastPress.TryRemove(deviceKey, out _);
            }

            if (deviceKey == DeviceKeys.NONE || Properties.Sequence.Keys.Contains(deviceKey)) return;
            var pt = Effects.GetBitmappingFromDeviceKey(deviceKey).Center;
            if (pt.IsEmpty) return;

            _timeOfLastPress.TryAdd(deviceKey, currentTime ?? Time.GetMillisecondsSinceEpoch());

            _inputList.Add(CreateInputItem(deviceKey, pt));
            _previousKey = deviceKey;
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
                        new AnimationCircle(origin, Effects.CanvasWidth * 0.80f, secondaryC, Properties.EffectWidth)
                    );
                    wave.SetFrame(1.00f,
                        new AnimationCircle(origin, Effects.CanvasWidth + (Properties.EffectWidth / 2), Color.FromArgb(0, secondaryC), Properties.EffectWidth)
                    );
                    animMix.AddTrack(wave);
                    break;
                }
                case InteractiveEffects.Wave_Rainbow:
                {
                    var rainbowWave = new AnimationTrack("Rainbow Wave", 1.0f);

                    rainbowWave.SetFrame(0.0f, new AnimationGradientCircle(origin, 0, new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip()).SetBrushType(EffectBrush.BrushType.Radial), Properties.EffectWidth));
                    rainbowWave.SetFrame(1.0f, new AnimationGradientCircle(origin, Effects.CanvasWidth + (Properties.EffectWidth / 2), new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip()).SetBrushType(EffectBrush.BrushType.Radial), Properties.EffectWidth));

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
                        new AnimationFilledCircle(origin, Effects.CanvasWidth * 0.80f, secondaryC, Properties.EffectWidth)
                    );
                    wave.SetFrame(1.00f,
                        new AnimationFilledCircle(origin, Effects.CanvasWidth + (Properties.EffectWidth / 2), Color.FromArgb(0, secondaryC), Properties.EffectWidth)
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
                                new AnimationLine(origin, new PointF(origin.X + Effects.CanvasWidth * 0.33f, origin.Y), ColorUtils.BlendColors(primaryC, secondaryC, 0.33D), Properties.EffectWidth),
                                new AnimationLine(origin, new PointF(origin.X - Effects.CanvasWidth * 0.33f, origin.Y), ColorUtils.BlendColors(primaryC, secondaryC, 0.33D), Properties.EffectWidth)
                            }
                        )
                    );
                    arrow.SetFrame(0.66f,
                        new AnimationLines(
                            new[] {
                                new AnimationLine(new PointF(origin.X + Effects.CanvasWidth * 0.33f, origin.Y), new PointF(origin.X + Effects.CanvasWidth * 0.66f, origin.Y), secondaryC, Properties.EffectWidth),
                                new AnimationLine(new PointF(origin.X - Effects.CanvasWidth * 0.33f, origin.Y), new PointF(origin.X - Effects.CanvasWidth * 0.66f, origin.Y), secondaryC, Properties.EffectWidth)
                            }
                        )
                    );
                    arrow.SetFrame(1.0f,
                        new AnimationLines(
                            new[] {
                                new AnimationLine(new PointF(origin.X + Effects.CanvasWidth * 0.66f, origin.Y), new PointF(origin.X + Effects.CanvasWidth, origin.Y), Color.FromArgb(0, secondaryC), Properties.EffectWidth),
                                new AnimationLine(new PointF(origin.X - Effects.CanvasWidth * 0.66f, origin.Y), new PointF(origin.X - Effects.CanvasWidth, origin.Y), Color.FromArgb(0, secondaryC), Properties.EffectWidth)
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
            foreach (var lengthPresses in _timeOfLastPress.Where(_keysToRemove))
            {
                _timeOfLastPress.TryRemove(lengthPresses.Key, out _);
            }

            if (_inputList.Count == 0)
            {
                return EffectLayer.EmptyLayer;
            }

            EffectLayer.Clear();
            foreach (var input in _inputList.ToArray())
            {
                if (input == null)
                    continue;

                try
                {
                    switch (input.type)
                    {
                        case input_item.input_type.Spectrum:
                        default:
                        {
                            var transitionValue = input.progress / Effects.CanvasWidth;

                            if (transitionValue > 1.0f)
                                continue;

                            var color = input.spectrum.GetColorAt(transitionValue);

                            EffectLayer.Set(input.key, color);
                            break;
                        }
                        case input_item.input_type.AnimationMix:
                        {
                            var timeValue = input.progress / Effects.CanvasWidth;

                            if (timeValue > 1.0f)
                                continue;

                            input.animation.Draw(EffectLayer.GetGraphics(), timeValue);
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
                    if (_inputList[x].progress > Effects.CanvasWidth)
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

            return EffectLayer;
        }

        public override void Dispose()
        {
            EffectLayer.Dispose();
            base.Dispose();
        }
    }
}
