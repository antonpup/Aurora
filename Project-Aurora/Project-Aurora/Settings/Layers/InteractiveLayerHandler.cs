using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Profiles.Desktop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Controls;
using Aurora.Utils;
using Gma.System.MouseKeyHook;
using SharpDX.RawInput;

namespace Aurora.Settings.Layers
{
    public class input_item
    {
        public enum input_type
        {
            AnimationMix,
            Spectrum
        };

        public Devices.DeviceKeys key;
        public float progress;
        public bool waitOnKeyUp;
        public AnimationMix animation;
        public ColorSpectrum spectrum;
        public readonly input_type type;

        public input_item(Devices.DeviceKeys key, float progress, bool waitOnKeyUp, AnimationMix animation)
        {
            this.key = key;
            this.progress = progress;
            this.waitOnKeyUp = waitOnKeyUp;
            this.animation = animation;

            type = input_type.AnimationMix;
        }

        public input_item(Devices.DeviceKeys key, float progress, bool waitOnKeyUp, ColorSpectrum spectrum)
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

        public float? _EffectSpeed { get; set; }

        public bool? _WaitOnKeyUp { get; set; }

        [JsonIgnore]
        public bool WaitOnKeyUp { get { return Logic._WaitOnKeyUp ?? _WaitOnKeyUp ?? false; } }

        [JsonIgnore]
        public float EffectSpeed { get { return Logic._EffectSpeed ?? _EffectSpeed ?? 0.0f; } }

        public InteractiveEffects? _InteractiveEffect { get; set; }

        [JsonIgnore]
        public InteractiveEffects InteractiveEffect { get { return Logic._InteractiveEffect ?? _InteractiveEffect ?? InteractiveEffects.None; } }

        public int? _EffectWidth { get; set; }

        [JsonIgnore]
        public int EffectWidth { get { return Logic._EffectWidth ?? _EffectWidth ?? 0; } }

        public InteractiveLayerHandlerProperties() : base() { }

        public InteractiveLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._RandomPrimaryColor = false;
            this._RandomSecondaryColor = false;
            this._WaitOnKeyUp = false;
            this._EffectSpeed = 1.0f;
            this._InteractiveEffect = InteractiveEffects.None;
            this._EffectWidth = 2;
        }
    }

    public class InteractiveLayerHandler : LayerHandler<InteractiveLayerHandlerProperties>
    {
        private List<input_item> _input_list = new List<input_item>();
        private Keys previous_key = Keys.None;

        private long previoustime = 0;
        private long currenttime = 0;

        private input_item holdKeyInputItem;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        public InteractiveLayerHandler()
        {
            _ID = "Interactive";

            Global.InputEvents.KeyDown += InputEventsKeyDown;
            Global.InputEvents.KeyUp += InputEventsKeyUp;
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_InteractiveLayer(this);
        }

        private void InputEventsKeyUp(object sender, KeyboardInputEventArgs e)
        {
            if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                return; //This event wasn't used for at least 1 second

            Devices.DeviceKeys deviceKey = e.GetDeviceKey();
            if (deviceKey != Devices.DeviceKeys.NONE)
            {
                foreach (var input in _input_list.ToArray())
                {
                    if (input.waitOnKeyUp && input.key == deviceKey)
                        input.waitOnKeyUp = false;
                }
            }

            if (previous_key == e.Key)
                previous_key = Keys.None;
        }

        private Dictionary<Devices.DeviceKeys, long> TimeOfLastPress = new Dictionary<Devices.DeviceKeys, long>();
        private const long pressBuffer = 300L;

        private void InputEventsKeyDown(object sender, KeyboardInputEventArgs e)
        {
            if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                return; //This event wasn't used for at least 1 second


            if (previous_key == e.Key)
                return;

            long? currentTime = null;
            Devices.DeviceKeys device_key = e.GetDeviceKey();

            lock (TimeOfLastPress)
            {
                if (TimeOfLastPress.ContainsKey(device_key))
                {
                    if ((currentTime = Utils.Time.GetMillisecondsSinceEpoch()) - TimeOfLastPress[device_key] < pressBuffer)
                        return;
                    else
                        TimeOfLastPress.Remove(device_key);
                }
            }

            if (device_key != Devices.DeviceKeys.NONE && !Properties.Sequence.keys.Contains(device_key))
            {
                PointF pt = Effects.GetBitmappingFromDeviceKey(device_key).Center;
                if (pt != new PointF(0, 0))
                {
                    lock (TimeOfLastPress)
                        TimeOfLastPress.Add(device_key, currentTime ?? Utils.Time.GetMillisecondsSinceEpoch());

                    _input_list.Add(CreateInputItem(device_key, pt));
                    previous_key = e.Key;
                }
            }
        }

        private input_item CreateInputItem(Devices.DeviceKeys key, PointF origin)
        {
            Color primary_c = Properties.RandomPrimaryColor ? Utils.ColorUtils.GenerateRandomColor() : Properties.PrimaryColor;
            Color secondary_c = Properties.RandomSecondaryColor ? Utils.ColorUtils.GenerateRandomColor() : Properties.SecondaryColor;

            AnimationMix anim_mix = new AnimationMix();

            if (Properties.InteractiveEffect == InteractiveEffects.Wave)
            {
                AnimationTrack wave = new AnimationTrack("Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, primary_c, Properties.EffectWidth)
                    );
                wave.SetFrame(0.80f,
                    new AnimationCircle(origin, Effects.canvas_width * 0.80f, secondary_c, Properties.EffectWidth)
                    );
                wave.SetFrame(1.00f,
                    new AnimationCircle(origin, Effects.canvas_width + (Properties.EffectWidth / 2), Color.FromArgb(0, secondary_c), Properties.EffectWidth)
                    );
                anim_mix.AddTrack(wave);
            }
            else if (Properties.InteractiveEffect == InteractiveEffects.Wave_Rainbow)
            {
                AnimationTrack rainbowWave = new AnimationTrack("Rainbow Wave", 1.0f);

                rainbowWave.SetFrame(0.0f, new AnimationGradientCircle(origin, 0, new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip()).SetBrushType(EffectBrush.BrushType.Radial), Properties.EffectWidth));
                rainbowWave.SetFrame(1.0f, new AnimationGradientCircle(origin, Effects.canvas_width + (Properties.EffectWidth / 2), new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip()).SetBrushType(EffectBrush.BrushType.Radial), Properties.EffectWidth));

                anim_mix.AddTrack(rainbowWave);
            }
            else if (Properties.InteractiveEffect == InteractiveEffects.Wave_Filled)
            {
                AnimationTrack wave = new AnimationTrack("Filled Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationFilledCircle(origin, 0, primary_c, Properties.EffectWidth)
                    );
                wave.SetFrame(0.80f,
                    new AnimationFilledCircle(origin, Effects.canvas_width * 0.80f, secondary_c, Properties.EffectWidth)
                    );
                wave.SetFrame(1.00f,
                    new AnimationFilledCircle(origin, Effects.canvas_width + (Properties.EffectWidth / 2), Color.FromArgb(0, secondary_c), Properties.EffectWidth)
                    );
                anim_mix.AddTrack(wave);
            }
            else if (Properties.InteractiveEffect == InteractiveEffects.KeyPress)
            {
                ColorSpectrum spec = new ColorSpectrum(primary_c, secondary_c);
                spec = new ColorSpectrum(primary_c, Color.FromArgb(0, secondary_c));
                spec.SetColorAt(0.80f, secondary_c);

                return new input_item(key, 0.0f, Properties.WaitOnKeyUp, spec);
            }
            else if (Properties.InteractiveEffect == InteractiveEffects.ArrowFlow)
            {
                AnimationTrack arrow = new AnimationTrack("Arrow Flow effect", 1.0f);
                arrow.SetFrame(0.0f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(origin, origin, primary_c, Properties.EffectWidth),
                            new AnimationLine(origin, origin, primary_c, Properties.EffectWidth)
                        }
                        )
                    );
                arrow.SetFrame(0.33f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(origin, new PointF(origin.X + Effects.canvas_width * 0.33f, origin.Y), Utils.ColorUtils.BlendColors(primary_c, secondary_c, 0.33D), Properties.EffectWidth),
                            new AnimationLine(origin, new PointF(origin.X - Effects.canvas_width * 0.33f, origin.Y), Utils.ColorUtils.BlendColors(primary_c, secondary_c, 0.33D), Properties.EffectWidth)
                        }
                        )
                    );
                arrow.SetFrame(0.66f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(new PointF(origin.X + Effects.canvas_width * 0.33f, origin.Y), new PointF(origin.X + Effects.canvas_width * 0.66f, origin.Y), secondary_c, Properties.EffectWidth),
                            new AnimationLine(new PointF(origin.X - Effects.canvas_width * 0.33f, origin.Y), new PointF(origin.X - Effects.canvas_width * 0.66f, origin.Y), secondary_c, Properties.EffectWidth)
                        }
                        )
                    );
                arrow.SetFrame(1.0f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(new PointF(origin.X + Effects.canvas_width * 0.66f, origin.Y), new PointF(origin.X + Effects.canvas_width, origin.Y), Color.FromArgb(0, secondary_c), Properties.EffectWidth),
                            new AnimationLine(new PointF(origin.X - Effects.canvas_width * 0.66f, origin.Y), new PointF(origin.X - Effects.canvas_width, origin.Y), Color.FromArgb(0, secondary_c), Properties.EffectWidth)
                        }
                        )
                    );
                anim_mix.AddTrack(arrow);
            }

            return new input_item(key, 0.0f, Properties.WaitOnKeyUp, anim_mix);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();
            lock (TimeOfLastPress)
            {
                foreach (var lengthPresses in TimeOfLastPress.ToList())
                {
                    if (currenttime - lengthPresses.Value > pressBuffer)
                    {
                        TimeOfLastPress.Remove(lengthPresses.Key);
                    }
                }
            }
            EffectLayer interactive_layer = new EffectLayer("Interactive Effects");

            foreach (var input in _input_list.ToArray())
            {
                if (input == null)
                    continue;

                try
                {
                    if (input.type == input_item.input_type.Spectrum)
                    {
                        float transition_value = input.progress / Effects.canvas_width;

                        if (transition_value > 1.0f)
                            continue;

                        Color color = input.spectrum.GetColorAt(transition_value);

                        interactive_layer.Set(input.key, color);
                    }
                    else if (input.type == input_item.input_type.AnimationMix)
                    {
                        float time_value = input.progress / Effects.canvas_width;

                        if (time_value > 1.0f)
                            continue;

                        input.animation.Draw(interactive_layer.GetGraphics(), time_value);
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Interative layer exception, " + exc);
                }
            }

            for (int x = _input_list.Count - 1; x >= 0; x--)
            {
                try
                {
                    if (_input_list[x].progress > Effects.canvas_width)
                        _input_list.RemoveAt(x);
                    else
                    {
                        if(!_input_list[x].waitOnKeyUp)
                        {
                            float trans_added = (Properties.EffectSpeed * (getDeltaTime() * 5.0f));
                            _input_list[x].progress += trans_added;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Interative layer exception, " + exc);
                }
            }

            return interactive_layer;
        }
    }
}
