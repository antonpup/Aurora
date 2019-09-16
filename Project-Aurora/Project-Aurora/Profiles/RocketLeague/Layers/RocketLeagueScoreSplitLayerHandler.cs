using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using System.Windows.Controls;
using Aurora.Profiles.RocketLeague.GSI;
using System.Drawing.Drawing2D;
using Aurora.Profiles.RocketLeague.GSI.Nodes;

namespace Aurora.Profiles.RocketLeague.Layers
{
    public class RocketLeagueScoreSplitLayerProperties : LayerHandlerProperties<RocketLeagueScoreSplitLayerProperties>
    {
        public RocketLeagueScoreSplitLayerProperties() : base()
        {

        }

        public RocketLeagueScoreSplitLayerProperties(bool arg = false) : base(arg)
        {

        }

        public override void Default()
        {
            base.Default();
        }
    }

    public class RocketLeagueScoreSplitLayerHandler : LayerHandler<RocketLeagueScoreSplitLayerProperties>
    {
        public RocketLeagueScoreSplitLayerHandler() : base()
        {
            _ID = "RocketLeagueScoreSplit";
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer layer = new EffectLayer("Score Split");

            if (gamestate is GameState_RocketLeague)
            {
                GameState_RocketLeague rlstate = gamestate as GameState_RocketLeague;

                if (rlstate.Match.Blue.Goals >= 0 && rlstate.Match.Orange.Goals >= 0 && ColorsValid(rlstate))
                {
                    Color blue = GetTeamColor(rlstate.Match.Blue);
                    Color orange = GetTeamColor(rlstate.Match.Orange);

                    int total_score = rlstate.Match.Blue.Goals + rlstate.Match.Orange.Goals;

                    LinearGradientBrush the_split_brush = new LinearGradientBrush(
                            new Point(0, 0),
                            new Point(Effects.canvas_biggest, 0),
                            Color.Red, Color.Red);

                    Color[] colors = new Color[]
                    {
                        blue,
                        blue,
                        orange,
                        orange
                    };
                    int num_colors = colors.Length;
                    float[] blend_positions = new float[num_colors];

                    if (rlstate.Match.Orange.Goals > rlstate.Match.Blue.Goals)
                    {
                        blend_positions[0] = 0.0f;
                        blend_positions[1] = (1.0f - ((float)rlstate.Match.Orange.Goals / (float)total_score)) - 0.01f;
                        blend_positions[2] = (1.0f - ((float)rlstate.Match.Orange.Goals / (float)total_score)) + 0.01f;
                        blend_positions[3] = 1.0f;
                    }
                    else if (rlstate.Match.Orange.Goals < rlstate.Match.Blue.Goals)
                    {
                        blend_positions[0] = 0.0f;
                        blend_positions[1] = ((float)rlstate.Match.Blue.Goals / (float)total_score) - 0.01f;
                        blend_positions[2] = ((float)rlstate.Match.Blue.Goals / (float)total_score) + 0.01f;
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
                    the_split_brush.InterpolationColors = color_blend;

                    layer.Fill(the_split_brush);
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
            return new Control_RocketLeagueScoreSplitLayer();
        }

        private static bool ColorsValid(GameState_RocketLeague state)
        {
            return (state.Match.Orange.Red   >= 0 && state.Match.Blue.Red   <= 1) &&
                   (state.Match.Orange.Green >= 0 && state.Match.Blue.Green <= 1) &&
                   (state.Match.Orange.Blue  >= 0 && state.Match.Blue.Blue  <= 1) &&
                   (state.Match.Orange.Red   >= 0 && state.Match.Blue.Red   <= 1) &&
                   (state.Match.Orange.Green >= 0 && state.Match.Blue.Green <= 1) &&
                   (state.Match.Orange.Blue  >= 0 && state.Match.Blue.Blue  <= 1);
        }

        private Color GetTeamColor(Team_RocketLeague team)
        {
            return Color.FromArgb(
            (int)(team.Red * 255.0f),
            (int)(team.Green * 255.0f),
            (int)(team.Blue * 255.0f));
        }
    }
}
