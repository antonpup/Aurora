using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.GSI.Nodes;
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

namespace Aurora.Profiles.ResidentEvil2.Layers
{
    public class ResidentEvil2HealthLayerHandlerProperties : LayerHandlerProperties2Color<ResidentEvil2HealthLayerHandlerProperties>
    {
        public ResidentEvil2HealthLayerHandlerProperties() : base() { }

        public ResidentEvil2HealthLayerHandlerProperties(bool assign_default = false) : base( assign_default ) { }

        public override void Default()
        {
            base.Default();
        }

    }

    public class ResidentEvil2HealthLayerHandler : LayerHandler<ResidentEvil2HealthLayerHandlerProperties>
    {
        public ResidentEvil2HealthLayerHandler() : base()
        {
            _ID = "ResidentEvil2Health";
        }

        protected override UserControl CreateControl()
        {
            return new Control_ResidentEvil2HealthLayer( this );
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer( "Resident Evil 2 - Health" );

            if (state is GameState_ResidentEvil2)
            {
                GameState_ResidentEvil2 re2state = state as GameState_ResidentEvil2;

                switch (re2state.Player.Status)
                {
                    case Player_ResidentEvil2.PlayerStatus.Fine:
                        bg_layer.Fill(Color.Green);
                        break;
                    case Player_ResidentEvil2.PlayerStatus.LiteFine:
                        bg_layer.Fill(Color.YellowGreen);
                        break;
                    case Player_ResidentEvil2.PlayerStatus.Caution:
                        bg_layer.Fill(Color.Gold);
                        break;
                    case Player_ResidentEvil2.PlayerStatus.Danger:
                        bg_layer.Fill(Color.Red);
                        break;
                    case Player_ResidentEvil2.PlayerStatus.Dead:
                        bg_layer.Fill(Color.DarkGray);
                        break;
                    default:
                        bg_layer.Fill(Color.DarkSlateBlue);
                        break;
                }
            }
            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            ( Control as Control_ResidentEvil2HealthLayer ).SetProfile( profile );
            base.SetApplication( profile );
        }
    }
}