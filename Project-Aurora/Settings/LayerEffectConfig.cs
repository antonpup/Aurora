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
        public bool animation_fade;
        public bool respect_cz_dimensions;
        public bool retain_square_ratio;
        public EffectBrush brush;

        [JsonIgnoreAttribute]
        public float shift_amount = 0.0f;
        [JsonIgnoreAttribute]
        public long last_effect_call = 0L;

        public LayerEffectConfig()
        {
            primary = Color.Red;
            secondary = Color.Blue;
            speed = 1.0f;
            angle = 0.0f;
            animation_type = AnimationType.Translate_XY;
            animation_reverse = false;
            respect_cz_dimensions = true;
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
