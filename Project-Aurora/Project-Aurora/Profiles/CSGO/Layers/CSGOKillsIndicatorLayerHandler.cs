using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Common.Devices;
using Newtonsoft.Json;

namespace Aurora.Profiles.CSGO.Layers;

public class CSGOKillIndicatorLayerHandlerProperties : LayerHandlerProperties2Color<CSGOKillIndicatorLayerHandlerProperties>
{
    public Color? _RegularKillColor { get; set; }

    [JsonIgnore]
    public Color RegularKillColor => Logic._RegularKillColor ?? _RegularKillColor ?? Color.Empty;

    public Color? _HeadshotKillColor { get; set; }

    [JsonIgnore]
    public Color HeadshotKillColor => Logic._HeadshotKillColor ?? _HeadshotKillColor ?? Color.Empty;

    public CSGOKillIndicatorLayerHandlerProperties()
    { }

    public CSGOKillIndicatorLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();

        _Sequence = new KeySequence(new[] { DeviceKeys.G1, DeviceKeys.G2, DeviceKeys.G3, DeviceKeys.G4, DeviceKeys.G5 });
        _RegularKillColor = Color.FromArgb(255, 204, 0);
        _HeadshotKillColor = Color.FromArgb(255, 0, 0);
    }

}

public class CSGOKillIndicatorLayerHandler : LayerHandler<CSGOKillIndicatorLayerHandlerProperties>
{
    private enum RoundKillType
    {
        None,
        Regular,
        Headshot
    }

    private List<RoundKillType> roundKills = new()
    {
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
    };
    private int _lastCountedKill;

    public CSGOKillIndicatorLayerHandler(): base("CSGO - Kills Indicator")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_CSGOKillIndicatorLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameState_CSGO csgostate) return EffectLayer.EmptyLayer;

        if (_lastCountedKill != csgostate.Player.State.RoundKills)
        {
            CalculateKills(csgostate);
        }

        if (!csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID)) return EffectLayer;
        for (var pos = 0; pos < Properties.Sequence.Keys.Count; pos++)
        {
            if (pos < roundKills.Count)
            {
                switch (roundKills[pos])
                {
                    case RoundKillType.Regular:
                        EffectLayer.Set(Properties.Sequence.Keys[pos], Properties.RegularKillColor);
                        break;
                    case RoundKillType.Headshot:
                        EffectLayer.Set(Properties.Sequence.Keys[pos], Properties.HeadshotKillColor);
                        break;
                    case RoundKillType.None:
                        EffectLayer.Set(Properties.Sequence.Keys[pos], Color.Empty);
                        break;
                }
            }
        }

        return EffectLayer;
    }

    private void CalculateKills(GameState_CSGO csgostate)
    {
        var roundClearPhase = csgostate.Round.WinTeam == RoundWinTeam.Undefined &&
                              csgostate.Previously.Round.WinTeam != RoundWinTeam.Undefined;
        var respawned = csgostate.Player.State.Health == 100 &&
                        csgostate.Previously.Player.State.Health is > -1 and < 100 &&
                        csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID);
            
        if (csgostate.Player.State.RoundKills == 0 || roundClearPhase || respawned)
        {
            for (var i = 0; i < roundKills.Count; i++)
            {
                roundKills[i] = RoundKillType.None;
            }
        }

        if (csgostate.Previously.Player.State.RoundKills != -1 && csgostate.Player.State.RoundKills != -1 &&
            csgostate.Previously.Player.State.RoundKills < csgostate.Player.State.RoundKills &&
            csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
        {
            var index = csgostate.Player.State.RoundKills - 1;
            if (index >= roundKills.Count)
            {
                return;
            }
            if (csgostate.Previously.Player.State.RoundKillHS != -1 && csgostate.Player.State.RoundKillHS != -1 &&
                csgostate.Previously.Player.State.RoundKillHS < csgostate.Player.State.RoundKillHS)
                    
                roundKills[index] = RoundKillType.Headshot;
            else
                roundKills[index] = RoundKillType.Regular;
        }

        _lastCountedKill = csgostate.Player.State.RoundKills;
    }

    public override void SetApplication(Application profile)
    {
        (Control as Control_CSGOKillIndicatorLayer).SetProfile(profile);
        base.SetApplication(profile);
    }
}