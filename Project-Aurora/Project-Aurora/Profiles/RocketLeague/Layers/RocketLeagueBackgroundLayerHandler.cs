using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.RocketLeague.Layers
{
    public class RocketLeagueBackgroundLayerHandlerProperties : LayerHandlerProperties2Color<RocketLeagueBackgroundLayerHandlerProperties>
    {
        public Color? _DefaultColor { get; set; }

        [JsonIgnore]
        public Color DefaultColor { get { return Logic._DefaultColor ?? _DefaultColor ?? Color.Empty; } }

        public Color? _BlueColor { get; set; }

        [JsonIgnore]
        public Color BlueColor { get { return Logic._BlueColor ?? _BlueColor ?? Color.Empty; } }

        public Color? _OrangeColor { get; set; }

        [JsonIgnore]
        public Color OrangeColor { get { return Logic._OrangeColor ?? _OrangeColor ?? Color.Empty; } }

        public bool? _ShowTeamScoreSplit { get; set; }

        [JsonIgnore]
        public bool ShowTeamScoreSplit { get { return Logic._ShowTeamScoreSplit ?? _ShowTeamScoreSplit ?? false; } }

        public bool? _ShowGoalExplosion { get; set; }

        [JsonIgnore]
        public bool ShowGoalExplosion { get { return Logic._ShowGoalExplosion ?? _ShowGoalExplosion ?? false; } }

        public bool? _ShowEnemyExplosion { get; set; }

        [JsonIgnore]
        public bool ShowEnemyExplosion { get { return Logic._ShowEnemyExplosion ?? _ShowEnemyExplosion ?? false; } }


        public RocketLeagueBackgroundLayerHandlerProperties() : base() { }

        public RocketLeagueBackgroundLayerHandlerProperties(bool assign_default = false) : base( assign_default ) { }

        public override void Default()
        {
            base.Default();

            this._DefaultColor = Color.FromArgb( 140, 190, 230 );
            this._OrangeColor = Color.Orange;
            this._BlueColor = Color.Blue;
            this._ShowTeamScoreSplit = true;
            this._ShowGoalExplosion = true;
            this._ShowEnemyExplosion = true;
        }

    }

    public class RocketLeagueBackgroundLayerHandler : LayerHandler<RocketLeagueBackgroundLayerHandlerProperties>
    {
        private AnimationTrack goal_explosion_track = new AnimationTrack( "Goal Explosion Track", 1.0f, 0.0f );
        private AnimationTrack goal_explosion_track_1 = new AnimationTrack( "Goal Explosion Track_1", 1.0f, 0.5f );
        private AnimationTrack goal_explosion_track_2 = new AnimationTrack( "Goal Explosion Track_2", 1.0f, 1.0f );
        private AnimationTrack goal_explosion_track_3 = new AnimationTrack( "Goal Explosion Track_3", 1.0f, 1.5f );
        private AnimationTrack goal_explosion_track_4 = new AnimationTrack( "Goal Explosion Track_4", 1.0f, 2.0f );


        private long previoustime = 0;
        private long currenttime = 0;

        private static float goalEffect_keyframe = 0.0f;
        private const float goalEffect_animationTime = 3.0f;

        private bool showAnimation_Explosion = false;

        public RocketLeagueBackgroundLayerHandler() : base()
        {
            _ID = "RocketLeagueBackground";
        }

        protected override UserControl CreateControl()
        {
            return new Control_RocketLeagueBackgroundLayer( this );
        }

        public override EffectLayer Render(IGameState state)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            EffectLayer bg_layer = new EffectLayer( "Rocket League - Background" );
            AnimationMix goal_explosion_mix = new AnimationMix();
            Color playerColor = new Color();
            Color enemyColor = new Color();


            if (state is GameState_RocketLeague)
            {
                GameState_RocketLeague rlstate = state as GameState_RocketLeague;

                switch (rlstate.Player.Team)
                {
                    case PlayerTeam.Blue:
                        bg_layer.Fill(Properties.BlueColor);
                        playerColor = Properties.BlueColor;
                        enemyColor = Properties.OrangeColor;
                        break;
                    case PlayerTeam.Orange:
                        bg_layer.Fill(Properties.OrangeColor);
                        playerColor = Properties.OrangeColor;
                        enemyColor = Properties.BlueColor;
                        break;
                    default:
                        bg_layer.Fill(Properties.DefaultColor);
                        playerColor = Properties.DefaultColor;
                        enemyColor = Properties.DefaultColor;
                        break;
                }

                if (Properties.ShowTeamScoreSplit)
                {

                    if (rlstate.Match.BlueTeam_Score != 0 || rlstate.Match.OrangeTeam_Score != 0)
                    {
                        int total_score = rlstate.Match.BlueTeam_Score + rlstate.Match.OrangeTeam_Score;


                        LinearGradientBrush the__split_brush = new LinearGradientBrush(
                                new Point(0, 0),
                                new Point(Effects.canvas_biggest, 0),
                                Color.Red, Color.Red);
                        Color[] colors = new Color[]
                        {
                                Properties.OrangeColor, //Orange //Team 1
                                Properties.OrangeColor, //Orange "Line"
                                Properties.BlueColor, //Blue "Line" //Team 2
                                Properties.BlueColor  //Blue
                        };
                        int num_colors = colors.Length;
                        float[] blend_positions = new float[num_colors];

                        if (rlstate.Match.OrangeTeam_Score > rlstate.Match.BlueTeam_Score)
                        {
                            blend_positions[0] = 0.0f;
                            blend_positions[1] = ((float)rlstate.Match.OrangeTeam_Score / (float)total_score) - 0.01f;
                            blend_positions[2] = ((float)rlstate.Match.OrangeTeam_Score / (float)total_score) + 0.01f;
                            blend_positions[3] = 1.0f;
                        }
                        else if (rlstate.Match.OrangeTeam_Score < rlstate.Match.BlueTeam_Score)
                        {
                            blend_positions[0] = 0.0f;
                            blend_positions[1] = (1.0f - ((float)rlstate.Match.BlueTeam_Score / (float)total_score)) - 0.01f;
                            blend_positions[2] = (1.0f - ((float)rlstate.Match.BlueTeam_Score / (float)total_score)) + 0.01f;
                            blend_positions[3] = 1.0f;
                        }
                        else
                        {
                            blend_positions[0] = 0.0f;
                            blend_positions[1] = 0.49f;
                            blend_positions[2] = 0.51f;
                            blend_positions[3] = 1.0f;
                        }

                        ColorBlend color_blend = new ColorBlend();
                        color_blend.Colors = colors;
                        color_blend.Positions = blend_positions;
                        the__split_brush.InterpolationColors = color_blend;

                        bg_layer.Fill(the__split_brush);
                    }
                }

                if (Properties.ShowGoalExplosion)
                {
                    if (rlstate.Match.YourTeam_LastScore < (rlstate.Player.Team == PlayerTeam.Blue ? rlstate.Match.BlueTeam_Score
                                                                                     : rlstate.Match.OrangeTeam_Score))
                    {
                        goal_explosion_track.SetFrame(0.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, playerColor, 4)
                        );
                        goal_explosion_track.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, playerColor, 4)
                        );

                        goal_explosion_track_1.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, playerColor, 4)
                       );
                        goal_explosion_track_1.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, playerColor, 4)
                        );

                        goal_explosion_track_2.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, playerColor, 4)
                       );
                        goal_explosion_track_2.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, playerColor, 4)
                        );

                        goal_explosion_track_3.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, playerColor, 4)
                       );
                        goal_explosion_track_3.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, playerColor, 4)
                        );

                        goal_explosion_track_4.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, playerColor, 4)
                       );
                        goal_explosion_track_4.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, playerColor, 4)
                        );

                        goal_explosion_mix.Clear();
                        showAnimation_Explosion = true;
                    }
                }

                if (Properties.ShowEnemyExplosion)
                {
                    if (rlstate.Match.EnemyTeam_LastScore < (rlstate.Player.Team == PlayerTeam.Orange ? rlstate.Match.BlueTeam_Score
                                                                                 : rlstate.Match.OrangeTeam_Score))
                    {
                        goal_explosion_track.SetFrame(0.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, enemyColor, 4)
                        );
                        goal_explosion_track.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, enemyColor, 4)
                        );

                        goal_explosion_track_1.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, enemyColor, 4)
                       );
                        goal_explosion_track_1.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, enemyColor, 4)
                        );

                        goal_explosion_track_2.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, enemyColor, 4)
                       );
                        goal_explosion_track_2.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, enemyColor, 4)
                        );

                        goal_explosion_track_3.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, enemyColor, 4)
                       );
                        goal_explosion_track_3.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, enemyColor, 4)
                        );

                        goal_explosion_track_4.SetFrame(0.0f,
                           new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, 0, enemyColor, 4)
                       );
                        goal_explosion_track_4.SetFrame(1.0f,
                            new AnimationCircle((int)(Effects.canvas_width_center * 0.9), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, enemyColor, 4)
                        );

                        goal_explosion_mix.Clear();
                        showAnimation_Explosion = true;
                    }
                }

                if (showAnimation_Explosion)
                {
                    bg_layer.Fill(Color.FromArgb(0, 0, 0));
                    goal_explosion_mix.AddTrack(goal_explosion_track);
                    goal_explosion_mix.AddTrack(goal_explosion_track_1);
                    goal_explosion_mix.AddTrack(goal_explosion_track_2);
                    goal_explosion_mix.AddTrack(goal_explosion_track_3);
                    goal_explosion_mix.AddTrack(goal_explosion_track_4);

                    goal_explosion_mix.Draw(bg_layer.GetGraphics(), goalEffect_keyframe);
                    goalEffect_keyframe += (currenttime - previoustime) / 1000.0f;

                    if (goalEffect_keyframe >= goalEffect_animationTime)
                    {
                        showAnimation_Explosion = false;
                        goalEffect_keyframe = 0;
                    }
                }
            }
            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            ( Control as Control_RocketLeagueBackgroundLayer ).SetProfile( profile );
            base.SetApplication( profile );
        }
    }
}