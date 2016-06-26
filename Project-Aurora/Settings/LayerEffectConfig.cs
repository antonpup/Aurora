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
        [Description("Translate on X")]
        Translate_X,
        [Description("Translate on Y")]
        Translate_Y,
        [Description("Translate Both")]
        Translate_XY
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

        public LayerEffectConfig()
        {
            primary = Color.Red;
            secondary = Color.Blue;
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
