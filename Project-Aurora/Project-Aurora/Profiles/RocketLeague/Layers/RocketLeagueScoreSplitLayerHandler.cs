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
                    var myTeam = rlstate.Player.Team == 0 ? rlstate.Match.Blue : rlstate.Match.Orange;
                    var enemyTeam = rlstate.Player.Team == 1 ? rlstate.Match.Blue : rlstate.Match.Orange;

                    Color[] blendColors = new Color[]
                    {
                        GetTeamColor(myTeam),
                        GetTeamColor(myTeam),
                        GetTeamColor(enemyTeam),
                        GetTeamColor(enemyTeam)
                    };

                    float goalRatio;

                    if (myTeam.Goals == 0 && enemyTeam.Goals == 0)
                        goalRatio = 0.50f;
                    else
                        goalRatio = (float)myTeam.Goals / (myTeam.Goals + enemyTeam.Goals);

                    float[] blendPositions = new float[4]
                    {
                        0.0f,
                        goalRatio - 0.01f,
                        goalRatio + 0.01f,
                        1.0f
                    };

                    ColorBlend blend = new ColorBlend
                    {
                        Colors = blendColors,
                        Positions = blendPositions
                    };

                    LinearGradientBrush brush = new LinearGradientBrush(
                            new Point(0, 0), new Point(Effects.canvas_biggest, 0),
                            Color.Empty, Color.Empty);

                    brush.InterpolationColors = blend;

                    layer.Fill(brush);
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
