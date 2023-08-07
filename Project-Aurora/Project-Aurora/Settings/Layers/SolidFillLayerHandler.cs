using System;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Settings.Layers.Controls;
using Lombok.NET;

namespace Aurora.Settings.Layers;

[NoArgsConstructor]
public partial class SolidFillLayerHandlerProperties : LayerHandlerProperties<SolidFillLayerHandlerProperties>
{
    public SolidFillLayerHandlerProperties(bool arg = false) : base(arg)
    {
    }

    public override void Default()
    {
        base.Default();
        _PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
    }
}

[Overrides.LogicOverrideIgnoreProperty("_Sequence")]
public sealed class SolidFillLayerHandler : LayerHandler<SolidFillLayerHandlerProperties>
{
    private readonly SolidBrush _solidBrush = new(Color.Transparent);

    public SolidFillLayerHandler() : base("Solid Fill Layer")
    {
        Effects.CanvasChanged += EffectsOnCanvasChanged;
    }

    protected override UserControl CreateControl()
    {
        return new Control_SolidFillLayer(this);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        return EffectLayer;
    }

    private void EffectsOnCanvasChanged(object? sender, EventArgs e)
    {
        EffectLayer.Fill(_solidBrush);
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        _solidBrush.Color = Properties.PrimaryColor;
        EffectLayer.Fill(_solidBrush);
    }

    public override void Dispose()
    {
        base.Dispose();

        Effects.CanvasChanged -= EffectsOnCanvasChanged;
    }
}