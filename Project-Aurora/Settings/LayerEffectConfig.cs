using Aurora.EffectsEngine;
using Newtonsoft.Json;
using System.Drawing;

namespace Aurora.Settings
{
    public class LayerEffectConfig
    {
        public Color primary;
        public Color secondary;
        public float speed;
        public float angle;
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
            brush = otherConfig.brush;
        }
    }
}
