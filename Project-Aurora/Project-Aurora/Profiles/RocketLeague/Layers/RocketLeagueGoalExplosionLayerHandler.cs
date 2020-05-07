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

        private readonly AnimationTrack[] tracks =
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

        public override EffectLayer Render(IGameState gamestate)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            EffectLayer layer = new EffectLayer("Goal Explosion");
            AnimationMix goal_explosion_mix = new AnimationMix();

            if (gamestate is GameState_RocketLeague)
            {
                var state = gamestate as GameState_RocketLeague;
                if (state.Game.Status == RLStatus.Undefined)
                    return layer;

                if (state.YourTeam.Goals == -1 || state.OpponentTeam.Goals == -1 || previousOwnTeamGoals > state.YourTeam.Goals || previousOpponentGoals > state.OpponentTeam.Goals)
                {
                    //reset goals when game ends
                    previousOwnTeamGoals = 0;
                    previousOpponentGoals = 0;
                }

                if (state.YourTeam.Goals > previousOwnTeamGoals)//keep track of goals even if we dont play the animation
                {
                    previousOwnTeamGoals = state.YourTeam.Goals;
                    if (Properties.ShowFriendlyGoalExplosion && state.ColorsValid())
                    {
                        Color playerColor = state.YourTeam.TeamColor;
                        this.SetTracks(playerColor);
                        goal_explosion_mix.Clear();
                        showAnimation_Explosion = true;
                    }
                }

                if(state.OpponentTeam.Goals > previousOpponentGoals)
                {
                    previousOpponentGoals = state.OpponentTeam.Goals;
                    if (Properties.ShowEnemyGoalExplosion && state.ColorsValid())
                    {
                        Color opponentColor = state.OpponentTeam.TeamColor;
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
            return new Control_RocketLeagueGoalExplosionLayer(this);
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
                        playerColor,
                        4)
                );
            }
        }
    }
}
