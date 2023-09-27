
using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.Utils;

namespace Aurora.Profiles.CSGO.Layers;

public class CSGOBackgroundLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBackgroundLayerHandlerProperties>
{
    public Color? _DefaultColor { get; set; }

    [JsonIgnore]
    public Color DefaultColor => Logic._DefaultColor ?? _DefaultColor ?? Color.Empty;

    public Color? _CTColor { get; set; }

    [JsonIgnore]
    public Color CTColor => Logic._CTColor ?? _CTColor ?? Color.Empty;

    public Color? _TColor { get; set; }

    [JsonIgnore]
    public Color TColor => Logic._TColor ?? _TColor ?? Color.Empty;

    public bool? _DimEnabled { get; set; }

    [JsonIgnore]
    public bool DimEnabled => Logic._DimEnabled ?? _DimEnabled ?? false;

    public double? _DimDelay { get; set; }

    [JsonIgnore]
    public double DimDelay => Logic._DimDelay ?? _DimDelay ?? 0.0;

    public int? _DimAmount { get; set; }

    [JsonIgnore]
    public int DimAmount => Logic._DimAmount ?? _DimAmount ?? 100;

    public CSGOBackgroundLayerHandlerProperties() : base() { }

    public CSGOBackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();

        _DefaultColor = Color.FromArgb(158, 205, 255);
        _CTColor = Color.FromArgb(33, 155, 221);
        _TColor = Color.FromArgb(221, 99, 33);
        _DimEnabled = true;
        _DimDelay = 15;
        _DimAmount = 20;
    }

}

public class CSGOBackgroundLayerHandler : LayerHandler<CSGOBackgroundLayerHandlerProperties>
{
    private bool _isDimming;
    private double _dimValue = 100.0;
    private long _dimBgAt = 15;
    private readonly SolidBrush _solidBrush = new(Color.Empty);

    public CSGOBackgroundLayerHandler(): base("CSGO - Background")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_CSGOBackgroundLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameState_CSGO csgostate) return EffectLayer.EmptyLayer;

        var inGame = csgostate.Previously.Player.State.Health is > -1 and < 100
                     || (csgostate.Round.WinTeam == RoundWinTeam.Undefined && csgostate.Previously.Round.WinTeam != RoundWinTeam.Undefined);
        if (csgostate.Player.State.Health == 100 && inGame && csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
        {
            _isDimming = false;
            _dimBgAt = Utils.Time.GetMillisecondsSinceEpoch() +  (long)Properties.DimDelay * 1000;
            _dimValue = 100.0;
        }

        var bgColor = csgostate.Player.Team switch
        {
            PlayerTeam.T => Properties.TColor,
            PlayerTeam.CT => Properties.CTColor,
            _ => Properties.DefaultColor
        };

        if (csgostate.Player.Team is PlayerTeam.CT or PlayerTeam.T)
        {
            if (_dimBgAt <= Time.GetMillisecondsSinceEpoch() || csgostate.Player.State.Health == 0)
            {
                _isDimming = true;
                bgColor = ColorUtils.MultiplyColorByScalar(bgColor, GetDimmingValue() / 100);
            }
            else
            {
                _isDimming = false;
                _dimValue = 100.0;
            }
        }

        if (_solidBrush.Color == bgColor) return EffectLayer;
        _solidBrush.Color = bgColor;
        EffectLayer.Fill(_solidBrush);

        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        (Control as Control_CSGOBackgroundLayer)?.SetProfile(profile);
        base.SetApplication(profile);
    }

    private double GetDimmingValue()
    {
        if (!_isDimming || !Properties.DimEnabled) return _dimValue = 100.0;
        _dimValue -= 2.0;
        return _dimValue = _dimValue < Math.Abs(Properties.DimAmount - 100) ? Math.Abs(Properties.DimAmount - 100) : _dimValue;

    }
}