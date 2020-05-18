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
        public Color CTColor { get { return Logic._CTColor ?? _CTColor ?? Color.Empty; } }

        public Color? _TColor { get; set; }

        [JsonIgnore]
        public Color TColor { get { return Logic._TColor ?? _TColor ?? Color.Empty; } }

        public CSGOWinningTeamLayerHandlerProperties() : base() { }

        public CSGOWinningTeamLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._CTColor = Color.FromArgb(33, 155, 221);
            this._TColor = Color.FromArgb(221, 99, 33);
        }

    }

    public class CSGOWinningTeamLayerHandler : LayerHandler<CSGOWinningTeamLayerHandlerProperties>
    {
        private readonly AnimationTrack[] tracks =
        {
            new AnimationTrack("Winning Team Track 0", 1.0f, 0.0f),
            new AnimationTrack("Winning Team Track 1", 1.0f, 1.0f),
            new AnimationTrack("Winning Team Track 2", 1.0f, 2.0f),
            new AnimationTrack("Winning Team Track 3", 1.0f, 3.0f),
            new AnimationTrack("Winning Team Track 4", 1.0f, 4.0f)
        };

        private long previoustime = 0;
        private long currenttime = 0;

        private static float winningTeamEffect_Keyframe = 0.0f;
        private const float winningTeamEffect_AnimationTime = 5.0f;

        private bool showAnimation = false;

        protected override UserControl CreateControl()
        {
            return new Control_CSGOWinningTeamLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            EffectLayer effectLayer = new EffectLayer("CSGO - Winning Team Effect");
            AnimationMix animationMix = new AnimationMix();

            if (state is GameState_CSGO)
            {
                GameState_CSGO csgostate = state as GameState_CSGO;
                
                // Block animations after end of round
                if (csgostate.Map.Phase == MapPhase.Undefined || csgostate.Round.Phase != RoundPhase.Over)
                {
                    return effectLayer;
                }

                Color color = Color.White;

                // Triggers directly after a team wins a round
                if (csgostate.Round.WinTeam != RoundWinTeam.Undefined && csgostate.Previously.Round.WinTeam == RoundWinTeam.Undefined)
                {
                    // Determine round or game winner
                    if (csgostate.Map.Phase == MapPhase.GameOver)
                    {
                        // End of match
                        int tScore = csgostate.Map.TeamT.Score;
                        int ctScore = csgostate.Map.TeamCT.Score;

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
                        // End of round
                        if (csgostate.Round.WinTeam == RoundWinTeam.T) color = Properties.TColor;
                        if (csgostate.Round.WinTeam == RoundWinTeam.CT) color = Properties.CTColor;
                    }

                    this.SetTracks(color);
                    animationMix.Clear();
                    showAnimation = true;
                }

                if (showAnimation)
                {
                    animationMix = new AnimationMix(tracks);

                    effectLayer.Fill(color);
                    animationMix.Draw(effectLayer.GetGraphics(), winningTeamEffect_Keyframe);
                    winningTeamEffect_Keyframe += (currenttime - previoustime) / 1000.0f;

                    if (winningTeamEffect_Keyframe >= winningTeamEffect_AnimationTime)
                    {
                        showAnimation = false;
                        winningTeamEffect_Keyframe = 0;
                    }
                }
            }

            return effectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOWinningTeamLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        private void SetTracks(Color playerColor)
        {
            for (int i = 0; i < tracks.Length; i++)
            {
                tracks[i].SetFrame(
                    0.0f,
                    new AnimationCircle(
                        (int)(Effects.canvas_width_center * 0.9),
                        Effects.canvas_height_center,
                        0,
                        Color.Black,
                        4)
                );

                tracks[i].SetFrame(
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