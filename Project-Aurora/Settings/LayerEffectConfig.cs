using Aurora.EffectsEngine;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Drawing;

namespace Aurora.Settings
{
    public enum AnimationType
    {
        [Description("No Animation")]
        None,
        [Description("Translate X and Y")]
        Translate_XY,
        [Description("(Radial only) Zoom in")]
        Zoom_in,
        [Description("(Radial only) Zoom out")]
        Zoom_out,
    };

    public class LayerEffectConfig
    {
        public Color primary;
        public Color secondary;
        public float speed;
        public float angle;
        public AnimationType animation_type;
        public bool animation_reverse;
        public EffectBrush brush;

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
            animation_type = AnimationType.Translate_XY;
            animation_reverse = false;
            brush = new EffectBrush(
                new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(1, 0),
                    primary,
                    secondary
                    )
                {
                    InterpolationColors = new System.Drawing.Drawing2D.ColorBlend(2)
                    {
                        Colors = new Color[] { primary, secondary },
                        Positions = new float[] { 0.0f, 1.0f }
                    }
                }
                );
        }

        public LayerEffectConfig SetAnimationSpeed(float speed)
        {
            this.speed = speed;

            return this;
        }

        public LayerEffectConfig SetAnimationAngle(float angle)
        {
            this.angle = angle;

            return this;
        }

        public LayerEffectConfig SetAnimationType(AnimationType type)
        {
            animation_type = type;

            return this;
        }

        public LayerEffectConfig SetAnimationDirection(bool reverse)
        {
            animation_reverse = reverse;

            return this;
        }

        public LayerEffectConfig(LayerEffectConfig otherConfig)
        {
            primary = otherConfig.primary;
            secondary = otherConfig.secondary;
            speed = otherConfig.speed;
            angle = otherConfig.angle;
            animation_type = otherConfig.animation_type;
            animation_reverse = otherConfig.animation_reverse;
            brush = otherConfig.brush;
        }
    }
}
