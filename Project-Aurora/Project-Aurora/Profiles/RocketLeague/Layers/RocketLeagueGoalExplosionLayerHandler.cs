using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.GSI.Nodes;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.RocketLeague.Layers
{
    public class RocketLeagueGoalExplosionProperties : LayerHandlerProperties<RocketLeagueGoalExplosionProperties>
    {
        public bool? _ShowFriendlyGoalExplosion { get; set; }

        [JsonIgnore]
        public bool ShowFriendlyGoalExplosion { get { return Logic._ShowFriendlyGoalExplosion ?? _ShowFriendlyGoalExplosion ?? true; } }

        public bool? _ShowEnemyGoalExplosion { get; set; }

        [JsonIgnore]
        public bool ShowEnemyGoalExplosion { get { return Logic._ShowEnemyGoalExplosion ?? _ShowEnemyGoalExplosion ?? true; } }

        public bool? _Background { get; set; }

        [JsonIgnore]
        public bool Background { get { return Logic._Background ?? _Background ?? true; } }

        public RocketLeagueGoalExplosionProperties() : base()
        {

        }

        public RocketLeagueGoalExplosionProperties(bool arg = false) : base(arg)
        {

        }

        public override void Default()
        {
            base.Default();
            _PrimaryColor = Color.FromArgb(125,0,0,0);
            _ShowEnemyGoalExplosion = true;
            _ShowFriendlyGoalExplosion = true;
            _Background = true;
        } 
    }

    public class RocketLeagueGoalExplosionLayerHandler : LayerHandler<RocketLeagueGoalExplosionProperties>
    {
        private int previousOwnTeamGoals = 0;
        private int previousOpponentGoals = 0;

        private AnimationTrack[] tracks =
        {
            new AnimationTrack("Goal Explosion Track 0", 1.0f, 0.0f),
            new AnimationTrack("Goal Explosion Track 1", 1.0f, 0.5f),
            new AnimationTrack("Goal Explosion Track 2", 1.0f, 1.0f),
            new AnimationTrack("Goal Explosion Track 3", 1.0f, 1.5f),
            new AnimationTrack("Goal Explosion Track 4", 1.0f, 2.0f)
        };

        private long previoustime = 0;
        private long currenttime = 0;

        private static float goalEffect_keyframe = 0.0f;
        private const float goalEffect_animationTime = 3.0f;

        private bool showAnimation_Explosion = false;
        public RocketLeagueGoalExplosionLayerHandler() : base()
        {
            _ID = "RocketLeagueGoalExplosion";
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            EffectLayer layer = new EffectLayer("Goal Explosion");
            AnimationMix goal_explosion_mix = new AnimationMix();

            if (gamestate is GameState_RocketLeague)
            {
                var state = gamestate as GameState_RocketLeague;
                var ownTeam = state.Player.Team == 0 ? state.Match.Blue : state.Match.Orange;
                var opponentTeam = state.Player.Team == 1 ? state.Match.Blue : state.Match.Orange;

                if (Properties.ShowFriendlyGoalExplosion)
                {
                    if(ownTeam.Goals > previousOwnTeamGoals && ColorsValid(state))//if goal happened
                    {
                        previousOwnTeamGoals = ownTeam.Goals;
                        Color playerColor = GetTeamColor(ownTeam);
                        this.SetTracks(playerColor);

                        goal_explosion_mix.Clear();
                        showAnimation_Explosion = true;
                    }
                }

                if (Properties.ShowEnemyGoalExplosion)
                {
                    if(opponentTeam.Goals > previousOpponentGoals && ColorsValid(state))
                    {
                        previousOpponentGoals = opponentTeam.Goals;
                        Color opponentColor = GetTeamColor(opponentTeam);
                        this.SetTracks(opponentColor);

                        goal_explosion_mix.Clear();
                        showAnimation_Explosion = true;
                    }
                }

                if (showAnimation_Explosion)
                {
                    if(Properties.Background)
                        layer.Fill(Properties.PrimaryColor);

                    goal_explosion_mix = new AnimationMix(tracks);

                    goal_explosion_mix.Draw(layer.GetGraphics(), goalEffect_keyframe);
                    goalEffect_keyframe += (currenttime - previoustime) / 1000.0f;

                    if (goalEffect_keyframe >= goalEffect_animationTime)
                    {
                        showAnimation_Explosion = false;
                        goalEffect_keyframe = 0;
                    }
                }
            }
            return layer;
        }

        public override void SetApplication(Application profile)
        {
            base.SetApplication(profile);
        }

        protected override UserControl CreateControl()
        {
            return new Control_RocketLeagueGoalExplosionLayer();
        }

        private static bool ColorsValid(GameState_RocketLeague state)
        {
            return (state.Match.Orange.Red >= 0 && state.Match.Blue.Red <= 1) &&
                   (state.Match.Orange.Green >= 0 && state.Match.Blue.Green <= 1) &&
                   (state.Match.Orange.Blue >= 0 && state.Match.Blue.Blue <= 1) &&
                   (state.Match.Orange.Red >= 0 && state.Match.Blue.Red <= 1) &&
                   (state.Match.Orange.Green >= 0 && state.Match.Blue.Green <= 1) &&
                   (state.Match.Orange.Blue >= 0 && state.Match.Blue.Blue <= 1);
        }

        private Color GetTeamColor(Team_RocketLeague team)
        {
            return Color.FromArgb(
            (int)(team.Red * 255.0f),
            (int)(team.Green * 255.0f),
            (int)(team.Blue * 255.0f));
        }

        private void SetTracks(Color playerColor)
        {
            for(int i = 0; i< tracks.Length; i++)
            {
                tracks[i].SetFrame(
                    0.0f, 
                    new AnimationCircle(
                        (int)(Effects.canvas_width_center * 0.9),
                        Effects.canvas_height_center,
                        0, 
                        playerColor,
                        4)
                );

                tracks[i].SetFrame(
                    1.0f,
                    new AnimationCircle(
                        (int)(Effects.canvas_width_center * 0.9),
                        Effects.canvas_height_center, 
                        Effects.canvas_biggest / 2.0f, 
                        playerColor, 4)
                );
            }
        }
    }
}
