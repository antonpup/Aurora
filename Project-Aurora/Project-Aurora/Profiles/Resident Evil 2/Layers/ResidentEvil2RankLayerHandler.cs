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
    public class ResidentEvil2RankLayerHandlerProperties : LayerHandlerProperties2Color<ResidentEvil2RankLayerHandlerProperties>
    {
        public ResidentEvil2RankLayerHandlerProperties() : base() { }

        public ResidentEvil2RankLayerHandlerProperties(bool assign_default = false) : base( assign_default ) { }

        public override void Default()
        {
            base.Default();
            this._Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE,
                            Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE
                        });
        }

    }

    public class ResidentEvil2RankLayerHandler : LayerHandler<ResidentEvil2RankLayerHandlerProperties>
    {
        public ResidentEvil2RankLayerHandler() : base()
        {
            _ID = "ResidentEvil2Rank";
        }

        protected override UserControl CreateControl()
        {
            return new Control_ResidentEvil2RankLayer( this );
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer keys_layer = new EffectLayer( "Resident Evil 2 - Rank" );

            if (state is GameState_ResidentEvil2)
            {
                GameState_ResidentEvil2 re2state = state as GameState_ResidentEvil2;

                if (re2state.Player.Status != Player_ResidentEvil2.PlayerStatus.OffGame && re2state.Player.Rank != 0)
                {
                    keys_layer.Set(Properties.Sequence.keys[re2state.Player.Rank - 1], Color.White);
                }
            }
            return keys_layer;
        }

        public override void SetApplication(Application profile)
        {
            ( Control as Control_ResidentEvil2RankLayer ).SetProfile( profile );
            base.SetApplication( profile );
        }
    }
}