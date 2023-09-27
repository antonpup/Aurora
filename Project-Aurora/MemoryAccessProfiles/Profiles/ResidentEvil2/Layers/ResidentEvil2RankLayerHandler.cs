using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Common.Devices;
using MemoryAccessProfiles.Profiles.ResidentEvil2.GSI;
using MemoryAccessProfiles.Profiles.ResidentEvil2.GSI.Nodes;

namespace MemoryAccessProfiles.Profiles.ResidentEvil2.Layers;

public class ResidentEvil2RankLayerHandlerProperties : LayerHandlerProperties2Color<ResidentEvil2RankLayerHandlerProperties>
{
    public ResidentEvil2RankLayerHandlerProperties() : base() { }

    public ResidentEvil2RankLayerHandlerProperties(bool assign_default = false) : base( assign_default ) { }

    public override void Default()
    {
        base.Default();
        this._Sequence = new KeySequence(new DeviceKeys[] {
            DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE,
            DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE
        });
    }

}

public class ResidentEvil2RankLayerHandler : LayerHandler<ResidentEvil2RankLayerHandlerProperties>
{
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
                keys_layer.Set(Properties.Sequence.Keys[re2state.Player.Rank - 1], Color.White);
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