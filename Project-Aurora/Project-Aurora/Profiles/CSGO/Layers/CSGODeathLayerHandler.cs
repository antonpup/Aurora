using System;
using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System.Drawing;
using System.Windows.Controls;
using Common.Utils;

namespace Aurora.Profiles.CSGO.Layers;

public class CSGODeathLayerHandlerProperties : LayerHandlerProperties2Color<CSGODeathLayerHandlerProperties>
{
    public Color? _DeathColor { get; set; }

    [JsonIgnore]
    public Color DeathColor => Logic._DeathColor ?? _DeathColor ?? Color.Empty;

    public int? _FadeOutAfter { get; set; }

    [JsonIgnore]
    public int FadeOutAfter => Logic._FadeOutAfter ?? _FadeOutAfter ?? 5;

    public CSGODeathLayerHandlerProperties()
    { }

    public CSGODeathLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();

        _DeathColor = Color.Red;
        _FadeOutAfter = 3;
    }

}

public class CSGODeathLayerHandler : LayerHandler<CSGODeathLayerHandlerProperties>
{
    private bool _isDead;
    private int _fadeAlpha = 255;
    private long _lastTimeMillis;
    private readonly SolidBrush _solidBrush = new(Color.Empty);

    public CSGODeathLayerHandler(): base("CSGO - Death Effect")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_CSGODeathLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameState_CSGO gameState) return EffectLayer.EmptyLayer;
        var deathColor = Properties.DeathColor;

        // Confirm if CS:GO Player is correct
        if (!gameState.Provider.SteamID.Equals(gameState.Player.SteamID)) return EffectLayer.EmptyLayer;

        // Are they dead?
        if (!_isDead && gameState.Player.State.Health <= 0 && gameState.Previously.Player.State.Health > 0)
        {
            _isDead = true;
            _lastTimeMillis = Utils.Time.GetMillisecondsSinceEpoch();
            _fadeAlpha = 255;
        }

        if (!_isDead)
        {
            return EffectLayer.EmptyLayer;
        }

        var fadeAlpha = GetFadeAlpha();
        if (fadeAlpha <= 0)
        {
            _isDead = false;
            return EffectLayer.EmptyLayer;
        }

        _solidBrush.Color = CommonColorUtils.FastColor(deathColor.R, deathColor.G, deathColor.B, (byte)fadeAlpha);
        EffectLayer.Fill(_solidBrush);
        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        ((Control_CSGODeathLayer)Control).SetProfile(profile);
        base.SetApplication(profile);
    }

    private int GetFadeAlpha()
    {
        var t = Utils.Time.GetMillisecondsSinceEpoch() - _lastTimeMillis;
        _lastTimeMillis = Utils.Time.GetMillisecondsSinceEpoch();
        _fadeAlpha -= (int)(t / 10);
        _fadeAlpha = Math.Min(_fadeAlpha, 255);
        return _fadeAlpha;
    }
}