using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles.Witcher3.GSI;
using Aurora.Profiles.Witcher3.GSI.Nodes;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Witcher3.Layers;

public class Witcher3BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<Witcher3BackgroundLayerHandlerProperties>
{
    public Color? _DefaultColor { get; set; }

    public Color DefaultColor => Logic._DefaultColor ?? _DefaultColor ?? Color.Empty;

    public Color? _QuenColor { get; set; }

    public Color QuenColor => Logic._QuenColor ?? _QuenColor ?? Color.Empty;

    public Color? _IgniColor { get; set; }

    public Color IgniColor => Logic._IgniColor ?? _IgniColor ?? Color.Empty;

    public Color? _AardColor { get; set; }

    public Color AardColor => Logic._AardColor ?? _AardColor ?? Color.Empty;

    public Color? _YrdenColor { get; set; }

    public Color YrdenColor => Logic._YrdenColor ?? _YrdenColor ?? Color.Empty;

    public Color? _AxiiColor { get; set; }

    public Color AxiiColor => Logic._AxiiColor ?? _AxiiColor ?? Color.Empty;


    public Witcher3BackgroundLayerHandlerProperties()
    { }

    public Witcher3BackgroundLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();
        _DefaultColor = Color.Gray;
        _QuenColor = Color.Yellow;
        _IgniColor = Color.Red;
        _AardColor = Color.Blue;
        _YrdenColor = Color.Purple;
        _AxiiColor = Color.Green;
    }
}

public class Witcher3BackgroundLayerHandler : LayerHandler<Witcher3BackgroundLayerHandlerProperties>
{
    private readonly SolidBrush _currentColor = new(Color.White);
    
    public Witcher3BackgroundLayerHandler() : base("Witcher3 - Background")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_Witcher3BackgroundLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameState_Witcher3 witcher3State) return EffectLayer.EmptyLayer;

        Color bgColor = witcher3State.Player.ActiveSign switch
        {
            WitcherSign.Aard => Properties.AardColor,
            WitcherSign.Igni => Properties.IgniColor,
            WitcherSign.Quen => Properties.QuenColor,
            WitcherSign.Yrden => Properties.YrdenColor,
            WitcherSign.Axii => Properties.AxiiColor,
            WitcherSign.None => Properties.DefaultColor,
            _ => Properties.DefaultColor
        };

        if (_currentColor.Color == bgColor) return EffectLayer;
        _currentColor.Color = bgColor;
        EffectLayer.FillOver(bgColor);

        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        (Control as Control_Witcher3BackgroundLayer).SetProfile(profile);
        base.SetApplication(profile);
    }
}