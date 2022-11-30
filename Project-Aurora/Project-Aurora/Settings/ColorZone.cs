using System;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aurora.Settings;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ColorZone
{
    public string Name { get; set; }
    public KeySequence Keysequence { get; set; }
    public Color Color { get; set; }
    public LayerEffects Effect { get; set; }
    public LayerEffectConfig EffectConfig { get; set; }

    public ColorZone()
    {
        Name = "New Zone";
        Keysequence = new KeySequence();
        Color = Color.Black;
        Effect = LayerEffects.None;
        EffectConfig = new LayerEffectConfig();
        GenerateRandomColor();
    }

    private void GenerateRandomColor()
    {
        Random r = new Random(Utils.Time.GetSeconds());
        Color = Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
    }

    public override string ToString()
    {
        return Name;
    }
}