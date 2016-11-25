﻿using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Profiles.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

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

    public class InteractiveLayerHandler : LayerHandler
    {
        public Color PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
        public bool RandomPrimaryColor = false;
        public Color SecondaryColor = Utils.ColorUtils.GenerateRandomColor();
        public bool RandomSecondaryColor = false;
        public float EffectSpeed = 1.0f;
        public InteractiveEffects InteractiveEffect = InteractiveEffects.None;
        public bool TriggerOnMouseClick = false;
        public int EffectWidth = 2;

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
            _Control = new Control_InteractiveLayer(this);

            _Type = LayerType.Interactive;

            Global.input_subscriptions.KeyDown += Input_subscriptions_KeyDown;
            Global.input_subscriptions.KeyUp += Input_subscriptions_KeyUp;
            Global.input_subscriptions.MouseClick += Input_subscriptions_MouseClick;
        }

        private void Input_subscriptions_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!TriggerOnMouseClick)
                return;

            Devices.DeviceKeys device_key = Devices.DeviceKeys.Peripheral;

            if (device_key != Devices.DeviceKeys.NONE)
            {
                PointF pt = Effects.GetBitmappingFromDeviceKey(device_key).Center;
                if (pt != new PointF(0, 0))
                {
                    _input_list.Add(CreateInputItem(device_key, pt));
                }
            }
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

            //Global.logger.LogLine($"Keycode: { e.KeyCode }");

            Devices.DeviceKeys device_key = Utils.KeyUtils.GetDeviceKey(e.KeyCode);

            if (device_key != Devices.DeviceKeys.NONE)
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
            Color primary_c = RandomPrimaryColor ? Utils.ColorUtils.GenerateRandomColor() : PrimaryColor;
            Color secondary_c = RandomSecondaryColor ? Utils.ColorUtils.GenerateRandomColor() : SecondaryColor;

            AnimationMix anim_mix = new AnimationMix();

            if (InteractiveEffect == InteractiveEffects.Wave)
            {
                AnimationTrack wave = new AnimationTrack("Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, primary_c, EffectWidth)
                    );
                wave.SetFrame(0.80f,
                    new AnimationCircle(origin, Effects.canvas_width * 0.80f, secondary_c, EffectWidth)
                    );
                wave.SetFrame(1.00f,
                    new AnimationCircle(origin, Effects.canvas_width, Color.FromArgb(0, secondary_c), EffectWidth)
                    );
                anim_mix.AddTrack(wave);
            }
            else if (InteractiveEffect == InteractiveEffects.Wave_Filled)
            {
                AnimationTrack wave = new AnimationTrack("Filled Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationFilledCircle(origin, 0, primary_c, EffectWidth)
                    );
                wave.SetFrame(0.80f,
                    new AnimationFilledCircle(origin, Effects.canvas_width * 0.80f, secondary_c, EffectWidth)
                    );
                wave.SetFrame(1.00f,
                    new AnimationFilledCircle(origin, Effects.canvas_width, Color.FromArgb(0, secondary_c), EffectWidth)
                    );
                anim_mix.AddTrack(wave);
            }
            else if (InteractiveEffect == InteractiveEffects.KeyPress)
            {
                ColorSpectrum spec = new ColorSpectrum(primary_c, secondary_c);
                spec = new ColorSpectrum(primary_c, Color.FromArgb(0, secondary_c));
                spec.SetColorAt(0.80f, secondary_c);

                return new input_item(key, 0.0f, spec);
            }
            else if (InteractiveEffect == InteractiveEffects.ArrowFlow)
            {
                AnimationTrack arrow = new AnimationTrack("Arrow Flow effect", 1.0f);
                arrow.SetFrame(0.0f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(origin, origin, primary_c, EffectWidth),
                            new AnimationLine(origin, origin, primary_c, EffectWidth)
                        }
                        )
                    );
                arrow.SetFrame(0.33f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(origin, new PointF(origin.X + Effects.canvas_width * 0.33f, origin.Y), Utils.ColorUtils.BlendColors(primary_c, secondary_c, 0.33D), EffectWidth),
                            new AnimationLine(origin, new PointF(origin.X - Effects.canvas_width * 0.33f, origin.Y), Utils.ColorUtils.BlendColors(primary_c, secondary_c, 0.33D), EffectWidth)
                        }
                        )
                    );
                arrow.SetFrame(0.66f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(new PointF(origin.X + Effects.canvas_width * 0.33f, origin.Y), new PointF(origin.X + Effects.canvas_width * 0.66f, origin.Y), secondary_c, EffectWidth),
                            new AnimationLine(new PointF(origin.X - Effects.canvas_width * 0.33f, origin.Y), new PointF(origin.X - Effects.canvas_width * 0.66f, origin.Y), secondary_c, EffectWidth)
                        }
                        )
                    );
                arrow.SetFrame(1.0f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(new PointF(origin.X + Effects.canvas_width * 0.66f, origin.Y), new PointF(origin.X + Effects.canvas_width, origin.Y), Color.FromArgb(0, secondary_c), EffectWidth),
                            new AnimationLine(new PointF(origin.X - Effects.canvas_width * 0.66f, origin.Y), new PointF(origin.X - Effects.canvas_width, origin.Y), Color.FromArgb(0, secondary_c), EffectWidth)
                        }
                        )
                    );
                anim_mix.AddTrack(arrow);
            }

            return new input_item(key, 0.0f, anim_mix);
        }

        public override EffectLayer Render(GameState gamestate)
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
                        float trans_added = (EffectSpeed * (getDeltaTime() * 5.0f));
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
