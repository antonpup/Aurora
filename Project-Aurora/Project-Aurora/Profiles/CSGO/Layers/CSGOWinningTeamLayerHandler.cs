using System;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.CSGO.Layers;

public class CSGOWinningTeamLayerHandlerProperties : LayerHandlerProperties2Color<CSGOWinningTeamLayerHandlerProperties>
{
    public Color? _CTColor { get; set; }

    [JsonIgnore]
    public Color CTColor => Logic._CTColor ?? _CTColor ?? Color.Empty;

    public Color? _TColor { get; set; }

    [JsonIgnore]
    public Color TColor => Logic._TColor ?? _TColor ?? Color.Empty;

    public CSGOWinningTeamLayerHandlerProperties() : base() { }

    public CSGOWinningTeamLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();

        _CTColor = Color.FromArgb(33, 155, 221);
        _TColor = Color.FromArgb(221, 99, 33);
    }

}

public class CSGOWinningTeamLayerHandler : LayerHandler<CSGOWinningTeamLayerHandlerProperties>
{
    private readonly AnimationTrack[] _tracks =
    {
        new("Winning Team Track 0", 1.0f),
        new("Winning Team Track 1", 1.0f, 1.0f),
        new("Winning Team Track 2", 1.0f, 2.0f),
        new("Winning Team Track 3", 1.0f, 3.0f),
        new("Winning Team Track 4", 1.0f, 4.0f)
    };
    private readonly AnimationMix _animationMix;

    private long _previoustime;
    private long _currenttime ;

    private static float _winningTeamEffectKeyframe;
    private const float WinningTeamEffectAnimationTime = 5.0f;

    private bool _showAnimation;

    private readonly SolidBrush _solidBrush = new(Color.Empty);

    public CSGOWinningTeamLayerHandler(): base("CSGO - Winning Team Effect")
    {
        WeakEventManager<Effects, EventArgs>.AddHandler(null, nameof(Effects.CanvasChanged), Effects_CanvasChanged);
        _animationMix = new AnimationMix(_tracks);
        SetTracks();
    }

    private void Effects_CanvasChanged(object? sender, EventArgs e)
    {
        SetTracks();
    }

    protected override UserControl CreateControl()
    {
        return new Control_CSGOWinningTeamLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        _previoustime = _currenttime;
        _currenttime = Utils.Time.GetMillisecondsSinceEpoch();

        if (state is not GameState_CSGO csgostate) return EffectLayer.EmptyLayer;

        // Block animations after end of round
        if (csgostate.Map.Phase == MapPhase.Undefined || csgostate.Round.Phase != RoundPhase.Over)
        {
            return EffectLayer.EmptyLayer;
        }

        _solidBrush.Color = Color.White;

        // Triggers directly after a team wins a round
        if (csgostate.Round.WinTeam != RoundWinTeam.Undefined && csgostate.Previously.Round.WinTeam == RoundWinTeam.Undefined)
        {
            // Determine round or game winner
            if (csgostate.Map.Phase == MapPhase.GameOver)
            {
                // End of match
                var tScore = csgostate.Map.TeamT.Score;
                var ctScore = csgostate.Map.TeamCT.Score;

                if (tScore > ctScore)
                {
                    _solidBrush.Color = Properties.TColor;
                }
                else if (ctScore > tScore)
                {
                    _solidBrush.Color = Properties.CTColor;
                }
            }
            else
            {
                _solidBrush.Color = csgostate.Round.WinTeam switch
                {
                    // End of round
                    RoundWinTeam.T => Properties.TColor,
                    RoundWinTeam.CT => Properties.CTColor,
                    _ => _solidBrush.Color
                };
            }

            _animationMix.Clear();
            _showAnimation = true;
        }

        if (!_showAnimation) return EffectLayer;

        EffectLayer.Fill(_solidBrush);
        _animationMix.Draw(EffectLayer.GetGraphics(), _winningTeamEffectKeyframe);
        _winningTeamEffectKeyframe += (_currenttime - _previoustime) / 1000.0f;

        if (!(_winningTeamEffectKeyframe >= WinningTeamEffectAnimationTime)) return EffectLayer;
        _showAnimation = false;
        _winningTeamEffectKeyframe = 0;

        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        (Control as Control_CSGOWinningTeamLayer)?.SetProfile(profile);
        base.SetApplication(profile);
    }

    private void SetTracks()
    {
        foreach (var track in _tracks)
        {
            track.SetFrame(
                0.0f,
                new AnimationCircle(
                    (int)(Effects.CanvasWidthCenter * 0.9),
                    Effects.CanvasHeightCenter,
                    0,
                    Color.Black,
                    4)
            );

            track.SetFrame(
                1.0f,
                new AnimationCircle(
                    (int)(Effects.CanvasWidthCenter * 0.9),
                    Effects.CanvasHeightCenter,
                    Effects.CanvasBiggest / 2.0f,
                    Color.Black,
                    4)
            );
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        WeakEventManager<Effects, EventArgs>.RemoveHandler(null, nameof(Effects.CanvasChanged), Effects_CanvasChanged);
    }
}