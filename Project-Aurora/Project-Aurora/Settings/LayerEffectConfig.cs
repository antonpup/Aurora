﻿using Aurora.EffectsEngine;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Drawing;

namespace Aurora.Settings
{

    public class LayerEffectConfig
    {
        public Color primary;
        public Color secondary;
        public float speed;
        public float angle;
        public float gradient_size = 100.0f;
        public bool animation_reverse;
        public EffectBrush brush;

        // Added properties so we can use WPF to bind to the fields
        // Unfortunately we CANNOT use auto-properties (and keep the correct naming convention (upper-case first letter)) because
        // then there would be no lowercase name and the JSON would not be able to be deserialized properly, breaking all existing
        // profiles. >:( Little bit frustrating and verbose. Perhaps we can replace this next time there is a breaking change...
        [JsonIgnore] public Color Primary { get => primary; set => primary = value; }
        [JsonIgnore] public Color Secondary { get => secondary; set => secondary = value; }
        [JsonIgnore] public float AnimationSpeed { get => speed; set => speed = value; }
        [JsonIgnore] public float Angle { get => angle; set => angle = value; }
        [JsonIgnore] public float GradientSize { get => gradient_size; set => gradient_size = value; }
        [JsonIgnore] public bool AnimationReverse { get => animation_reverse; set => animation_reverse = value; }
        [JsonIgnore] public EffectBrush Brush { get => brush; set => brush = value; }


        [JsonIgnoreAttribute]
        public float shift_amount = 0.0f;
        [JsonIgnoreAttribute]
        public long last_effect_call = 0L;

        public LayerEffectConfig() : this(Color.Red, Color.Blue)
        {
        }

        public LayerEffectConfig(Color color) : this(color, color)
        {
        }

        public LayerEffectConfig(Color primary_color, Color secondary_color)
        {
            primary = primary_color;
            secondary = secondary_color;
            speed = 1.0f;
            angle = 0.0f;
            animation_reverse = false;
            brush = new LinearEffectBrush(new ColorBox.ColorSpectrum(primary_color, secondary_color));
        }

        public LayerEffectConfig(LayerEffectConfig otherConfig)
        {
            primary = otherConfig.primary;
            secondary = otherConfig.secondary;
            speed = otherConfig.speed;
            angle = otherConfig.angle;
            animation_reverse = otherConfig.animation_reverse;
            brush = otherConfig.brush;
        }
    }
}
