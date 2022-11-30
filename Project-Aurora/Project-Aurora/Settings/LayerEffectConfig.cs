using Aurora.EffectsEngine;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Drawing;
using Newtonsoft.Json.Serialization;

namespace Aurora.Settings;

public enum AnimationType
{
    [Description("No Animation")]
    None,
    [Description("Translate X and Y")]
    TranslateXy,
    [Description("(Radial only) Zoom in")]
    ZoomIn,
    [Description("(Radial only) Zoom out")]
    ZoomOut,
}

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class LayerEffectConfig
{
    public Color Primary { get; set; }
    public Color Secondary { get; set; }
    public float Speed { get; set; }
    public float Angle { get; set; }
    public float GradientSize { get; set; }
    public AnimationType AnimationType { get; set; }
    public bool AnimationReverse { get; set; }
    public EffectBrush Brush { get; set; }

    [JsonIgnore]
    public float ShiftAmount { get; set; }
    [JsonIgnore]
    public long LastEffectCall { get; set; }

    public LayerEffectConfig() : this(Color.Red, Color.Blue)
    {
    }

    public LayerEffectConfig(Color primaryColor, Color secondaryColor)
    {
        Primary = primaryColor;
        Secondary = secondaryColor;
        Speed = 1.0f;
        Angle = 0.0f;
        AnimationType = AnimationType.TranslateXy;
        AnimationReverse = false;
        Brush = new EffectBrush(
            new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(0, 0),
                new PointF(1, 0),
                Primary,
                Secondary
            )
            {
                InterpolationColors = new System.Drawing.Drawing2D.ColorBlend(2)
                {
                    Colors = new[] { Primary, Secondary },
                    Positions = new[] { 0.0f, 1.0f }
                }
            }
        );
    }
}