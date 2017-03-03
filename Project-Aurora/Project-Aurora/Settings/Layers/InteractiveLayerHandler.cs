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
        public AnimationMix animation;
        public ColorSpectrum spectrum;
        public readonly input_type type;

        public input_item(Devices.DeviceKeys key, float progress, AnimationMix animation)
        {
            this.key = key;
            this.progress = progress;
            this.animation = animation;

            type = input_type.AnimationMix;
        }

        public input_item(Devices.DeviceKeys key, float progress, ColorSpectrum spectrum)
        {
            this.key = key;
            this.progress = progress;
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

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        public InteractiveLayerHandler()
        {
            _ID = "Interactive";

            Global.input_subscriptions.KeyDown += Input_subscriptions_KeyDown;
            Global.input_subscriptions.KeyUp += Input_subscriptions_KeyUp;
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_InteractiveLayer(this);
        }

        private void Input_subscriptions_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                return; //This event wasn't used for at least 1 second

            if (previous_key == e.KeyCode)
                previous_key = Keys.None;
        }

        private void Input_subscriptions_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                return; //This event wasn't used for at least 1 second

            if (previous_key == e.KeyCode)
                return;

            Devices.DeviceKeys device_key = Utils.KeyUtils.GetDeviceKey(e.KeyCode);

            if (device_key != Devices.DeviceKeys.NONE && !Properties.Sequence.keys.Contains(device_key))
            {
                PointF pt = Effects.GetBitmappingFromDeviceKey(device_key).Center;
                if (pt != new PointF(0, 0))
                {
                    _input_list.Add(CreateInputItem(device_key, pt));
                    previous_key = e.KeyCode;
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
                    new AnimationCircle(origin, Effects.canvas_width, Color.FromArgb(0, secondary_c), Properties.EffectWidth)
                    );
                anim_mix.AddTrack(wave);
            }
            else if (Properties.InteractiveEffect == InteractiveEffects.Wave_Rainbow)
            {

                float dT = (float)(Math.Sqrt((double)Properties.EffectWidth - 0.7) * Math.Sqrt(12.0)) * 0.0065f;

                AnimationTrack wave_red = new AnimationTrack("Wave effect (red)", 1.00f - dT * 6);
                AnimationTrack wave_orange = new AnimationTrack("Wave effect (orange)", 1.00f - dT * 5);
                AnimationTrack wave_yellow = new AnimationTrack("Wave effect (yellow)", 1.00f - dT * 4);
                AnimationTrack wave_green = new AnimationTrack("Wave effect (green)", 1.00f - dT * 3);
                AnimationTrack wave_lightblue = new AnimationTrack("Wave effect (light blue)", 1.00f - dT * 2);
                AnimationTrack wave_blue = new AnimationTrack("Wave effect (blue)", 1.00f - dT * 1);
                AnimationTrack wave_purple = new AnimationTrack("Wave effect (purple)", 1.00f);

                wave_red.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, Color.Red, Properties.EffectWidth)
                    );
                wave_red.SetFrame(1.00f - dT * 6,
                    new AnimationCircle(origin, Effects.canvas_width, Color.Red, Properties.EffectWidth)
                    );

                wave_orange.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, Color.Orange, Properties.EffectWidth)
                    );
                wave_orange.SetFrame(0.0f + dT,
                    new AnimationCircle(origin, 0, Color.Orange, Properties.EffectWidth)
                    );
                wave_orange.SetFrame(1.00f - dT * 5,
                    new AnimationCircle(origin, Effects.canvas_width, Color.Orange, Properties.EffectWidth)
                    );

                wave_yellow.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, Color.Yellow, Properties.EffectWidth)
                    );
                wave_yellow.SetFrame(0.0f + dT * 2,
                    new AnimationCircle(origin, 0, Color.Yellow, Properties.EffectWidth)
                    );
                wave_yellow.SetFrame(1.00f - dT * 4,
                    new AnimationCircle(origin, Effects.canvas_width, Color.Yellow, Properties.EffectWidth)
                    );

                wave_green.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, Color.Green, Properties.EffectWidth)
                    );
                wave_green.SetFrame(0.0f + dT * 3,
                    new AnimationCircle(origin, 0, Color.Green, Properties.EffectWidth)
                    );
                wave_green.SetFrame(1.00f - dT * 3,
                    new AnimationCircle(origin, Effects.canvas_width, Color.Green, Properties.EffectWidth)
                    );

                wave_lightblue.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, Color.Blue, Properties.EffectWidth)
                    );
                wave_lightblue.SetFrame(0.0f + dT * 4,
                    new AnimationCircle(origin, 0, Color.Blue, Properties.EffectWidth)
                    );
                wave_lightblue.SetFrame(1.00f - dT * 2,
                    new AnimationCircle(origin, Effects.canvas_width, Color.Blue, Properties.EffectWidth)
                    );

                wave_blue.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, Color.DarkBlue, Properties.EffectWidth)
                    );
                wave_blue.SetFrame(0.0f + dT * 5,
                    new AnimationCircle(origin, 0, Color.DarkBlue, Properties.EffectWidth)
                    );
                wave_blue.SetFrame(1.00f - dT,
                    new AnimationCircle(origin, Effects.canvas_width, Color.DarkBlue, Properties.EffectWidth)
                    );

                wave_purple.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, Color.Purple, Properties.EffectWidth)
                    );
                wave_purple.SetFrame(0.0f + dT * 6,
                    new AnimationCircle(origin, 0, Color.Purple, Properties.EffectWidth)
                    );
                wave_purple.SetFrame(1.00f,
                    new AnimationCircle(origin, Effects.canvas_width, Color.Purple, Properties.EffectWidth)
                    );

                anim_mix.AddTrack(wave_red);
                anim_mix.AddTrack(wave_orange);
                anim_mix.AddTrack(wave_yellow);
                anim_mix.AddTrack(wave_green);
                anim_mix.AddTrack(wave_lightblue);
                anim_mix.AddTrack(wave_blue);
                anim_mix.AddTrack(wave_purple);
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
                    new AnimationFilledCircle(origin, Effects.canvas_width, Color.FromArgb(0, secondary_c), Properties.EffectWidth)
                    );
                anim_mix.AddTrack(wave);
            }
            else if (Properties.InteractiveEffect == InteractiveEffects.KeyPress)
            {
                ColorSpectrum spec = new ColorSpectrum(primary_c, secondary_c);
                spec = new ColorSpectrum(primary_c, Color.FromArgb(0, secondary_c));
                spec.SetColorAt(0.80f, secondary_c);

                return new input_item(key, 0.0f, spec);
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

            return new input_item(key, 0.0f, anim_mix);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

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
                    Global.logger.LogLine("Interative layer exception, " + exc, Logging_Level.Error);
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
                        float trans_added = (Properties.EffectSpeed * (getDeltaTime() * 5.0f));
                        _input_list[x].progress += trans_added;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine("Interative layer exception, " + exc, Logging_Level.Error);
                }
            }

            return interactive_layer;
        }
    }
}
