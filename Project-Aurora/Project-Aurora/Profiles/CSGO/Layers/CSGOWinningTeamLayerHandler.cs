using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.CSGO.Layers
{
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
        private readonly EffectLayer _effectLayer = new("CSGO - Winning Team Effect");
        
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

        public CSGOWinningTeamLayerHandler()
        {
            _animationMix = new AnimationMix(_tracks);
        }

        protected override UserControl CreateControl()
        {
            return new Control_CSGOWinningTeamLayer(this);
        }

        private bool _empty = true;
        public override EffectLayer Render(IGameState state)
        {
            _previoustime = _currenttime;
            _currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            if (state is not GameState_CSGO csgostate) return _effectLayer;

            // Block animations after end of round
            if (csgostate.Map.Phase == MapPhase.Undefined || csgostate.Round.Phase != RoundPhase.Over)
            {
                if (!_empty)
                {
                    _effectLayer.Clear();
                    _empty = true;
                }
                return _effectLayer;
            }

            var color = Color.White;

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
                        color = Properties.TColor;
                    }
                    else if (ctScore > tScore)
                    {
                        color = Properties.CTColor;
                    }
                }
                else
                {
                    color = csgostate.Round.WinTeam switch
                    {
                        // End of round
                        RoundWinTeam.T => Properties.TColor,
                        RoundWinTeam.CT => Properties.CTColor,
                        _ => color
                    };
                }

                SetTracks(color);
                _animationMix.Clear();
                _showAnimation = true;
            }

            if (!_showAnimation) return _effectLayer;



            _empty = false;
            _effectLayer.Fill(color);
            _animationMix.Draw(_effectLayer.GetGraphics(), _winningTeamEffectKeyframe);
            _winningTeamEffectKeyframe += (_currenttime - _previoustime) / 1000.0f;

            if (_winningTeamEffectKeyframe >= WinningTeamEffectAnimationTime)
            {
                _showAnimation = false;
                _winningTeamEffectKeyframe = 0;
            }

            return _effectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOWinningTeamLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        private void SetTracks(Color playerColor)
        {
            for (int i = 0; i < _tracks.Length; i++)
            {
                _tracks[i].SetFrame(
                    0.0f,
                    new AnimationCircle(
                        (int)(Effects.canvas_width_center * 0.9),
                        Effects.canvas_height_center,
                        0,
                        Color.Black,
                        4)
                );

                _tracks[i].SetFrame(
                    1.0f,
                    new AnimationCircle(
                        (int)(Effects.canvas_width_center * 0.9),
                        Effects.canvas_height_center,
                        Effects.canvas_biggest / 2.0f,
                        Color.Black,
                        4)
                );
            }
        }
    }
}