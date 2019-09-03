using Aurora.EffectsEngine;
using Aurora.Profiles;

namespace Aurora.Settings.Layers
{
    public class SolidColorLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        public SolidColorLayerHandler()
        {
            _ID = "Solid";
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer solidcolor_layer = new EffectLayer();
            solidcolor_layer.Set(Properties.Sequence, Properties.PrimaryColor);
            return solidcolor_layer;
        }
    }
}
