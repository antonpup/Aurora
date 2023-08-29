using System;
using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Profiles.CSGO.Layers;

public class CSGOBurningLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBurningLayerHandlerProperties>
{
    public Color? _BurningColor { get; set; }

    [JsonIgnore]
    public Color BurningColor => Logic._BurningColor ?? _BurningColor ?? Color.Empty;

    public bool? _Animated { get; set; }

    [JsonIgnore]
    public bool Animated => Logic._Animated ?? _Animated ?? false;

    public CSGOBurningLayerHandlerProperties()
    { }

    public CSGOBurningLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();

        _BurningColor = Color.FromArgb(255, 70, 0);
        _Animated = true;
    }
}

public class CSGOBurningLayerHandler : LayerHandler<CSGOBurningLayerHandlerProperties>
{
    private readonly Random _randomizer = new();
    private readonly SolidBrush _solidBrush = new(Color.Empty);

    public CSGOBurningLayerHandler(): base("CSGO - Burning")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_CSGOBurningLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameState_CSGO csgostate) return EffectLayer.EmptyLayer;

        //Update Burning

        if (csgostate.Player.State.Burning <= 0) return EffectLayer.EmptyLayer;
        var burnColor = Properties.BurningColor;

        if (Properties.Animated)
        {
            var redAdjusted = (int)(burnColor.R + (Math.Cos((Time.GetMillisecondsSinceEpoch() + _randomizer.Next(75)) / 75.0) * 0.15 * 255));

            byte red = redAdjusted switch
            {
                > 255 => 255,
                < 0 => 0,
                _ => (byte) redAdjusted
            };

            var greenAdjusted = (int)(burnColor.G + (Math.Sin((Time.GetMillisecondsSinceEpoch() + _randomizer.Next(150)) / 75.0) * 0.15 * 255));

            byte green = greenAdjusted switch
            {
                > 255 => 255,
                < 0 => 0,
                _ => (byte) greenAdjusted
            };

            var blueAdjusted = (int)(burnColor.B + (Math.Cos((Time.GetMillisecondsSinceEpoch() + _randomizer.Next(225)) / 75.0) * 0.15 * 255));

            byte blue = blueAdjusted switch
            {
                > 255 => 255,
                < 0 => 0,
                _ => (byte) blueAdjusted
            };

            burnColor = Color.FromArgb(csgostate.Player.State.Burning, red, green, blue);
        }

        if (_solidBrush.Color == burnColor) return EffectLayer;
        _solidBrush.Color = burnColor;
        EffectLayer.Fill(_solidBrush);
        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        (Control as Control_CSGOBurningLayer).SetProfile(profile);
        base.SetApplication(profile);
    }
}